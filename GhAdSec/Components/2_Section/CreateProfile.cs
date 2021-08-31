using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;
using System.Text.RegularExpressions;

using Grasshopper.Kernel.Parameters;
using GhAdSec.Parameters;
using System.Resources;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;
using GhAdSec.Helpers;
using Oasys.Profiles;
using UnitsNet;
using UnitsNet.GH;

namespace GhAdSec.Components
{
    /// <summary>
    /// Component to create AdSec profile
    /// </summary>
    public class CreateProfile : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("ea0741e5-905e-4ecb-8270-a584e3f99aa3");
        public CreateProfile()
          : base("Create Profile", "Profile", "Create Profile AdSec Section",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override string HtmlHelp_Source()
        {
            string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.Profiles.html";
            return help;
        }

        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.CreateProfile;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                Dictionary<string, Type> profileTypesInitial = GhAdSec.Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.Profiles");
                profileTypes = new Dictionary<string, Type>();
                foreach (KeyValuePair<string, Type> kvp in profileTypesInitial)
                {
                    // filter out IProfile, IPoint, IFlange, IWeb and ITrapezoidProfileAbstractInterface
                    if (!excludedInterfaces.Contains(kvp.Key))
                    {
                        // remove the "Profile" from name
                        string key = kvp.Key.Replace("Profile", "");
                        // rempove the "I" from name
                        key = key.Remove(0, 1);
                        // add whitespace in front of capital characters
                        StringBuilder name = new StringBuilder(key.Length * 2);
                        name.Append(key[0]);
                        for (int i = 1; i < key.Length; i++)
                        {
                            if (char.IsUpper(key[i]))
                                if ((key[i - 1] != ' ' && !char.IsUpper(key[i - 1])) ||
                                    (char.IsUpper(key[i - 1]) &&
                                     i < key.Length - 1 && !char.IsUpper(key[i + 1])))
                                    name.Append(' ');
                            name.Append(key[i]);
                        }
                        // add to final dictionary
                        profileTypes.Add(name.ToString(), kvp.Value);
                    }
                }
                if (selecteditems == null)
                {
                    // create a new list of selected items and add the first material type
                    selecteditems = new List<string>();
                    selecteditems.Add("Rectangle");
                }
                if (dropdownitems == null)
                {
                    // create a new list of selected items and add the first material type
                    dropdownitems = new List<List<string>>();
                    dropdownitems.Add(profileTypes.Keys.ToList());
                }

                // length
                dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
                selecteditems.Add(lengthUnit.ToString());

                IQuantity quantity = new UnitsNet.Length(0, lengthUnit);
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }

