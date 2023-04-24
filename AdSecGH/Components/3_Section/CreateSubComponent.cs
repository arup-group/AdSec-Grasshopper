using System;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using Oasys.Profiles;
using OasysGH;
using OasysGH.Components;
using OasysUnits;

namespace AdSecGH.Components {
  public class CreateSubcomponent : GH_OasysComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("38747c89-01a4-4388-921b-8c8d8cbca410");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.SubComponent;

    public CreateSubcomponent() : base(
      "SubComponent",
      "SubComponent",
      "Create an AdSec Subcomponent from a Section",
      CategoryName.Name(),
      SubCategoryName.Cat4()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to create Subcomponent from", GH_ParamAccess.item);
      pManager.AddGenericParameter("Offset", "Off", "[Optional] Section offset (Vertex Point)." + Environment.NewLine + "Offset is applied between origins of containing section and sub-component. The offset of the profile is in the containing section's Profile Coordinate System. Any rotation applied to the containing section's profile will be applied to its sub-components. Sub-components can also have an additional rotation for their profiles.", GH_ParamAccess.item);
      // make all but first input optional
      for (int i = 1; i < pManager.ParamCount; i++) {
        pManager[i].Optional = true;
      }
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("SubComponent", "Sub", "AdSet Subcomponent", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      AdSecSection section = AdSecInput.AdSecSection(this, DA, 0);
      if (section == null) {
        return;
      }
      IPoint offset = AdSecInput.IPoint(this, DA, 1, true) ?? IPoint.Create(Length.Zero, Length.Zero);
      var subComponent = ISubComponent.Create(section.Section, offset);
      var subGoo = new AdSecSubComponentGoo(subComponent, section.LocalPlane, section.DesignCode, section._codeName, section._materialName);
      DA.SetData(0, subGoo);
    }
  }
}
