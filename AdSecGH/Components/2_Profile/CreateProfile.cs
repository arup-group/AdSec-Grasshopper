using System;
using System.Drawing;
using System.IO;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using OasysGH;

using Rhino.Geometry;

namespace AdSecGH.Components {
  /// <summary>
  ///   Component to create AdSec profile
  /// </summary>
  public class CreateProfile : ProfileAdapter<CreateProfileFunction> {

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("ea0741e5-905e-4ecb-8270-a584e3f99aa3");
    public override string DataSource => Path.Combine(AddReferencePriority.PluginPath, "sectlib.db3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateProfile;

    protected override string HtmlHelp_Source() {
      string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.Profiles.html";
      return help;
    }

    protected override void Mode1Clicked() {
      // remove plane
      var plane = Params.Input[Params.Input.Count - 1];
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

      // remove input parameters
      while (Params.Input.Count > 0) {
        Params.UnregisterInputParameter(Params.Input[0], true);
      }

      // register input parameter
      Params.RegisterInputParam(new Param_String());
      Params.RegisterInputParam(new Param_Boolean());

      // add plane
      Params.RegisterInputParam(plane);

      _mode = FoldMode.Catalogue;

      base.UpdateUI();
    }

    protected override void Mode2Clicked() {
      var plane = Params.Input[Params.Input.Count - 1];
      // remove plane
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);

      // check if mode is correct
      if (_mode != FoldMode.Other) {
        // if we come from catalogue mode remove all input parameters
        while (Params.Input.Count > 0) {
          Params.UnregisterInputParameter(Params.Input[0], true);
        }

        // set mode to other
        _mode = FoldMode.Other;
      }

      UpdateParameters();

      // add plane
      Params.RegisterInputParam(plane);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
      ExpireSolution(true);
    }

    //protected override void RegisterInputParams(GH_InputParamManager pManager) {
    //  string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
    //  pManager.AddGenericParameter($"Width [{unitAbbreviation}]", "B", "Profile width", GH_ParamAccess.item);

    //  pManager.AddGenericParameter($"Depth [{unitAbbreviation}]", "H", "Profile depth", GH_ParamAccess.item);

    //  pManager.AddPlaneParameter("LocalPlane", "P",
    //    "[Optional] Plane representing local coordinate system, by default a YZ-plane is used", GH_ParamAccess.item,
    //    Plane.WorldYZ);
    //  pManager.HideParameter(2);
    //}

    //protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
    //  pManager.AddGenericParameter("Profile", "Pf", "Profile for AdSec Section", GH_ParamAccess.item);
    //}

    protected override void SolveInternal(IGH_DataAccess DA) {
      ClearRuntimeMessages();
      for (int i = 0; i < Params.Input.Count; i++) {
        Params.Input[i].ClearRuntimeMessages();
      }

      var local = Plane.WorldYZ;
      var temp = Plane.Unset;
      if (DA.GetData(Params.Input.Count - 1, ref temp)) {
        local = temp;
      }

      if (_mode == FoldMode.Catalogue) {
        var profiles = SolveInstanceForCatalogueProfile(DA);
        var adSecProfile = AdSecProfiles.CreateProfile(profiles[0]);
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, local));
      } else if (_mode == FoldMode.Other) {
        var profile = SolveInstanceForStandardProfile(DA);
        var adSecProfile = AdSecProfiles.CreateProfile(profile);
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, local));
      }
    }
  }
}