        public void SetSelected(int i, int j)
        {
            // input -1 to force update of catalogue sections
            bool updateCat = false;
            if (i == -1)
            {
                selecteditems[0] = "Catalogue";
                updateCat = true;
                i = 0;
            }
            else
            {
                // change selected item
                selecteditems[i] = dropdownitems[i][j];
            }

            if (selecteditems[0] == "Catalogue")
            {
                // update spacer description to match catalogue dropdowns
                spacerDescriptions = new List<string>(new string[]
                {
                    "Profile type", "Catalogue", "Type", "Profile"
                });

                // if FoldMode is not currently catalogue state, then we update all lists
                if (_mode != FoldMode.Catalogue | updateCat)
                {
                    // remove any existing selections
                    while (selecteditems.Count > 1)
                        selecteditems.RemoveAt(1);
                    
                    // set catalogue selection to all
                    catalogueIndex = -1;

                    catalogueNames = cataloguedata.Item1;
                    catalogueNumbers = cataloguedata.Item2;

                    // set types to all
                    typeIndex = -1;
                    // update typelist with all catalogues
                    typedata = SqlReader.GetTypesDataFromSQLite(catalogueIndex, Path.Combine(GhAdSec.AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);
                    typeNames = typedata.Item1;
                    typeNumbers = typedata.Item2;

                    // update section list to all types
                    sectionList = SqlReader.GetSectionsDataFromSQLite(typeNumbers, Path.Combine(GhAdSec.AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);

                    // filter by search pattern
                    filteredlist = new List<string>();
                    if (search == "")
                    {
                        filteredlist = sectionList;
                    }
                    else
                    {
                        for (int k = 0; k < sectionList.Count; k++)
                        {
                            if (sectionList[k].ToLower().Contains(search))
                            {
                                filteredlist.Add(sectionList[k]);
                            }
                            if (!search.Any(char.IsDigit))
                            {
                                string test = sectionList[k].ToString();
                                test = Regex.Replace(test, "[0-9]", string.Empty);
                                test = test.Replace(".", string.Empty);
                                test = test.Replace("-", string.Empty);
                                test = test.ToLower();
                                if (test.Contains(search))
                                {
                                    filteredlist.Add(sectionList[k]);
                                }
                            }
                        }
                    }

                    // update displayed selections to all
                    selecteditems.Add(catalogueNames[0]);
                    selecteditems.Add(typeNames[0]);
                    selecteditems.Add(filteredlist[0]);

                    // call graphics update
                    Mode1Clicked();
                }

                // update dropdown lists
                while (dropdownitems.Count > 1)
                    dropdownitems.RemoveAt(1);

                // add catalogues (they will always be the same so no need to rerun sql call)
                dropdownitems.Add(catalogueNames);

                // type list
                // if second list (i.e. catalogue list) is changed, update types list to account for that catalogue
                if (i == 1)
                {
                    // update catalogue index with the selected catalogue
                    catalogueIndex = catalogueNumbers[j];
                    selecteditems[1] = catalogueNames[j];

                    // update typelist with selected input catalogue
                    typedata = SqlReader.GetTypesDataFromSQLite(catalogueIndex, Path.Combine(GhAdSec.AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);
                    typeNames = typedata.Item1;
                    typeNumbers = typedata.Item2;

                    // update section list from new types (all new types in catalogue)
                    List<int> types = typeNumbers.ToList();
                    types.RemoveAt(0); // remove -1 from beginning of list
                    sectionList = SqlReader.GetSectionsDataFromSQLite(types, Path.Combine(GhAdSec.AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);

                    // filter by search pattern
                    filteredlist = new List<string>();
                    if (search == "")
                    {
                        filteredlist = sectionList;
                    }
                    else
                    {
                        for (int k = 0; k < sectionList.Count; k++)
                        {
                            if (sectionList[k].ToLower().Contains(search))
                            {
                                filteredlist.Add(sectionList[k]);
                            }
                            if (!search.Any(char.IsDigit))
                            {
                                string test = sectionList[k].ToString();
                                test = Regex.Replace(test, "[0-9]", string.Empty);
                                test = test.Replace(".", string.Empty);
                                test = test.Replace("-", string.Empty);
                                test = test.ToLower();
                                if (test.Contains(search))
                                {
                                    filteredlist.Add(sectionList[k]);
                                }
                            }
                        }
                    }

                    // update selections to display first item in new list
                    selecteditems[2] = typeNames[0];
                    selecteditems[3] = filteredlist[0];
                }
                dropdownitems.Add(typeNames);

                // section list
                // if third list (i.e. types list) is changed, update sections list to account for these section types
                
                if (i == 2)
                {
                    // update catalogue index with the selected catalogue
                    typeIndex = typeNumbers[j];
                    selecteditems[2] = typeNames[j];

                    // create type list
                    List<int> types = new List<int>();
                    if (typeIndex == -1) // if all
                    {
                        types = typeNumbers.ToList(); // use current selected list of type numbers
                        types.RemoveAt(0); // remove -1 from beginning of list
                    }
                    else
                        types = new List<int> { typeIndex }; // create empty list and add the single selected type 


                    // section list with selected types (only types in selected type)
                    sectionList = SqlReader.GetSectionsDataFromSQLite(types, Path.Combine(GhAdSec.AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);

                    // filter by search pattern
                    filteredlist = new List<string>();
                    if (search == "")
                    {
                        filteredlist = sectionList;
                    }
                    else
                    {
                        for (int k = 0; k < sectionList.Count; k++)
                        {
                            if (sectionList[k].ToLower().Contains(search))
                            {
                                filteredlist.Add(sectionList[k]);
                            }
                            if (!search.Any(char.IsDigit))
                            {
                                string test = sectionList[k].ToString();
                                test = Regex.Replace(test, "[0-9]", string.Empty);
                                test = test.Replace(".", string.Empty);
                                test = test.Replace("-", string.Empty);
                                test = test.ToLower();
                                if (test.Contains(search))
                                {
                                    filteredlist.Add(sectionList[i]);
                                }
                            }
                        }
                    }

                    // update selected section to be all
                    selecteditems[3] = filteredlist[0];
                }
                dropdownitems.Add(filteredlist);

                // selected profile
                // if fourth list (i.e. section list) is changed, updated the sections list to only be that single profile
                if (i == 3)
                {
                    // update displayed selected
                    selecteditems[3] = filteredlist[j];
                }
                profileString = selecteditems[3];
            }
            else
            {
                // update spacer description to match none-catalogue dropdowns
                spacerDescriptions = new List<string>(new string[]
                {
                    "Profile type", "Measure", "Type", "Profile"
                });

                
                Type typ = profileTypes[selecteditems[0]];
                Mode2Clicked();
            }
        }

        #endregion


        #region Input and output
        // list of lists with all dropdown lists conctent
        List<List<string>> dropdownitems;
        // list of selected items
        List<string> selecteditems;
        // list of descriptions 
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Profile type", "Measure", "Type", "Profile"
        });
        List<string> excludedInterfaces = new List<string>(new string[]
        {
        "IProfile", "IPoint", "IPolygon", "IFlange", "IWeb", "ITrapezoidProfileAbstractInterface", "IIBeamProfile"
        });
        Dictionary<string, Type> profileTypes;
        Dictionary<string, FieldInfo> profileFields;

        private UnitsNet.Units.LengthUnit lengthUnit = GhAdSec.DocumentUnits.LengthUnit;
        string unitAbbreviation;

        #region catalogue sections
        // for catalogue selection
        // Catalogues
        readonly Tuple<List<string>, List<int>> cataloguedata = SqlReader.GetCataloguesDataFromSQLite(Path.Combine(GhAdSec.AddReferencePriority.PluginPath, "sectlib.db3"));
        List<int> catalogueNumbers = new List<int>(); // internal db catalogue numbers
        List<string> catalogueNames = new List<string>(); // list of displayed catalogues
        bool inclSS;

        // Types
        Tuple<List<string>, List<int>> typedata = SqlReader.GetTypesDataFromSQLite(-1, Path.Combine(GhAdSec.AddReferencePriority.PluginPath, "sectlib.db3"), false);
        List<int> typeNumbers = new List<int>(); //  internal db type numbers
        List<string> typeNames = new List<string>(); // list of displayed types

        // Sections
        // list of displayed sections
        List<string> sectionList = SqlReader.GetSectionsDataFromSQLite(new List<int> { -1 }, Path.Combine(GhAdSec.AddReferencePriority.PluginPath, "sectlib.db3"), false);
        List<string> filteredlist = new List<string>();
        int catalogueIndex = -1; //-1 is all
        int typeIndex = -1;
        // displayed selections
        string typeName = "All";
        string sectionName = "All";
        // list of sections as outcome from selections
        string profileString = "HE HE200.B";
        string search = "";
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Width [" + unitAbbreviation + "]" , "B", "Profile width", GH_ParamAccess.item);
            pManager.AddGenericParameter("Depth [" + unitAbbreviation + "]", "H", "Profile depth", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Profile", "Pf", "Profile for AdSec Section", GH_ParamAccess.item);
        }
        #endregion
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region catalogue
            if (_mode == FoldMode.Catalogue)
            {
                // get user input filter search string
                bool incl = false;
                if (DA.GetData(1, ref incl))
                {
                    if (inclSS != incl)
                    {
                        SetSelected(-1, 0);
                        this.ExpireSolution(true);
                    }
                }

                // get user input filter search string
                string inSearch = "";
                if (DA.GetData(0, ref inSearch))
                {
                    inSearch = inSearch.ToLower();

                }
                if (!inSearch.Equals(search))
                {
                    search = inSearch.ToString();
                    SetSelected(-1, 0);
                    this.ExpireSolution(true);
                }

                AdSecProfileGoo catalogueProfile = new AdSecProfileGoo(ICatalogueProfile.Create("CAT " + profileString));
                Oasys.Collections.IList<Oasys.AdSec.IWarning> warn = catalogueProfile.Profile.Validate();
                foreach(Oasys.AdSec.IWarning warning in warn)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning.Description);
                DA.SetData(0, catalogueProfile);
                                
                return;
            }

            if (_mode == FoldMode.Other)
            {
                IProfile profile = null;
                // angle
                if (typ.GetType().ToString().Equals(typeof(IAngleProfile).ToString() + "_Implementation"))
                {
                    profile = IAngleProfile.Create(
                        GetUnitNumberInput(DA, 0), 
                        GetFlangeInput(DA, 1),
                        GetWebInput(DA, 2));
                }

                // channel
                else if (typ.GetType().ToString().Equals(typeof(IChannelProfile).ToString() + "_Implementation"))
                {
                    profile = IChannelProfile.Create(
                        GetUnitNumberInput(DA, 0),
                        GetFlangeInput(DA, 1),
                        (IWebConstant)GetWebInput(DA, 2));
                }

                // circle hollow
                else if (typ.GetType().ToString().Equals(typeof(ICircleHollowProfile).ToString() + "_Implementation"))
                {
                    ICircleHollowProfile circleHollow = (ICircleHollowProfile)typ;
                    profile = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
                }

                // circle
                else if (typ.GetType().ToString().Equals(typeof(ICircleProfile).ToString() + "_Implementation"))
                {
                    ICircleProfile circle = (ICircleProfile)typ;
                    profile = ICircleProfile.Create(circle.Diameter);
                }

                // ICruciformSymmetricalProfile
                else if (typ.GetType().ToString().Equals(typeof(ICruciformSymmetricalProfile).ToString() + "_Implementation"))
                {
                    ICruciformSymmetricalProfile cruciformSymmetrical = (ICruciformSymmetricalProfile)typ;
                    profile = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange, cruciformSymmetrical.Web);
                }

                // IEllipseHollowProfile
                else if (typ.GetType().ToString().Equals(typeof(IEllipseHollowProfile).ToString() + "_Implementation"))
                {
                    IEllipseHollowProfile ellipseHollow = (IEllipseHollowProfile)typ;
                    profile = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width, ellipseHollow.WallThickness);
                }

                // IEllipseProfile
                else if (typ.GetType().ToString().Equals(typeof(IEllipseProfile).ToString() + "_Implementation"))
                {
                    IEllipseProfile ellipse = (IEllipseProfile)typ;
                    profile = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
                }

                // IGeneralCProfile
                else if (typ.GetType().ToString().Equals(typeof(IGeneralCProfile).ToString() + "_Implementation"))
                {
                    IGeneralCProfile generalC = (IGeneralCProfile)typ;
                    profile = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
                }

                // IGeneralZProfile
                else if (typ.GetType().ToString().Equals(typeof(IGeneralZProfile).ToString() + "_Implementation"))
                {
                    IGeneralZProfile generalZ = (IGeneralZProfile)typ;
                    profile = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth, generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
                }

                // IIBeamAsymmetricalProfile
                else if (typ.GetType().ToString().Equals(typeof(IIBeamAsymmetricalProfile).ToString() + "_Implementation"))
                {
                    IIBeamAsymmetricalProfile iBeamAsymmetrical = (IIBeamAsymmetricalProfile)typ;
                    profile = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange, iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
                }

                // IIBeamCellularProfile
                else if (typ.GetType().ToString().Equals(typeof(IIBeamCellularProfile).ToString() + "_Implementation"))
                {
                    IIBeamCellularProfile iBeamCellular = (IIBeamCellularProfile)typ;
                    profile = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web, iBeamCellular.WebOpening);
                }

