using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using AdSecGH.Helpers;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Oasys.Profiles;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.UI;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;
using Rhino.Geometry;

namespace AdSecGH.Components
{
  public class CreateProfile : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("ea0741e5-905e-4ecb-8270-a584e3f99aa3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateProfile;

    public CreateProfile() : base("Create Profile", "Profile", "Create Profile for AdSec Section", Ribbon.CategoryName.Name(), Ribbon.SubCategoryName.Cat2())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }

    protected override string HtmlHelp_Source()
    {
      string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.Profiles.html";
      return help;
    }
    #endregion

    #region Custom UI
    public override void CreateAttributes()
    {
        Dictionary<string, Type> profileTypesInitial = ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.Profiles");
        this.ProfileTypes = new Dictionary<string, Type>();
        foreach (KeyValuePair<string, Type> kvp in profileTypesInitial)
        {
          // filter out IProfile, IPoint, IFlange, IWeb and ITrapezoidProfileAbstractInterface
          if (!ExcludedInterfaces.Contains(kvp.Key))
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
            this.ProfileTypes.Add(name.ToString(), kvp.Value);
          }
        }
        if (this.SelectedItems == null)
        {
          // create a new list of selected items and add the first material type
          this.SelectedItems = new List<string>();
          this.SelectedItems.Add("Rectangle");
        }
        if (this.DropDownItems == null)
        {
          // create a new list of selected items and add the first material type
          this.DropDownItems = new List<List<string>>();
          this.DropDownItems.Add(this.ProfileTypes.Keys.ToList());
        }

        // length
        this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
        this.SelectedItems.Add(LengthUnit.ToString());

