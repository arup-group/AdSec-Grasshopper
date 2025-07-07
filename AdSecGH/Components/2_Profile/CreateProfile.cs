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
      var planeInputCache = UnregisterPlaneInput();
      UnregisterAllInputs();

      Params.RegisterInputParam(new Param_String());
      Params.RegisterInputParam(new Param_Boolean());

      // add cached plane
      Params.RegisterInputParam(planeInputCache);
      _mode = FoldMode.Catalogue;

      base.UpdateUI();
    }

    protected override void Mode2Clicked() {
      var planeInputCache = UnregisterPlaneInput();

      // check if mode is correct
      if (_mode != FoldMode.Other) {
        // if we come from catalogue mode remove all input parameters
        UnregisterAllInputs();
        _mode = FoldMode.Other;
      }

      UpdateParameters();

      Params.RegisterInputParam(planeInputCache);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
      ExpireSolution(true);
    }

    protected void UnregisterAllInputs() {
      while (Params.Input.Count > 0) {
        Params.UnregisterInputParameter(Params.Input[0], true);
      }
    }

    protected IGH_Param UnregisterPlaneInput() {
      var plane = Params.Input[Params.Input.Count - 1];
      Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);
      return plane;
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      ClearRuntimeMessages();
      Params.Input.ForEach(input => input.ClearRuntimeMessages());

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