                // IIBeamSymmetricalProfile
                else if (typ.GetType().ToString().Equals(typeof(IIBeamSymmetricalProfile).ToString() + "_Implementation"))
                {
                    IIBeamSymmetricalProfile iBeamSymmetrical = (IIBeamSymmetricalProfile)typ;
                    profile = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges, iBeamSymmetrical.Web);
                }

                // IRectangleHollowProfile
                else if (typ.GetType().ToString().Equals(typeof(IRectangleHollowProfile).ToString() + "_Implementation"))
                {
                    IRectangleHollowProfile rectangleHollow = (IRectangleHollowProfile)typ;
                    profile = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges, rectangleHollow.Webs);
                }

                // IRectangleProfile
                else if (typ.GetType().ToString().Equals(typeof(IRectangleProfile).ToString() + "_Implementation"))
                {
                    IRectangleProfile rectangle = (IRectangleProfile)typ;
                    profile = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
                }

                // IRectoEllipseProfile
                else if (typ.GetType().ToString().Equals(typeof(IRectoEllipseProfile).ToString() + "_Implementation"))
                {
                    IRectoEllipseProfile rectoEllipse = (IRectoEllipseProfile)typ;
                    profile = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width, rectoEllipse.WidthFlat);
                }

                // ISecantPileProfile
                else if (typ.GetType().ToString().Equals(typeof(ISecantPileProfile).ToString() + "_Implementation"))
                {
                    ISecantPileProfile secantPile = (ISecantPileProfile)typ;
                    profile = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount, secantPile.IsWallNotSection);
                }

                // ISheetPileProfile
                else if (typ.GetType().ToString().Equals(typeof(ISheetPileProfile).ToString() + "_Implementation"))
                {
                    ISheetPileProfile sheetPile = (ISheetPileProfile)typ;
                    profile = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth, sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
                }

                // IStadiumProfile
                else if (typ.GetType().ToString().Equals(typeof(IStadiumProfile).ToString() + "_Implementation"))
                {
                    IStadiumProfile stadium = (IStadiumProfile)typ;
                    profile = IStadiumProfile.Create(stadium.Depth, stadium.Width);
                }

                // ITrapezoidProfile
                else if (typ.GetType().ToString().Equals(typeof(ITrapezoidProfile).ToString() + "_Implementation"))
                {
                    ITrapezoidProfile trapezoid = (ITrapezoidProfile)typ;
                    profile = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
                }

                // ITSectionProfile
                else if (typ.GetType().ToString().Equals(typeof(ITSectionProfile).ToString() + "_Implementation"))
                {
                    ITSectionProfile tSection = (ITSectionProfile)typ;
                    profile = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
                }

                // IPerimeterProfile (last chance...)
                else
                {
                    profile = IPerimeterProfile.Create(typ);
                }

                Oasys.Collections.IList<Oasys.AdSec.IWarning> warn = profile.Profile.Validate();
                foreach (Oasys.AdSec.IWarning warning in warn)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning.Description);
                DA.SetData(0, profile);
                return;
            }

            #endregion

        }

        private UnitsNet.Length GetUnitNumberInput(IGH_DataAccess DA, int inputid)
        {
            GH_UnitNumber unitNumber = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    unitNumber = (GH_UnitNumber)gh_typ.Value;
                    // check that unit is of right type
                    if (!unitNumber.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.LengthUnit)))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 1: Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + unitNumber.Value.QuantityInfo.Name + " but must be Length");
                        return UnitsNet.Length.Zero;
                    }
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    unitNumber = new GH_UnitNumber(new UnitsNet.Length(val, lengthUnit));
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input index " + inputid);
                    return UnitsNet.Length.Zero;
                }
            }
            else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input index " + inputid);
            return (UnitsNet.Length)unitNumber.Value;
        }
        private IFlange GetFlangeInput(IGH_DataAccess DA, int inputid)
        {
            AdSecProfileFlangeGoo flange = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecProfileFlangeGoo)
                {
                    flange = (AdSecProfileFlangeGoo)gh_typ.Value;
                    return flange.Value;
                }
                // try cast from web
                else if (gh_typ.Value is AdSecProfileWebGoo)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert flange to web; input index " + inputid);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input index " + inputid);
                    return null;
                }
            }
            else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input index " + inputid);
            return null;
        }
        private IWeb GetWebInput(IGH_DataAccess DA, int inputid)
        {
            AdSecProfileWebGoo web = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecProfileWebGoo)
                {
                    web = (AdSecProfileWebGoo)gh_typ.Value;
                    return web.Value;
                }
                // try cast from web
                else if (gh_typ.Value is AdSecProfileFlangeGoo)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert web to flange; input index " + inputid);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input index " + inputid);
                    return null;
                }
            }
            else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input index " + inputid);
            return null;
        }


        #region menu override
        private enum FoldMode
        {
            Catalogue,
            Other
        }
        private bool first = true;
        private FoldMode _mode = FoldMode.Other;


        private void Mode1Clicked()
        {
            FoldMode myMode = FoldMode.Catalogue;
            if (_mode == myMode)
                return;

            RecordUndoEvent(myMode.ToString() + " Parameter");

            //remove input parameters
            while (Params.Input.Count > 0)
                Params.UnregisterInputParameter(Params.Input[0], true);

            //register input parameter
            Params.RegisterInputParam(new Param_String());
            Params.RegisterInputParam(new Param_Boolean());

            _mode = myMode;

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        private void SetNumberOfGenericInputs(int inputs, bool isSecantPile = false)
        {
            numberOfInputs = inputs;

            // if last input previously was a bool and we no longer need that
            if (lastInputWasSecant && !isSecantPile)
            {
                // make sure to remove last param
                Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 2], true);
            }

            // remove any additional inputs
            while (Params.Input.Count > inputs)
                Params.UnregisterInputParameter(Params.Input[inputs], true);

            if (isSecantPile) // add two less generic than input says
            {
                inputs -= 2;
            }

            // add inputs parameter
            while (Params.Input.Count < inputs)
                Params.RegisterInputParam(new Param_GenericObject());

            if (isSecantPile) // finally add int and bool param if secant
            {
                Params.RegisterInputParam(new Param_Integer());
                Params.RegisterInputParam(new Param_Boolean());
                lastInputWasSecant = true;
            }
        }
        private bool lastInputWasSecant;
        private int numberOfInputs;
        Type typ;
        private void Mode2Clicked()
        {
            RecordUndoEvent("Dropdown changed");

            // check if mode is correct
            if (_mode != FoldMode.Other)
            {
                // if we come from catalogue mode remove all input parameters
                while (Params.Input.Count > 0)
                    Params.UnregisterInputParameter(Params.Input[0], true);

                // set mode to other
                _mode = FoldMode.Other;
            }

            // angle
            if (typ.ToString().Equals(typeof(IAngleProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(3);
                //dup = IAngleProfile.Create(angle.Depth, angle.Flange, angle.Web);
            }

            // channel
            else if (typ.ToString().Equals(typeof(IChannelProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(3);
                //dup = IChannelProfile.Create(channel.Depth, channel.Flanges, channel.Web);
            }

            // circle hollow
            else if (typ.ToString().Equals(typeof(ICircleHollowProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(2);
                //dup = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
            }

            // circle
            else if (typ.ToString().Equals(typeof(ICircleProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(1);
                //dup = ICircleProfile.Create(circle.Diameter);
            }

            // ICruciformSymmetricalProfile
            else if (typ.ToString().Equals(typeof(ICruciformSymmetricalProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(3);
                //dup = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange, cruciformSymmetrical.Web);
            }

            // IEllipseHollowProfile
            else if (typ.ToString().Equals(typeof(IEllipseHollowProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(3);
                //dup = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width, ellipseHollow.WallThickness);
            }

            // IEllipseProfile
            else if (typ.ToString().Equals(typeof(IEllipseProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(2);
                //dup = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
            }

            // IGeneralCProfile
            else if (typ.ToString().Equals(typeof(IGeneralCProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(4);
                //dup = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
            }

            // IGeneralZProfile
            else if (typ.ToString().Equals(typeof(IGeneralZProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(6);
                //dup = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth, generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
            }

            // IIBeamAsymmetricalProfile
            else if (typ.ToString().Equals(typeof(IIBeamAsymmetricalProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(4);
                //dup = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange, iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
            }

            // IIBeamCellularProfile
            else if (typ.ToString().Equals(typeof(IIBeamCellularProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(4);
                //dup = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web, iBeamCellular.WebOpening);
            }

            // IIBeamSymmetricalProfile
            else if (typ.ToString().Equals(typeof(IIBeamSymmetricalProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(3);
                //dup = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges, iBeamSymmetrical.Web);
            }

            // IRectangleHollowProfile
            else if (typ.ToString().Equals(typeof(IRectangleHollowProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(3);
                //dup = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges, rectangleHollow.Webs);
            }

            // IRectangleProfile
            else if (typ.ToString().Equals(typeof(IRectangleProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(2);
                //dup = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
            }

            // IRectoEllipseProfile
            else if (typ.ToString().Equals(typeof(IRectoEllipseProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(4);
                //dup = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width, rectoEllipse.WidthFlat);
            }

            // ISecantPileProfile
            else if (typ.ToString().Equals(typeof(ISecantPileProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(4, true);
                //dup = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount, secantPile.IsWallNotSection);
            }

            // ISheetPileProfile
            else if (typ.ToString().Equals(typeof(ISheetPileProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(6);
                //dup = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth, sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
            }

            // IStadiumProfile
            else if (typ.ToString().Equals(typeof(IStadiumProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(2);
                //dup = IStadiumProfile.Create(stadium.Depth, stadium.Width);
            }

            // ITrapezoidProfile
            else if (typ.ToString().Equals(typeof(ITrapezoidProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(3);
                //dup = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
            }

            // ITSectionProfile
            else if (typ.ToString().Equals(typeof(ITSectionProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(3);
                //dup = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
            }
            // IPerimeterProfile
            else if (typ.ToString().Equals(typeof(IPerimeterProfile).ToString() + "_Implementation"))
            {
                SetNumberOfGenericInputs(2);
                //dup = IPerimeterProfile.Create();
                //solidPolygon;
                //voidPolygons;
            }

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        #endregion
        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            GhAdSec.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
            writer.SetString("enum", _mode.ToString());
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            GhAdSec.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
            _mode = (FoldMode)Enum.Parse(typeof(FoldMode), reader.GetString("mode"));
            first = false;
            return base.Read(reader);
        }

        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            return null;
        }
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        #endregion
        #region IGH_VariableParameterComponent null implementation
        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            if (_mode == FoldMode.Catalogue)
            {
                int i = 0;
                Params.Input[i].NickName = "S";
                Params.Input[i].Name = "Search";
                Params.Input[i].Description = "Text to search from";
                Params.Input[i].Access = GH_ParamAccess.item;
                Params.Input[i].Optional = true;

                i++;
                Params.Input[i].NickName = "iSS";
                Params.Input[i].Name = "Include Superseeded";
                Params.Input[i].Description = "Input true to include superseeded catalogue sections";
                Params.Input[i].Access = GH_ParamAccess.item;
                Params.Input[i].Optional = true;
            }
            else
            {
                int i = 0;
                // angle
                if (typ.ToString().Equals(typeof(IAngleProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the angle profile (leg in the local z axis).";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Fla";
                    Params.Input[i].Name = "Flange";
                    Params.Input[i].Description = "The flange of the angle profile (leg in the local y axis).";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Web";
                    Params.Input[i].Name = "Web";
                    Params.Input[i].Description = "The web of the angle profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    //dup = IAngleProfile.Create(angle.Depth, angle.Flange, angle.Web);
                }

                // channel
                else if (typ.ToString().Equals(typeof(IChannelProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the channel profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Fla";
                    Params.Input[i].Name = "Flanges";
                    Params.Input[i].Description = "The flanges of the channel profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Web";
                    Params.Input[i].Name = "Web";
                    Params.Input[i].Description = "The web of the channel profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IChannelProfile.Create(channel.Depth, channel.Flanges, channel.Web);
                }

                // circle hollow
                else if (typ.ToString().Equals(typeof(ICircleHollowProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "Ø";
                    Params.Input[i].Name = "Diameter [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The diameter of the hollow circle.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "t";
                    Params.Input[i].Name = "Thickness [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The wall thickness of the hollow circle.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
                }

                // circle
                else if (typ.ToString().Equals(typeof(ICircleProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "Ø";
                    Params.Input[i].Name = "Diameter [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The diameter of the circle.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    //dup = ICircleProfile.Create(circle.Diameter);
                }

                // ICruciformSymmetricalProfile
                else if (typ.ToString().Equals(typeof(ICruciformSymmetricalProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Fla";
                    Params.Input[i].Name = "Flange";
                    Params.Input[i].Description = "The flange (local y axis leg) of the cruciform.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Web";
                    Params.Input[i].Name = "Web";
                    Params.Input[i].Description = "The web (local z axis leg) thickness of the cruciform.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange, cruciformSymmetrical.Web);
                }

                // IEllipseHollowProfile
                else if (typ.ToString().Equals(typeof(IEllipseHollowProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the hollow ellipse.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "B";
                    Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The width of the hollow ellipse.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "t";
                    Params.Input[i].Name = "Thickness [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The wall thickness of the hollow ellipse.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width, ellipseHollow.WallThickness);
                }

                // IEllipseProfile
                else if (typ.ToString().Equals(typeof(IEllipseProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the ellipse.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "B";
                    Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The width of the ellipse.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
                }

                // IGeneralCProfile
                else if (typ.ToString().Equals(typeof(IGeneralCProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the generic c section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "B";
                    Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The flange width of the generic c section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "L";
                    Params.Input[i].Name = "Lip [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The lip of the generic c section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "t";
                    Params.Input[i].Name = "Thickness [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The thickness of the generic c section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
                }

                // IGeneralZProfile
                else if (typ.ToString().Equals(typeof(IGeneralZProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the generic z section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Bt";
                    Params.Input[i].Name = "Top Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The top flange width of the generic z section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Bb";
                    Params.Input[i].Name = "Bottom Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The bottom flange width of the generic z section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Lt";
                    Params.Input[i].Name = "Top Lip [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The top lip of the generic z section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Lb";
                    Params.Input[i].Name = "Lip [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The top lip of the generic z section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "t";
                    Params.Input[i].Name = "Thickness [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The thickness of the generic z section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth, generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
                }

                // IIBeamAsymmetricalProfile
                else if (typ.ToString().Equals(typeof(IIBeamAsymmetricalProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Ft";
                    Params.Input[i].Name = "Top Flange";
                    Params.Input[i].Description = "The top flange of the beam. Top is relative to the beam local access.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Fb";
                    Params.Input[i].Name = "Bottom Flange";
                    Params.Input[i].Description = "The bottom flange of the beam. Bottom is relative to the beam local access.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Web";
                    Params.Input[i].Name = "Web";
                    Params.Input[i].Description = "The web of the beam.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange, iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
                }

                // IIBeamCellularProfile
                else if (typ.ToString().Equals(typeof(IIBeamCellularProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Fla";
                    Params.Input[i].Name = "Flanges";
                    Params.Input[i].Description = "The flange of the cellular I-beam section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Web";
                    Params.Input[i].Name = "Web";
                    Params.Input[i].Description = "The web of the beam.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Wop";
                    Params.Input[i].Name = "Web Opening [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The size of the web opening.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web, iBeamCellular.WebOpening);
                }

                // IIBeamSymmetricalProfile
                else if (typ.ToString().Equals(typeof(IIBeamSymmetricalProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Fla";
                    Params.Input[i].Name = "Flanges";
                    Params.Input[i].Description = "Both flanges of the beam.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Web";
                    Params.Input[i].Name = "Web";
                    Params.Input[i].Description = "The web of the beam.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges, iBeamSymmetrical.Web);
                }

                // IRectangleHollowProfile
                else if (typ.ToString().Equals(typeof(IRectangleHollowProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the hollow rectangle.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Fla";
                    Params.Input[i].Name = "Flanges";
                    Params.Input[i].Description = "The flanges (top and bottom) of the hollow rectangle. The flange width defines the profile's total width.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Web";
                    Params.Input[i].Name = "Webs";
                    Params.Input[i].Description = "The webs (side walls) of the hollow rectangle.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges, rectangleHollow.Webs);
                }

                // IRectangleProfile
                else if (typ.ToString().Equals(typeof(IRectangleProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "Depth of the rectangle, in local z-axis direction.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "B";
                    Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "Width of the rectangle, in loca y-axis direction.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
                }

                // IRectoEllipseProfile
                else if (typ.ToString().Equals(typeof(IRectoEllipseProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The overall depth of the recto-ellipse profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Df";
                    Params.Input[i].Name = "Depth Flat [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The flat length of the profile's overall depth.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "B";
                    Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The overall width of the recto-ellipse profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Bf";
                    Params.Input[i].Name = "Width Flat [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The flat length of the profile's overall width.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width, rectoEllipse.WidthFlat);
                }

                // ISecantPileProfile
                else if (typ.ToString().Equals(typeof(ISecantPileProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "Ø";
                    Params.Input[i].Name = "Diameter [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The diameter of the piles.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "c/c";
                    Params.Input[i].Name = "Pile Centres [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The centre to centre distance between adjacent piles.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "No";
                    Params.Input[i].Name = "Pile Count";
                    Params.Input[i].Description = "The number of piles in the profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "W/S";
                    Params.Input[i].Name = "isWall";
                    Params.Input[i].Description = "Converts the profile into a wall secant pile profile if true -- Converts the profile into a section secant pile profile if false.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount, secantPile.IsWallNotSection);
                }

                // ISheetPileProfile
                else if (typ.ToString().Equals(typeof(ISheetPileProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the sheet pile section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "B";
                    Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The overall width of the sheet pile section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Bt";
                    Params.Input[i].Name = "Top Flange Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The top flange width of the sheet pile section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Bb";
                    Params.Input[i].Name = "Bottom Flange Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The bottom flange width of the sheet pile section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Ft";
                    Params.Input[i].Name = "Flange Thickness [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The flange thickness of the sheet pile section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Wt";
                    Params.Input[i].Name = "Web Thickness [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The web thickness of the sheet pile section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth, sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
                }

                // IStadiumProfile
                else if (typ.ToString().Equals(typeof(IStadiumProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The profile's overall depth considering the side length of the rectangle and the radii of the semicircles on the two ends.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "B";
                    Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The profile's width (diameter of the semicircles).";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = IStadiumProfile.Create(stadium.Depth, stadium.Width);
                }

                // ITrapezoidProfile
                else if (typ.ToString().Equals(typeof(ITrapezoidProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth in z-axis direction of trapezoidal profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Bt";
                    Params.Input[i].Name = "Top Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The top width of trapezoidal profile. Top is relative to the local z-axis.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Bb";
                    Params.Input[i].Name = "Bottom Width [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The bottom width of trapezoidal profile. Bottom is relative to the local z-axis.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
                }

                // ITSectionProfile
                else if (typ.ToString().Equals(typeof(ITSectionProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "D";
                    Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
                    Params.Input[i].Description = "The depth of the T section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Fla";
                    Params.Input[i].Name = "Flange";
                    Params.Input[i].Description = "The flange of the T section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "Web";
                    Params.Input[i].Name = "Web";
                    Params.Input[i].Description = "The web of the T section profile.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;
                    //dup = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
                }
                // IPerimeterProfile
                else if (typ.ToString().Equals(typeof(IPerimeterProfile).ToString() + "_Implementation"))
                {
                    Params.Input[i].NickName = "B";
                    Params.Input[i].Name = "Boundary";
                    Params.Input[i].Description = "The outer edge polyline or BRep. If BRep contains openings these will be added as voids";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    i++;
                    Params.Input[i].NickName = "V";
                    Params.Input[i].Name = "[Optional] Void polylines";
                    Params.Input[i].Description = "The void polygons within the solid polygon of the perimeter profile. If first input is a BRep this input will be ignored.";
                    Params.Input[i].Access = GH_ParamAccess.item;
                    Params.Input[i].Optional = false;

                    //dup = IPerimeterProfile.Create();
                    //solidPolygon;
                    //voidPolygons;
                }
            }
        }
        #endregion  
    }
}