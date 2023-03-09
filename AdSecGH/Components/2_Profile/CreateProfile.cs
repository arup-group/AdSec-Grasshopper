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
using OasysUnits;
using OasysUnits.Units;
using Rhino.Geometry;

namespace AdSecGH.Components
{
  /// <summary>
  /// Component to create AdSec profile
  /// </summary>
  public class CreateProfile : CreateOasysProfile, IGH_VariableParameterComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("ea0741e5-905e-4ecb-8270-a584e3f99aa3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    public override string DataSource => throw new NotImplementedException();
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateProfile;

    public CreateProfile() : base(
      "Create Profile",
      "Profile",
      "Create Profile for AdSec Section",
      Ribbon.CategoryName.Name(),
      Ribbon.SubCategoryName.Cat2())
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
    //This region overrides the typical component layout
    public override void CreateAttributes()
    {
      if (first)
      {
        Dictionary<string, Type> profileTypesInitial = ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.Profiles");
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
        dropdownitems.Add(Units.FilteredLengthUnits);
        selecteditems.Add(lengthUnit.ToString());


      }

      m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);

    }

    public void SetSelected(int i, int j)
    {
      // input -1 to force update of catalogue sections to include/exclude superseeded
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
        spacerDescriptions[1] = "Catalogue";

        // if FoldMode is not currently catalogue state, then we update all lists
        if (_mode != FoldMode.Catalogue | updateCat)
        {
          // remove any existing selections
          while (selecteditems.Count > 1)
            selecteditems.RemoveAt(1);

          typeNames = _typeData.Item1;
          typeNumbers = _typeData.Item2;

          // update section list to all types
          _sectionList = SqlReader.GetSectionsDataFromSQLite(typeNumbers, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);

          // filter by search pattern
          filteredlist = new List<string>();
          if (search == "")
          {
            filteredlist = _sectionList;
          }
          else
          {
            for (int k = 0; k < _sectionList.Count; k++)
            {
              if (_sectionList[k].ToLower().Contains(search))
              {
                filteredlist.Add(_sectionList[k]);
              }
              if (!search.Any(char.IsDigit))
              {
                string test = _sectionList[k].ToString();
                test = Regex.Replace(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(search))
                {
                  filteredlist.Add(_sectionList[k]);
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
          _typeData = SqlReader.GetTypesDataFromSQLite(catalogueIndex, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);
          typeNames = _typeData.Item1;
          typeNumbers = _typeData.Item2;

          // update section list from new types (all new types in catalogue)
          List<int> types = typeNumbers.ToList();
          types.RemoveAt(0); // remove -1 from beginning of list
          _sectionList = SqlReader.GetSectionsDataFromSQLite(types, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);

          // filter by search pattern
          filteredlist = new List<string>();
          if (search == "")
          {
            filteredlist = _sectionList;
          }
          else
          {
            for (int k = 0; k < _sectionList.Count; k++)
            {
              if (_sectionList[k].ToLower().Contains(search))
              {
                filteredlist.Add(_sectionList[k]);
              }
              if (!search.Any(char.IsDigit))
              {
                string test = _sectionList[k].ToString();
                test = Regex.Replace(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(search))
                {
                  filteredlist.Add(_sectionList[k]);
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
          _sectionList = SqlReader.GetSectionsDataFromSQLite(types, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);

          // filter by search pattern
          filteredlist = new List<string>();
          if (search == "")
          {
            filteredlist = _sectionList;
          }
          else
          {
            for (int k = 0; k < _sectionList.Count; k++)
            {
              if (_sectionList[k].ToLower().Contains(search))
              {
                filteredlist.Add(_sectionList[k]);
              }
              if (!search.Any(char.IsDigit))
              {
                string test = _sectionList[k].ToString();
                test = Regex.Replace(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(search))
                {
                  filteredlist.Add(_sectionList[k]);
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


        this.ExpirePreview(true);
        (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
        ExpireSolution(true);
        Params.OnParametersChanged();
        this.OnDisplayExpired(true);
      }
      else
      {
        // update spacer description to match none-catalogue dropdowns
        spacerDescriptions[1] = "Measure";// = new List<string>(new string[]
                                          //{
                                          //    "Profile type", "Measure", "Type", "Profile"
                                          //});

        if (_mode != FoldMode.Other)
        {
          // remove all catalogue dropdowns
          while (dropdownitems.Count > 1)
            dropdownitems.RemoveAt(1);

          // add length measure dropdown list
          dropdownitems.Add(Units.FilteredLengthUnits);

          // set selected length
          selecteditems[1] = lengthUnit.ToString();
        }

        if (i == 0)
        {
          // update profile type if change is made to first dropdown menu
          typ = profileTypes[selecteditems[0]];
          Mode2Clicked();
        }
        else
        {
          // change unit
          lengthUnit = (LengthUnit)Enum.Parse(typeof(LengthUnit), selecteditems[i]);
        }
      }
        (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      ExpireSolution(true);
      Params.OnParametersChanged();
      this.OnDisplayExpired(true);
    }

    private void UpdateUIFromSelectedItems()
    {
      if (selecteditems[0] == "Catalogue")
      {
        // update spacer description to match catalogue dropdowns
        spacerDescriptions = new List<string>(new string[]
        {
                    "Profile type", "Catalogue", "Type", "Profile"
        });
        if (_catalogueData == null)
          _catalogueData = SqlReader.GetCataloguesDataFromSQLite(Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"));
        catalogueNames = _catalogueData.Item1;
        catalogueNumbers = _catalogueData.Item2;

        if (_typeData == null)
          _typeData = SqlReader.GetTypesDataFromSQLite(-1, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), false);
        _typeData = SqlReader.GetTypesDataFromSQLite(catalogueIndex, Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3"), inclSS);
        typeNames = _typeData.Item1;
        typeNumbers = _typeData.Item2;

        // call graphics update
        comingFromSave = true;
        Mode1Clicked();
        comingFromSave = false;

        profileString = selecteditems[3];
      }
      else
      {
        // update spacer description to match none-catalogue dropdowns
        spacerDescriptions = new List<string>(new string[]
        {
                    "Profile type", "Measure", "Type", "Profile"
        });

        typ = profileTypes[selecteditems[0]];
        Mode2Clicked();
      }
    }
    #endregion


    #region Input and output
    List<string> spacerDescriptions = new List<string>(new string[]
    {
            "Profile type", "Measure", "Type", "Profile"
    });
    List<string> excludedInterfaces = new List<string>(new string[]
    {
        "IProfile", "IPoint", "IPolygon", "IFlange", "IWeb", "IWebConstant", "IWebTapered", "ITrapezoidProfileAbstractInterface", "IIBeamProfile"
    });
    Dictionary<string, Type> profileTypes;
    Dictionary<string, FieldInfo> profileFields;




    // Sections
    List<string> filteredlist = new List<string>();
    int catalogueIndex = -1; //-1 is all
    int typeIndex = -1;
    // displayed selections
    string typeName = "All";
    string sectionName = "All";
    // list of sections as outcome from selections
    string profileString = "HE HE200.B";
    string search = "";

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      IQuantity quantity = new Length(0, lengthUnit);
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
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.Flange(this, DA, 1),
              GetInput.Web(this, DA, 2));
        }

        // channel
        else if (typ.Name.Equals(typeof(IChannelProfile).Name))
        {
          profile = IChannelProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.Flange(this, DA, 1),
              (IWebConstant)GetInput.Web(this, DA, 2));
        }

        // circle hollow
        else if (typ.Name.Equals(typeof(ICircleHollowProfile).Name))
        {
          profile = ICircleHollowProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit));
        }

        // circle
        else if (typ.Name.Equals(typeof(ICircleProfile).Name))
        {
          profile = ICircleProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit));
        }

        // ICruciformSymmetricalProfile
        else if (typ.Name.Equals(typeof(ICruciformSymmetricalProfile).Name))
        {
          profile = ICruciformSymmetricalProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.Flange(this, DA, 1),
              (IWebConstant)GetInput.Web(this, DA, 2));
        }

        // IEllipseHollowProfile
        else if (typ.Name.Equals(typeof(IEllipseHollowProfile).Name))
        {
          profile = IEllipseHollowProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit),
              GetInput.GetLength(this, DA, 2, lengthUnit));
        }

        // IEllipseProfile
        else if (typ.Name.Equals(typeof(IEllipseProfile).Name))
        {
          profile = IEllipseProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit));
        }

        // IGeneralCProfile
        else if (typ.Name.Equals(typeof(IGeneralCProfile).Name))
        {
          profile = IGeneralCProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit),
              GetInput.GetLength(this, DA, 2, lengthUnit),
              GetInput.GetLength(this, DA, 3, lengthUnit));
        }

        // IGeneralZProfile
        else if (typ.Name.Equals(typeof(IGeneralZProfile).Name))
        {
          profile = IGeneralZProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit),
              GetInput.GetLength(this, DA, 2, lengthUnit),
              GetInput.GetLength(this, DA, 3, lengthUnit),
              GetInput.GetLength(this, DA, 4, lengthUnit),
              GetInput.GetLength(this, DA, 5, lengthUnit));
        }

        // IIBeamAsymmetricalProfile
        else if (typ.Name.Equals(typeof(IIBeamAsymmetricalProfile).Name))
        {
          profile = IIBeamAsymmetricalProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.Flange(this, DA, 1),
              GetInput.Flange(this, DA, 2),
              GetInput.Web(this, DA, 3));
        }

        // IIBeamCellularProfile
        else if (typ.Name.Equals(typeof(IIBeamCellularProfile).Name))
        {
          profile = IIBeamCellularProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.Flange(this, DA, 1),
              (IWebConstant)GetInput.Web(this, DA, 2),
              GetInput.GetLength(this, DA, 3, lengthUnit));
        }

        // IIBeamSymmetricalProfile
        else if (typ.Name.Equals(typeof(IIBeamSymmetricalProfile).Name))
        {
          profile = IIBeamSymmetricalProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.Flange(this, DA, 1),
              (IWebConstant)GetInput.Web(this, DA, 2));
        }

        // IRectangleHollowProfile
        else if (typ.Name.Equals(typeof(IRectangleHollowProfile).Name))
        {
          profile = IRectangleHollowProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.Flange(this, DA, 1),
              (IWebConstant)GetInput.Web(this, DA, 2));
        }

        // IRectangleProfile
        else if (typ.Name.Equals(typeof(IRectangleProfile).Name))
        {
          profile = IRectangleProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit));
        }

        // IRectoEllipseProfile
        else if (typ.Name.Equals(typeof(IRectoEllipseProfile).Name))
        {
          profile = IRectoEllipseProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit),
              GetInput.GetLength(this, DA, 2, lengthUnit),
              GetInput.GetLength(this, DA, 3, lengthUnit));
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
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit),
              pileCount, isWallNotSection);
        }

        // ISheetPileProfile
        else if (typ.Name.Equals(typeof(ISheetPileProfile).Name))
        {
          profile = ISheetPileProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit),
              GetInput.GetLength(this, DA, 2, lengthUnit),
              GetInput.GetLength(this, DA, 3, lengthUnit),
              GetInput.GetLength(this, DA, 4, lengthUnit),
              GetInput.GetLength(this, DA, 5, lengthUnit));
        }

        // IStadiumProfile
        else if (typ.Name.Equals(typeof(IStadiumProfile).Name))
        {
          profile = IStadiumProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit));
        }

        // ITrapezoidProfile
        else if (typ.Name.Equals(typeof(ITrapezoidProfile).Name))
        {
          profile = ITrapezoidProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.GetLength(this, DA, 1, lengthUnit),
              GetInput.GetLength(this, DA, 2, lengthUnit));
        }

        // ITSectionProfile
        else if (typ.Name.Equals(typeof(ITSectionProfile).Name))
        {
          profile = ITSectionProfile.Create(
              GetInput.GetLength(this, DA, 0, lengthUnit),
              GetInput.Flange(this, DA, 1),
              GetInput.Web(this, DA, 2));
        }

        // IPerimeterProfile (last chance...)
        else if (typ.Name.Equals(typeof(IPerimeterProfile).Name))
        {
          //profile = GetInput.Boundaries(this, DA, 0, 1, lengthUnit);
          DA.SetData(0, GetInput.Boundaries(this, DA, 0, 1, lengthUnit, true));
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
    private bool first = true;

    private void Mode1Clicked()
    {
      if (_mode == FoldMode.Catalogue)
        if (!comingFromSave) { return; }

      // remove plane
      IGH_Param param_Plane = Params.Input[Params.Input.Count - 1];
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

      //remove input parameters
      while (Params.Input.Count > 0)
        Params.UnregisterInputParameter(Params.Input[0], true);

      //register input parameter
      Params.RegisterInputParam(new Param_String());
      Params.RegisterInputParam(new Param_Boolean());

      // add plane
      Params.RegisterInputParam(param_Plane);

      _mode = FoldMode.Catalogue;

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
      ExpireSolution(true);
    }
    private void SetNumberOfGenericInputs(int inputs, bool isSecantPile = false)
    {
      numberOfInputs = inputs;

      // if last input previously was a bool and we no longer need that
      if (lastInputWasSecant || isSecantPile)
      {
        if (Params.Input.Count > 0)
        {
          // make sure to remove last param
          Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], true);
          Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], true);
        }
      }

      // remove any additional inputs
      while (Params.Input.Count > inputs)
        Params.UnregisterInputParameter(Params.Input[inputs], true);

      if (isSecantPile) // add two less generic than input says
      {
        while (Params.Input.Count > inputs + 2)
          Params.UnregisterInputParameter(Params.Input[inputs + 2], true);
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
    private Type typ = typeof(IRectangleProfile);

    private void Mode2Clicked()
    {
      // remove plane
      IGH_Param param_Plane = Params.Input[Params.Input.Count - 1];
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

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
      Params.RegisterInputParam(param_Plane);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
      ExpireSolution(true);
    }
    #endregion
  }
}