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

namespace GhAdSec.Components
{
    /// <summary>
    /// Component to create a profile text-string
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
        { this.Hidden = true; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        //protected override System.Drawing.Bitmap Icon => GhSA.Properties.Resources.CreateProfile;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                Dictionary<string, Type> profileTypesInitial = GhAdSec.Helpers.ReflectAdSecAPI.ReflectNamespace("Oasys.Profiles");
                profileTypes = new Dictionary<string, Type>();
                foreach (KeyValuePair<string, Type> kvp in profileTypesInitial)
                {
                    if (kvp.Key != "IProfile")
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
                _mode = FoldMode.Other;
                Type typ = profileTypes[selecteditems[0]];
                Mode2Clicked(typ);
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
            "Profile type", "Catalogue", "Type", "Profile"
        });
        Dictionary<string, Type> profileTypes;
        Dictionary<string, FieldInfo> profileFields;

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
            pManager.AddTextParameter("Search", "S", "Text to search from", GH_ParamAccess.item);
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
                Oasys.Collections.IList<Oasys.AdSec.IWarning> warn = catalogueProfile.Value.Validate();
                foreach(Oasys.AdSec.IWarning warning in warn)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning.Description);
                DA.SetData(0, catalogueProfile);
                return;
            }
            #endregion

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
        private void Mode2Clicked(Type typ)
        {
            RecordUndoEvent("Dropdown changed");

            Dictionary<List<string>, Type> profileMethods = GhAdSec.Helpers.ReflectAdSecAPI.ReflectMethods(typ);
            //    // set number of input parameters
            //    int param;
            //    if (isTapered)
            //        param = 3;
            //    else
            //    {
            //        if (isHollow)
            //            param = 4;
            //        else
            //            param = 2;
            //    }
            //    //handle exception when we come from Geometric or Catalogue mode where 
            //    //first input paraemter is of curve type and must be deleted
            //    int par2;
            //    if (_mode == FoldMode.Geometric || _mode == FoldMode.Catalogue)
            //        par2 = 0;
            //    else
            //        par2 = param;
            //    //remove input parameters
            //    while (Params.Input.Count > par2)
            //        Params.UnregisterInputParameter(Params.Input[par2], true);

            //    //register input parameter
            //    while (Params.Input.Count < param)
            //        Params.RegisterInputParam(new Param_Number());

            //    _mode = myMode;

            //    (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            //    Params.OnParametersChanged();
            //    ExpireSolution(true);
        }

        #endregion
        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            GhAdSec.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);

            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            GhAdSec.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

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
                

                i = 1;
                Params.Input[i].NickName = "iSS";
                Params.Input[i].Name = "Include Superseeded";
                Params.Input[i].Description = "Input true to include superseeded catalogue sections";
                Params.Input[i].Access = GH_ParamAccess.item;
                Params.Input[i].Optional = true;
            }
            
        }
        private void ReDrawComponent()
        {
            System.Drawing.PointF pivot = new System.Drawing.PointF(this.Attributes.Pivot.X, this.Attributes.Pivot.Y);
            this.CreateAttributes();
            this.Attributes.Pivot = pivot;
            this.Attributes.ExpireLayout();
            this.Attributes.PerformLayout();
        }
        #endregion  
    }
}