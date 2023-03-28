using System;
using System.Collections.Generic;
using System.IO;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Oasys.Interop;
using Oasys.Taxonomy.Profiles;
using OasysGH;
using OasysGH.Components;
using OasysUnits;
using Rhino.Geometry;

namespace AdSecGH.Components
{
  /// <summary>
  /// Component to create AdSec profile
  /// </summary>
  public class CreateProfile : CreateOasysProfile
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("ea0741e5-905e-4ecb-8270-a584e3f99aa3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    public override string DataSource => Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3");
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateProfile;

    public CreateProfile() : base(
      "Create Profile",
      "Profile",
      "Create Profile for AdSec Section",
      CategoryName.Name(),
      SubCategoryName.Cat2())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }

    protected override string HtmlHelp_Source()
    {
      string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.Profiles.html";
      return help;
    }
    #endregion

    #region Input and output
    //List<string> spacerDescriptions = new List<string>(new string[]
    //{
    //        "Profile type", "Measure", "Type", "Profile"
    //});
    //List<string> excludedInterfaces = new List<string>(new string[]
    //{
    //    "IProfile", "IPoint", "IPolygon", "IFlange", "IWeb", "IWebConstant", "IWebTapered", "ITrapezoidProfileAbstractInterface", "IIBeamProfile"
    //});
    //Dictionary<string, Type> profileTypes;
    //Dictionary<string, FieldInfo> profileFields;
    //// Sections
    //List<string> filteredlist = new List<string>();
    //int catalogueIndex = -1; //-1 is all
    //int typeIndex = -1;
    //// displayed selections
    //string typeName = "All";
    //string sectionName = "All";
    //// list of sections as outcome from selections
    //string profileString = "HE HE200.B";
    //string search = "";

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
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

      if (this._mode == FoldMode.Catalogue)
      {
        List<IProfile> profiles = this.SolveInstanceForCatalogueProfile(DA);

        if (profiles.Count > 100)
        {

          DataTree<string> tree = new DataTree<string>();

          int pathCount = 0;
          if (this.Params.Output[0].VolatileDataCount > 0)
            pathCount = this.Params.Output[0].VolatileData.PathCount;

          GH_Path path = new GH_Path(new int[] { pathCount });
          tree.AddRange(this._profileDescriptions, path);

          DA.SetDataTree(0, tree);
        }
        else
        {
          List<AdSecProfileGoo> adSecProfiles = new List<AdSecProfileGoo>();
          foreach (IProfile profile in profiles)
            adSecProfiles.Add(new AdSecProfileGoo(AdSecProfiles.CreateProfile(profile), Plane.WorldYZ));

          DataTree<AdSecProfileGoo> tree = new DataTree<AdSecProfileGoo>();

          int pathCount = 0;
          if (this.Params.Output[0].VolatileDataCount > 0)
            pathCount = this.Params.Output[0].VolatileData.PathCount;

          GH_Path path = new GH_Path(new int[] { pathCount });
          tree.AddRange(adSecProfiles, path);

          DA.SetDataTree(0, tree);
        }
      }
      else if (this._mode == FoldMode.Other)
      {
        IProfile profile = this.SolveInstanceForStandardProfile(DA);
        Oasys.Profiles.IProfile adSecProfile = AdSecProfiles.CreateProfile(profile);

        //try
        //{
        //    Oasys.Collections.IList<Oasys.AdSec.IWarning> warn = profile.Validate();
        //    foreach (Oasys.AdSec.IWarning warning in warn)
        //        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, warning.Description);
        //}
        //catch (Exception e)
        //{
        //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
        //    return;
        //}

        Plane local = Plane.WorldYZ;
        Plane temp = Plane.Unset;
        if (DA.GetData(Params.Input.Count - 1, ref temp))
          local = temp;

        DA.SetData(0, new AdSecProfileGoo(adSecProfile, local));
        return;
      }
    }

    protected override void Mode1Clicked()
    {
      // remove plane
      IGH_Param plane = Params.Input[Params.Input.Count - 1];
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

      // remove input parameters
      while (this.Params.Input.Count > 0)
        this.Params.UnregisterInputParameter(this.Params.Input[0], true);

      // register input parameter
      this.Params.RegisterInputParam(new Param_String());
      this.Params.RegisterInputParam(new Param_Boolean());

      // add plane
      Params.RegisterInputParam(plane);

      this._mode = FoldMode.Catalogue;

      base.UpdateUI();
    }

    protected override void Mode2Clicked()
    {
      IGH_Param plane = Params.Input[Params.Input.Count - 1];
      // remove plane
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

      this.UpdateParameters();

      // add plane
      Params.RegisterInputParam(plane);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
      ExpireSolution(true);
    }
  }
}
