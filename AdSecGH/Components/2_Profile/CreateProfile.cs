using System;
using System.Drawing;
using System.IO;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using GH_IO.Serialization;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Oasys.GH.Helpers;

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

    public override bool Read(GH_IReader reader) {
      bool result = base.Read(reader);
      SyncPlaneParameterMetadata();
      return result;
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

    protected override void SolveInternal(IGH_DataAccess DA) {
      ClearRuntimeMessages();
      Params.Input.ForEach(input => input.ClearRuntimeMessages());

      var localPlane = GetLocalPlane(DA, Plane.WorldYZ);
      if (_mode == FoldMode.Catalogue) {
        var profiles = SolveInstanceForCatalogueProfile(DA);
        var adSecProfile = AdSecProfiles.CreateProfile(profiles[0]);
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, Plane.WorldYZ, localPlane));
      } else if (_mode == FoldMode.Other) {
        localPlane = GetLocalPlane(DA, Plane.Unset);
        var profile = SolveInstanceForStandardProfile(DA);
        var adSecProfile = AdSecProfiles.CreateProfile(profile);
        DA.SetData(0, new AdSecProfileGoo(adSecProfile, PerimeterProfilePlane, localPlane));
      }
    }

    private Plane GetLocalPlane(IGH_DataAccess DA, Plane plane) {
      var localPlane = plane;
      if (DA.GetData(Params.Input.Count - 1, ref localPlane)) {
        return localPlane;
      }
      return localPlane;
    }

    private void SyncPlaneParameterMetadata() {
      var attributes = BusinessComponent.GetAllInputAttributes();
      // Plane is always the trailing input for this component in both modes.
      var planeParam = Params.Input[Params.Input.Count - 1];
      var planeAttribute = attributes[attributes.Length - 1];
      planeParam.Name = planeAttribute.Name;
      planeParam.NickName = planeAttribute.NickName;
      planeParam.Description = planeAttribute.Description;
    }
  }
}