      m_attributes = new DropDownComponentAttributes(this, this.SetSelected, this.DropDownItems, this.SelectedItems, this.SpacerDescriptions);
    }

    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[]
      {
        "Profile type", "Measure", "Type", "Profile"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // Profile type
      this.DropDownItems.Add(ProfileTypes.Keys.ToList());
      this.SelectedItems.Add("Rectangle");

      // Length
      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      this.SelectedItems.Add(Length.GetAbbreviation(this.LengthUnit));

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // input -1 to force update of catalogue sections to include/exclude superseeded
      bool updateCat = false;
      if (i == -1)
      {
        this.SelectedItems[0] = "Catalogue";
        updateCat = true;
        i = 0;
      }
      else
      {
        // change selected item
        this.SelectedItems[i] = this.DropDownItems[i][j];
      }

      if (this.SelectedItems[0] == "Catalogue")
      {
        // update spacer description to match catalogue dropdowns
        this.SpacerDescriptions[1] = "Catalogue";

        // if FoldMode is not currently catalogue state, then we update all lists
        if (_mode != FoldMode.Catalogue | updateCat)
        {
          // remove any existing selections
          while (this.SelectedItems.Count > 1)
            this.SelectedItems.RemoveAt(1);

          // set catalogue selection to all
          _catalogueIndex = -1;

          if (cataloguedata == null)
            cataloguedata = SqlReader.GetCataloguesDataFromSQLite(Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"));
          catalogueNames = cataloguedata.Item1;
          catalogueNumbers = cataloguedata.Item2;

          // set types to all
          _typeIndex = -1;
          // update typelist with all catalogues
          typedata = SqlReader.GetTypesDataFromSQLite(_catalogueIndex, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), _inclSS);
          typeNames = typedata.Item1;
          typeNumbers = typedata.Item2;

          // update section list to all types
          sectionList = SqlReader.GetSectionsDataFromSQLite(typeNumbers, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), _inclSS);

          // filter by search pattern
          filteredlist = new List<string>();
          if (_search == "")
          {
            filteredlist = sectionList;
          }
          else
          {
            for (int k = 0; k < sectionList.Count; k++)
            {
              if (sectionList[k].ToLower().Contains(_search))
              {
                filteredlist.Add(sectionList[k]);
              }
              if (!_search.Any(char.IsDigit))
              {
                string test = sectionList[k].ToString();
                test = Regex.Replace(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(_search))
                {
                  filteredlist.Add(sectionList[k]);
                }
              }
            }
          }

          // update displayed selections to all
          this.SelectedItems.Add(catalogueNames[0]);
          this.SelectedItems.Add(typeNames[0]);
          this.SelectedItems.Add(filteredlist[0]);

          // call graphics update
          Mode1Clicked();
        }

        // update dropdown lists
        while (this.DropDownItems.Count > 1)
          this.DropDownItems.RemoveAt(1);

        // add catalogues (they will always be the same so no need to rerun sql call)
        this.DropDownItems.Add(catalogueNames);

        // type list
        // if second list (i.e. catalogue list) is changed, update types list to account for that catalogue
        if (i == 1)
        {
          // update catalogue index with the selected catalogue
          _catalogueIndex = catalogueNumbers[j];
          this.SelectedItems[1] = catalogueNames[j];

          // update typelist with selected input catalogue
          typedata = SqlReader.GetTypesDataFromSQLite(_catalogueIndex, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), _inclSS);
          typeNames = typedata.Item1;
          typeNumbers = typedata.Item2;

          // update section list from new types (all new types in catalogue)
          List<int> types = typeNumbers.ToList();
          types.RemoveAt(0); // remove -1 from beginning of list
          sectionList = SqlReader.GetSectionsDataFromSQLite(types, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), _inclSS);

          // filter by search pattern
          filteredlist = new List<string>();
          if (_search == "")
          {
            filteredlist = sectionList;
          }
          else
          {
            for (int k = 0; k < sectionList.Count; k++)
            {
              if (sectionList[k].ToLower().Contains(_search))
              {
                filteredlist.Add(sectionList[k]);
              }
              if (!_search.Any(char.IsDigit))
              {
                string test = sectionList[k].ToString();
                test = Regex.Replace(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(_search))
                {
                  filteredlist.Add(sectionList[k]);
                }
              }
            }
          }

          // update selections to display first item in new list
          this.SelectedItems[2] = typeNames[0];
          this.SelectedItems[3] = filteredlist[0];
        }
        this.DropDownItems.Add(typeNames);

        // section list
        // if third list (i.e. types list) is changed, update sections list to account for these section types

        if (i == 2)
        {
          // update catalogue index with the selected catalogue
          _typeIndex = typeNumbers[j];
          this.SelectedItems[2] = typeNames[j];

          // create type list
          List<int> types = new List<int>();
          if (_typeIndex == -1) // if all
          {
            types = typeNumbers.ToList(); // use current selected list of type numbers
            types.RemoveAt(0); // remove -1 from beginning of list
          }
          else
            types = new List<int> { _typeIndex }; // create empty list and add the single selected type 


          // section list with selected types (only types in selected type)
          sectionList = SqlReader.GetSectionsDataFromSQLite(types, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), _inclSS);

          // filter by search pattern
          filteredlist = new List<string>();
          if (_search == "")
          {
            filteredlist = sectionList;
          }
          else
          {
            for (int k = 0; k < sectionList.Count; k++)
            {
              if (sectionList[k].ToLower().Contains(_search))
              {
                filteredlist.Add(sectionList[k]);
              }
              if (!_search.Any(char.IsDigit))
              {
                string test = sectionList[k].ToString();
                test = Regex.Replace(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(_search))
                {
                  filteredlist.Add(sectionList[k]);
                }
              }
            }
          }

          // update selected section to be all
          this.SelectedItems[3] = filteredlist[0];
        }
        this.DropDownItems.Add(filteredlist);

        // selected profile
        // if fourth list (i.e. section list) is changed, updated the sections list to only be that single profile
        if (i == 3)
        {
          // update displayed selected
          this.SelectedItems[3] = filteredlist[j];
        }
        profileString = this.SelectedItems[3];

        base.UpdateUI();
      }
      else
      {
        // update spacer description to match none-catalogue dropdowns
        this.SpacerDescriptions[1] = "Measure";// = new List<string>(new string[]
                                               //{
                                               //    "Profile type", "Measure", "Type", "Profile"
                                               //});

        if (_mode != FoldMode.Other)
        {
          // remove all catalogue dropdowns
          while (this.DropDownItems.Count > 1)
            this.DropDownItems.RemoveAt(1);

          // add length measure dropdown list
          this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));

          // set selected length
          this.SelectedItems[1] = LengthUnit.ToString();
        }

        if (i == 0)
        {
          // update profile type if change is made to first dropdown menu
          typ = this.ProfileTypes[this.SelectedItems[0]];
          Mode2Clicked();
        }
        else
        {
          // change unit
          LengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), this.SelectedItems[i]);
        }
      }
      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      if (this.SelectedItems[0] == "Catalogue")
      {
        // update spacer description to match catalogue dropdowns
        this.SpacerDescriptions = new List<string>(new string[]
        {
          "Profile type", "Catalogue", "Type", "Profile"
        });
        if (cataloguedata == null)
          cataloguedata = SqlReader.GetCataloguesDataFromSQLite(Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"));
        catalogueNames = cataloguedata.Item1;
        catalogueNumbers = cataloguedata.Item2;

        if (typedata == null)
          typedata = SqlReader.GetTypesDataFromSQLite(-1, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), false);
        typedata = SqlReader.GetTypesDataFromSQLite(_catalogueIndex, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), _inclSS);
        typeNames = typedata.Item1;
        typeNumbers = typedata.Item2;

        // call graphics update
        Mode1Clicked();

        profileString = this.SpacerDescriptions[3];
      }
      else
      {
        // update spacer description to match none-catalogue dropdowns
        this.SpacerDescriptions = new List<string>(new string[]
        {
          "Profile type", "Measure", "Type", "Profile"
        });

        typ = this.ProfileTypes[this.SelectedItems[0]];
        Mode2Clicked();
      }
    }
    #endregion


    #region Input and output
    List<string> ExcludedInterfaces = new List<string>(new string[]
    {
      "IProfile", "IPoint", "IPolygon", "IFlange", "IWeb", "IWebConstant", "IWebTapered", "ITrapezoidProfileAbstractInterface", "IIBeamProfile"
    });
    Dictionary<string, Type> ProfileTypes;
    Dictionary<string, FieldInfo> ProfileFields;

    private LengthUnit LengthUnit = DefaultUnits.LengthUnitGeometry;

    #region catalogue sections
    // for catalogue selection
    // Catalogues
    Tuple<List<string>, List<int>> cataloguedata;
    List<int> catalogueNumbers = new List<int>(); // internal db catalogue numbers
    List<string> catalogueNames = new List<string>(); // list of displayed catalogues
    bool _inclSS;

    // Types
    Tuple<List<string>, List<int>> typedata;
    List<int> typeNumbers = new List<int>(); //  internal db type numbers
    List<string> typeNames = new List<string>(); // list of displayed types

    // Sections
    // list of displayed sections
    List<string> sectionList;
    List<string> filteredlist = new List<string>();
    int _catalogueIndex = -1; //-1 is all
    int _typeIndex = -1;
    // displayed selections
    string typeName = "All";
    string sectionName = "All";
    // list of sections as outcome from selections
    string profileString = "HE HE200.B";
    string _search = "";
    #endregion

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      IQuantity quantity = new Length(0, LengthUnit);
      string unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
      pManager.AddGenericParameter("Width [" + unitAbbreviation + "]", "B", "Profile width", GH_ParamAccess.item);
      pManager.AddGenericParameter("Depth [" + unitAbbreviation + "]", "H", "Profile depth", GH_ParamAccess.item);
      pManager.AddPlaneParameter("LocalPlane", "P", "[Optional] Plane representing local coordinate system, by default a YZ-plane is used", GH_ParamAccess.item, Plane.WorldYZ);
      pManager.HideParameter(2);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Profile", "Pf", "Profile for AdSec Section", GH_ParamAccess.item);
    }

    #endregion
    protected override void SolveInstance(IGH_DataAccess DA)
    {
      this.ClearRuntimeMessages();
      for (int i = 0; i < this.Params.Input.Count; i++)
        this.Params.Input[i].ClearRuntimeMessages();

      #region catalogue
      if (_mode == FoldMode.Catalogue)
      {
        // get user input filter search string
        bool incl = false;
        if (DA.GetData(1, ref incl))
        {
          if (_inclSS != incl)
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
        if (!inSearch.Equals(_search))
        {
          _search = inSearch.ToString();
          SetSelected(-1, 0);
          this.ExpireSolution(true);
        }

        Plane local = Plane.WorldYZ;
        Plane temp = Plane.Unset;
        if (DA.GetData(2, ref temp))
          local = temp;

        AdSecProfileGoo catalogueProfile = new AdSecProfileGoo(ICatalogueProfile.Create("CAT " + profileString), local);
        Oasys.Collections.IList<Oasys.AdSec.IWarning> warn = catalogueProfile.Profile.Validate();
        foreach (Oasys.AdSec.IWarning warning in warn)
          AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning.Description);
        DA.SetData(0, catalogueProfile);

        return;
      }

      if (_mode == FoldMode.Other)
      {
        this.ClearRuntimeMessages();
        IProfile profile = null;
        // angle
        if (typ.Name.Equals(typeof(IAngleProfile).Name))
        {
          profile = IAngleProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              AdSecInput.Flange(this, DA, 1),
              AdSecInput.Web(this, DA, 2));
        }

        // channel
        else if (typ.Name.Equals(typeof(IChannelProfile).Name))
        {
          profile = IChannelProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              AdSecInput.Flange(this, DA, 1),
              (IWebConstant)AdSecInput.Web(this, DA, 2));
        }

        // circle hollow
        else if (typ.Name.Equals(typeof(ICircleHollowProfile).Name))
        {
          profile = ICircleHollowProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit));
        }

        // circle
        else if (typ.Name.Equals(typeof(ICircleProfile).Name))
        {
          profile = ICircleProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit));
        }

        // ICruciformSymmetricalProfile
        else if (typ.Name.Equals(typeof(ICruciformSymmetricalProfile).Name))
        {
          profile = ICruciformSymmetricalProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              AdSecInput.Flange(this, DA, 1),
              (IWebConstant)AdSecInput.Web(this, DA, 2));
        }

        // IEllipseHollowProfile
        else if (typ.Name.Equals(typeof(IEllipseHollowProfile).Name))
        {
          profile = IEllipseHollowProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 2, LengthUnit));
        }

        // IEllipseProfile
        else if (typ.Name.Equals(typeof(IEllipseProfile).Name))
        {
          profile = IEllipseProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit));
        }

        // IGeneralCProfile
        else if (typ.Name.Equals(typeof(IGeneralCProfile).Name))
        {
          profile = IGeneralCProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 2, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 3, LengthUnit));
        }

        // IGeneralZProfile
        else if (typ.Name.Equals(typeof(IGeneralZProfile).Name))
        {
          profile = IGeneralZProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 2, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 3, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 4, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 5, LengthUnit));
        }

        // IIBeamAsymmetricalProfile
        else if (typ.Name.Equals(typeof(IIBeamAsymmetricalProfile).Name))
        {
          profile = IIBeamAsymmetricalProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              AdSecInput.Flange(this, DA, 1),
              AdSecInput.Flange(this, DA, 2),
              AdSecInput.Web(this, DA, 3));
        }

        // IIBeamCellularProfile
        else if (typ.Name.Equals(typeof(IIBeamCellularProfile).Name))
        {
          profile = IIBeamCellularProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              AdSecInput.Flange(this, DA, 1),
              (IWebConstant)AdSecInput.Web(this, DA, 2),
              (Length)Input.UnitNumber(this, DA, 3, LengthUnit));
        }

        // IIBeamSymmetricalProfile
        else if (typ.Name.Equals(typeof(IIBeamSymmetricalProfile).Name))
        {
          profile = IIBeamSymmetricalProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              AdSecInput.Flange(this, DA, 1),
              (IWebConstant)AdSecInput.Web(this, DA, 2));
        }

        // IRectangleHollowProfile
        else if (typ.Name.Equals(typeof(IRectangleHollowProfile).Name))
        {
          profile = IRectangleHollowProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              AdSecInput.Flange(this, DA, 1),
              (IWebConstant)AdSecInput.Web(this, DA, 2));
        }

        // IRectangleProfile
        else if (typ.Name.Equals(typeof(IRectangleProfile).Name))
        {
          profile = IRectangleProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit));
        }

        // IRectoEllipseProfile
        else if (typ.Name.Equals(typeof(IRectoEllipseProfile).Name))
        {
          profile = IRectoEllipseProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 2, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 3, LengthUnit));
        }

        // ISecantPileProfile
        else if (typ.Name.Equals(typeof(ISecantPileProfile).Name))
        {
          int pileCount = 0;
          if (!DA.GetData(2, ref pileCount))
          {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input PileCount to integer.");
            return;
          }

          bool isWallNotSection = false;
          if (!DA.GetData(3, ref isWallNotSection))
          {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input isWall to boolean.");
            return;
          }

          profile = ISecantPileProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit),
              pileCount, isWallNotSection);
        }

        // ISheetPileProfile
        else if (typ.Name.Equals(typeof(ISheetPileProfile).Name))
        {
          profile = ISheetPileProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 2, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 3, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 4, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 5, LengthUnit));
        }

        // IStadiumProfile
        else if (typ.Name.Equals(typeof(IStadiumProfile).Name))
        {
          profile = IStadiumProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit));
        }

        // ITrapezoidProfile
        else if (typ.Name.Equals(typeof(ITrapezoidProfile).Name))
        {
          profile = ITrapezoidProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 1, LengthUnit),
              (Length)Input.UnitNumber(this, DA, 2, LengthUnit));
        }

        // ITSectionProfile
        else if (typ.Name.Equals(typeof(ITSectionProfile).Name))
        {
          profile = ITSectionProfile.Create(
              (Length)Input.UnitNumber(this, DA, 0, LengthUnit),
              AdSecInput.Flange(this, DA, 1),
              AdSecInput.Web(this, DA, 2));
        }

        // IPerimeterProfile (last chance...)
        else if (typ.Name.Equals(typeof(IPerimeterProfile).Name))
        {
          //profile = GetInput.Boundaries(this, DA, 0, 1, lengthUnit);
          DA.SetData(0, AdSecInput.Boundaries(this, DA, 0, 1, LengthUnit, true));
          return;
        }
        else
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to create profile");
          return;
        }

        try
        {
          Oasys.Collections.IList<Oasys.AdSec.IWarning> warn = profile.Validate();
          foreach (Oasys.AdSec.IWarning warning in warn)
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, warning.Description);
        }
        catch (Exception e)
        {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
          return;
        }

        Plane local = Plane.WorldYZ;
        Plane temp = Plane.Unset;
        if (DA.GetData(Params.Input.Count - 1, ref temp))
          local = temp;

        DA.SetData(0, new AdSecProfileGoo(profile, local));
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
    private FoldMode _mode = FoldMode.Other;

    private void Mode1Clicked()
    {
      // tempoarily removing plane
      IGH_Param param_Plane = this.Params.Input[Params.Input.Count - 1];
      this.Params.UnregisterInputParameter(this.Params.Input[this.Params.Input.Count - 1], false);

      // input parameters
      while (this.Params.Input.Count > 0)
        this.Params.UnregisterInputParameter(this.Params.Input[0], true);

      // register input parameter
      this.Params.RegisterInputParam(new Param_String());
      this.Params.RegisterInputParam(new Param_Boolean());

      // add plane
      this.Params.RegisterInputParam(param_Plane);

      this._mode = FoldMode.Catalogue;

      base.UpdateUI();
    }

    private void SetNumberOfGenericInputs(int inputs, bool isSecantPile = false)
    {
      this._numberOfInputs = inputs;

      // if last input previously was a bool and we no longer need that
      if (lastInputWasSecant || isSecantPile)
      {
        if (Params.Input.Count > 0)
        {
          // make sure to remove last param
          this.Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], true);
          this.Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], true);
        }
      }

      // remove any additional inputs
      while (Params.Input.Count > inputs)
        this.Params.UnregisterInputParameter(Params.Input[inputs], true);

      if (isSecantPile) // add two less generic than input says
      {
        while (Params.Input.Count > inputs + 2)
          this.Params.UnregisterInputParameter(Params.Input[inputs + 2], true);
        inputs -= 2;
      }

      // add inputs parameter
      while (Params.Input.Count < inputs)
        this.Params.RegisterInputParam(new Param_GenericObject());

      if (isSecantPile) // finally add int and bool param if secant
      {
        this.Params.RegisterInputParam(new Param_Integer());
        this.Params.RegisterInputParam(new Param_Boolean());
        lastInputWasSecant = true;
      }
    }

    private bool lastInputWasSecant;
    private int _numberOfInputs;
    private Type typ = typeof(IRectangleProfile);

    private void Mode2Clicked()
    {
      // remove plane
      IGH_Param param_Plane = this.Params.Input[Params.Input.Count - 1];
      this.Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

      // check if mode is correct
      if (_mode != FoldMode.Other)
      {
        // if we come from catalogue mode remove all input parameters
        while (Params.Input.Count > 0)
          this.Params.UnregisterInputParameter(Params.Input[0], true);

        // set mode to other
        _mode = FoldMode.Other;
      }

      // angle
      if (typ.Name.Equals(typeof(IAngleProfile).Name))
      {
        SetNumberOfGenericInputs(3);
        //dup = IAngleProfile.Create(angle.Depth, angle.Flange, angle.Web);
      }

      // channel
      else if (typ.Name.Equals(typeof(IChannelProfile).Name))
      {
        SetNumberOfGenericInputs(3);
        //dup = IChannelProfile.Create(channel.Depth, channel.Flanges, channel.Web);
      }

      // circle hollow
      else if (typ.Name.Equals(typeof(ICircleHollowProfile).Name))
      {
        SetNumberOfGenericInputs(2);
        //dup = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
      }

      // circle
      else if (typ.Name.Equals(typeof(ICircleProfile).Name))
      {
        SetNumberOfGenericInputs(1);
        //dup = ICircleProfile.Create(circle.Diameter);
      }

      // ICruciformSymmetricalProfile
      else if (typ.Name.Equals(typeof(ICruciformSymmetricalProfile).Name))
      {
        SetNumberOfGenericInputs(3);
        //dup = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange, cruciformSymmetrical.Web);
      }

      // IEllipseHollowProfile
      else if (typ.Name.Equals(typeof(IEllipseHollowProfile).Name))
      {
        SetNumberOfGenericInputs(3);
        //dup = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width, ellipseHollow.WallThickness);
      }

      // IEllipseProfile
      else if (typ.Name.Equals(typeof(IEllipseProfile).Name))
      {
        SetNumberOfGenericInputs(2);
        //dup = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
      }

      // IGeneralCProfile
      else if (typ.Name.Equals(typeof(IGeneralCProfile).Name))
      {
        SetNumberOfGenericInputs(4);
        //dup = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
      }

      // IGeneralZProfile
      else if (typ.Name.Equals(typeof(IGeneralZProfile).Name))
      {
        SetNumberOfGenericInputs(6);
        //dup = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth, generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
      }

      // IIBeamAsymmetricalProfile
      else if (typ.Name.Equals(typeof(IIBeamAsymmetricalProfile).Name))
      {
        SetNumberOfGenericInputs(4);
        //dup = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange, iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
      }

      // IIBeamCellularProfile
      else if (typ.Name.Equals(typeof(IIBeamCellularProfile).Name))
      {
        SetNumberOfGenericInputs(4);
        //dup = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web, iBeamCellular.WebOpening);
      }

      // IIBeamSymmetricalProfile
      else if (typ.Name.Equals(typeof(IIBeamSymmetricalProfile).Name))
      {
        SetNumberOfGenericInputs(3);
        //dup = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges, iBeamSymmetrical.Web);
      }

      // IRectangleHollowProfile
      else if (typ.Name.Equals(typeof(IRectangleHollowProfile).Name))
      {
        SetNumberOfGenericInputs(3);
        //dup = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges, rectangleHollow.Webs);
      }

      // IRectangleProfile
      else if (typ.Name.Equals(typeof(IRectangleProfile).Name))
      {
        SetNumberOfGenericInputs(2);
        //dup = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
      }

      // IRectoEllipseProfile
      else if (typ.Name.Equals(typeof(IRectoEllipseProfile).Name))
      {
        SetNumberOfGenericInputs(4);
        //dup = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width, rectoEllipse.WidthFlat);
      }

      // ISecantPileProfile
      else if (typ.Name.Equals(typeof(ISecantPileProfile).Name))
      {
        SetNumberOfGenericInputs(4, true);
        //dup = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount, secantPile.IsWallNotSection);
      }

      // ISheetPileProfile
      else if (typ.Name.Equals(typeof(ISheetPileProfile).Name))
      {
        SetNumberOfGenericInputs(6);
        //dup = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth, sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
      }

      // IStadiumProfile
      else if (typ.Name.Equals(typeof(IStadiumProfile).Name))
      {
        SetNumberOfGenericInputs(2);
        //dup = IStadiumProfile.Create(stadium.Depth, stadium.Width);
      }

      // ITrapezoidProfile
      else if (typ.Name.Equals(typeof(ITrapezoidProfile).Name))
      {
        SetNumberOfGenericInputs(3);
        //dup = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
      }

      // ITSectionProfile
      else if (typ.Name.Equals(typeof(ITSectionProfile).Name))
      {
        SetNumberOfGenericInputs(3);
        //dup = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
      }
      // IPerimeterProfile
      else if (typ.Name.Equals(typeof(IPerimeterProfile).Name))
      {
        SetNumberOfGenericInputs(2);
        //dup = IPerimeterProfile.Create();
        //solidPolygon;
        //voidPolygons;
      }

      // add plane
      this.Params.RegisterInputParam(param_Plane);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      this.Params.OnParametersChanged();
      this.ExpireSolution(true);
    }

    #endregion
    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      writer.SetString("mode", _mode.ToString());
      writer.SetString("lengthUnit", LengthUnit.ToString());
      writer.SetBoolean("inclSS", _inclSS);
      writer.SetInt32("NumberOfInputs", _numberOfInputs);
      writer.SetInt32("catalogueIndex", _catalogueIndex);
      writer.SetInt32("typeIndex", _typeIndex);
      writer.SetString("search", _search);
      return base.Write(writer);
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      this._mode = (FoldMode)Enum.Parse(typeof(FoldMode), reader.GetString("mode"));
      this.LengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), reader.GetString("lengthUnit"));

      this._inclSS = reader.GetBoolean("inclSS");
      this._numberOfInputs = reader.GetInt32("NumberOfInputs");
      this._catalogueIndex = reader.GetInt32("catalogueIndex");
      this._typeIndex = reader.GetInt32("typeIndex");
      this._search = reader.GetString("search");

      bool flag = base.Read(reader);
      this.Params.Output[0].Access = GH_ParamAccess.tree;

      return flag;
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      IQuantity quantity = new Length(0, LengthUnit);
      string unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));

      if (_mode == FoldMode.Catalogue)
      {
        int i = 0;
        this.Params.Input[i].NickName = "S";
        this.Params.Input[i].Name = "Search";
        this.Params.Input[i].Description = "Text to search from";
        this.Params.Input[i].Access = GH_ParamAccess.item;
        this.Params.Input[i].Optional = true;

        i++;
        this.Params.Input[i].NickName = "iSS";
        this.Params.Input[i].Name = "InclSuperseeded";
        this.Params.Input[i].Description = "Input true to include superseeded catalogue sections";
        this.Params.Input[i].Access = GH_ParamAccess.item;
        this.Params.Input[i].Optional = true;
      }
      else
      {
        int i = 0;
        // angle
        if (typ.Name.Equals(typeof(IAngleProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the angle profile (leg in the local z axis).";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Fla";
          this.Params.Input[i].Name = "Flange";
          this.Params.Input[i].Description = "The flange of the angle profile (leg in the local y axis).";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Web";
          this.Params.Input[i].Name = "Web";
          this.Params.Input[i].Description = "The web of the angle profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          //dup = IAngleProfile.Create(angle.Depth, angle.Flange, angle.Web);
        }

        // channel
        else if (typ.Name.Equals(typeof(IChannelProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the channel profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Fla";
          this.Params.Input[i].Name = "Flanges";
          this.Params.Input[i].Description = "The flanges of the channel profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Web";
          this.Params.Input[i].Name = "Web";
          this.Params.Input[i].Description = "The web of the channel profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IChannelProfile.Create(channel.Depth, channel.Flanges, channel.Web);
        }

        // circle hollow
        else if (typ.Name.Equals(typeof(ICircleHollowProfile).Name))
        {
          this.Params.Input[i].NickName = "Ø";
          this.Params.Input[i].Name = "Diameter [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The diameter of the hollow circle.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "t";
          this.Params.Input[i].Name = "Thickness [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The wall thickness of the hollow circle.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = ICircleHollowProfile.Create(circleHollow.Diameter, circleHollow.WallThickness);
        }

        // circle
        else if (typ.Name.Equals(typeof(ICircleProfile).Name))
        {
          this.Params.Input[i].NickName = "Ø";
          this.Params.Input[i].Name = "Diameter [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The diameter of the circle.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          //dup = ICircleProfile.Create(circle.Diameter);
        }

        // ICruciformSymmetricalProfile
        else if (typ.Name.Equals(typeof(ICruciformSymmetricalProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Fla";
          this.Params.Input[i].Name = "Flange";
          this.Params.Input[i].Description = "The flange (local y axis leg) of the cruciform.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Web";
          this.Params.Input[i].Name = "Web";
          this.Params.Input[i].Description = "The web (local z axis leg) thickness of the cruciform.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = ICruciformSymmetricalProfile.Create(cruciformSymmetrical.Depth, cruciformSymmetrical.Flange, cruciformSymmetrical.Web);
        }

        // IEllipseHollowProfile
        else if (typ.Name.Equals(typeof(IEllipseHollowProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the hollow ellipse.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "B";
          this.Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The width of the hollow ellipse.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "t";
          this.Params.Input[i].Name = "Thickness [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The wall thickness of the hollow ellipse.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IEllipseHollowProfile.Create(ellipseHollow.Depth, ellipseHollow.Width, ellipseHollow.WallThickness);
        }

        // IEllipseProfile
        else if (typ.Name.Equals(typeof(IEllipseProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the ellipse.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "B";
          this.Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The width of the ellipse.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IEllipseProfile.Create(ellipse.Depth, ellipse.Width);
        }

        // IGeneralCProfile
        else if (typ.Name.Equals(typeof(IGeneralCProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the generic c section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "B";
          this.Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The flange width of the generic c section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "L";
          this.Params.Input[i].Name = "Lip [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The lip of the generic c section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "t";
          this.Params.Input[i].Name = "Thickness [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The thickness of the generic c section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IGeneralCProfile.Create(generalC.Depth, generalC.FlangeWidth, generalC.Lip, generalC.Thickness);
        }

        // IGeneralZProfile
        else if (typ.Name.Equals(typeof(IGeneralZProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the generic z section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Bt";
          this.Params.Input[i].Name = "TopWidth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The top flange width of the generic z section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Bb";
          this.Params.Input[i].Name = "BottomWidth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The bottom flange width of the generic z section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Lt";
          this.Params.Input[i].Name = "Top Lip [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The top lip of the generic z section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Lb";
          this.Params.Input[i].Name = "Lip [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The top lip of the generic z section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "t";
          this.Params.Input[i].Name = "Thickness [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The thickness of the generic z section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IGeneralZProfile.Create(generalZ.Depth, generalZ.TopFlangeWidth, generalZ.BottomFlangeWidth, generalZ.TopLip, generalZ.BottomLip, generalZ.Thickness);
        }

        // IIBeamAsymmetricalProfile
        else if (typ.Name.Equals(typeof(IIBeamAsymmetricalProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Ft";
          this.Params.Input[i].Name = "TopFlange";
          this.Params.Input[i].Description = "The top flange of the beam. Top is relative to the beam local access.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Fb";
          this.Params.Input[i].Name = "BottomFlange";
          this.Params.Input[i].Description = "The bottom flange of the beam. Bottom is relative to the beam local access.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Web";
          this.Params.Input[i].Name = "Web";
          this.Params.Input[i].Description = "The web of the beam.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IIBeamAsymmetricalProfile.Create(iBeamAsymmetrical.Depth, iBeamAsymmetrical.TopFlange, iBeamAsymmetrical.BottomFlange, iBeamAsymmetrical.Web);
        }

        // IIBeamCellularProfile
        else if (typ.Name.Equals(typeof(IIBeamCellularProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Fla";
          this.Params.Input[i].Name = "Flanges";
          this.Params.Input[i].Description = "The flange of the cellular I-beam section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Web";
          this.Params.Input[i].Name = "Web";
          this.Params.Input[i].Description = "The web of the beam.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Wop";
          this.Params.Input[i].Name = "WebOpening [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The size of the web opening.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IIBeamCellularProfile.Create(iBeamCellular.Depth, iBeamCellular.Flanges, iBeamCellular.Web, iBeamCellular.WebOpening);
        }

        // IIBeamSymmetricalProfile
        else if (typ.Name.Equals(typeof(IIBeamSymmetricalProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Fla";
          this.Params.Input[i].Name = "Flanges";
          this.Params.Input[i].Description = "Both flanges of the beam.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Web";
          this.Params.Input[i].Name = "Web";
          this.Params.Input[i].Description = "The web of the beam.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IIBeamSymmetricalProfile.Create(iBeamSymmetrical.Depth, iBeamSymmetrical.Flanges, iBeamSymmetrical.Web);
        }

        // IRectangleHollowProfile
        else if (typ.Name.Equals(typeof(IRectangleHollowProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the hollow rectangle.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Fla";
          this.Params.Input[i].Name = "Flanges";
          this.Params.Input[i].Description = "The flanges (top and bottom) of the hollow rectangle. The flange width defines the profile's total width.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Web";
          this.Params.Input[i].Name = "Webs";
          this.Params.Input[i].Description = "The webs (side walls) of the hollow rectangle.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IRectangleHollowProfile.Create(rectangleHollow.Depth, rectangleHollow.Flanges, rectangleHollow.Webs);
        }

        // IRectangleProfile
        else if (typ.Name.Equals(typeof(IRectangleProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "Depth of the rectangle, in local z-axis direction.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "B";
          this.Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "Width of the rectangle, in loca y-axis direction.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IRectangleProfile.Create(rectangle.Depth, rectangle.Width);
        }

        // IRectoEllipseProfile
        else if (typ.Name.Equals(typeof(IRectoEllipseProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The overall depth of the recto-ellipse profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Df";
          this.Params.Input[i].Name = "DepthFlat [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The flat length of the profile's overall depth.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "B";
          this.Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The overall width of the recto-ellipse profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Bf";
          this.Params.Input[i].Name = "WidthFlat [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The flat length of the profile's overall width.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IRectoEllipseProfile.Create(rectoEllipse.Depth, rectoEllipse.DepthFlat, rectoEllipse.Width, rectoEllipse.WidthFlat);
        }

        // ISecantPileProfile
        else if (typ.Name.Equals(typeof(ISecantPileProfile).Name))
        {
          this.Params.Input[i].NickName = "Ø";
          this.Params.Input[i].Name = "Diameter [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The diameter of the piles.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "c/c";
          this.Params.Input[i].Name = "PileCentres [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The centre to centre distance between adjacent piles.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "No";
          this.Params.Input[i].Name = "PileCount";
          this.Params.Input[i].Description = "The number of piles in the profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "W/S";
          this.Params.Input[i].Name = "isWall";
          this.Params.Input[i].Description = "Converts the profile into a wall secant pile profile if true -- Converts the profile into a section secant pile profile if false.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = ISecantPileProfile.Create(secantPile.Diameter, secantPile.PileCentres, secantPile.PileCount, secantPile.IsWallNotSection);
        }

        // ISheetPileProfile
        else if (typ.Name.Equals(typeof(ISheetPileProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the sheet pile section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "B";
          this.Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The overall width of the sheet pile section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Bt";
          this.Params.Input[i].Name = "TopFlangeWidth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The top flange width of the sheet pile section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Bb";
          this.Params.Input[i].Name = "BottomFlangeWidth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The bottom flange width of the sheet pile section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Ft";
          this.Params.Input[i].Name = "FlangeThickness [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The flange thickness of the sheet pile section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Wt";
          this.Params.Input[i].Name = "WebThickness [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The web thickness of the sheet pile section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = ISheetPileProfile.Create(sheetPile.Depth, sheetPile.Width, sheetPile.TopFlangeWidth, sheetPile.BottomFlangeWidth, sheetPile.FlangeThickness, sheetPile.WebThickness);
        }

        // IStadiumProfile
        else if (typ.Name.Equals(typeof(IStadiumProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The profile's overall depth considering the side length of the rectangle and the radii of the semicircles on the two ends.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "B";
          this.Params.Input[i].Name = "Width [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The profile's width (diameter of the semicircles).";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = IStadiumProfile.Create(stadium.Depth, stadium.Width);
        }

        // ITrapezoidProfile
        else if (typ.Name.Equals(typeof(ITrapezoidProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth in z-axis direction of trapezoidal profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Bt";
          this.Params.Input[i].Name = "TopWidth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The top width of trapezoidal profile. Top is relative to the local z-axis.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Bb";
          this.Params.Input[i].Name = "BottomWidth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The bottom width of trapezoidal profile. Bottom is relative to the local z-axis.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = ITrapezoidProfile.Create(trapezoid.Depth, trapezoid.TopWidth, trapezoid.BottomWidth);
        }

        // ITSectionProfile
        else if (typ.Name.Equals(typeof(ITSectionProfile).Name))
        {
          this.Params.Input[i].NickName = "D";
          this.Params.Input[i].Name = "Depth [" + unitAbbreviation + "]";
          this.Params.Input[i].Description = "The depth of the T section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Fla";
          this.Params.Input[i].Name = "Flange";
          this.Params.Input[i].Description = "The flange of the T section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "Web";
          this.Params.Input[i].Name = "Web";
          this.Params.Input[i].Description = "The web of the T section profile.";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;
          //dup = ITSectionProfile.Create(tSection.Depth, tSection.Flange, tSection.Web);
        }
        // IPerimeterProfile
        else if (typ.Name.Equals(typeof(IPerimeterProfile).Name))
        {
          this.Params.Input[i].NickName = "B";
          this.Params.Input[i].Name = "Boundary";
          this.Params.Input[i].Description = "The outer edge polyline or BRep. If BRep contains openings these will be added as voids";
          this.Params.Input[i].Access = GH_ParamAccess.item;
          this.Params.Input[i].Optional = false;

          i++;
          this.Params.Input[i].NickName = "V";
          this.Params.Input[i].Name = "VoidPolylines";
          this.Params.Input[i].Description = "[Optional] The void polygons within the solid polygon of the perimeter profile. If first input is a BRep this input will be ignored.";
          this.Params.Input[i].Access = GH_ParamAccess.list;
          this.Params.Input[i].Optional = true;
        }
      }
    }
  }
}
