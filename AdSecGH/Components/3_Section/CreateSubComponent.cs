using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysGH;

using OasysUnits;

using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {
  public class CreateSubComponentComponentGh : CreateSubComponentFunction {

    public CreateSubComponentComponentGh() {
      var adSecSection = AdSecSection as Attribute;
      Section.Update(ref adSecSection);
      AdSecSection.OnValueChanged += goo => {
        if (goo.Value != null) {
          Section.Value = new SectionDesign {
            Section = goo.Value.Section,
            DesignCode = goo.Value.DesignCode,
          };
        }
      };
    }

    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        AdSecSection,
        Offset,
      };
    }
  }

  public class CreateSubcomponent : ComponentAdapter<CreateSubComponentComponentGh> {

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("38747c89-01a4-4388-921b-8c8d8cbca410");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.SubComponent;

    protected override void SolveInstance(IGH_DataAccess DA) {
      var section = this.GetAdSecSection(DA, 0);
      if (section == null) {
        return;
      }

      var offset = this.GetAdSecPointGoo(DA, 1, true).AdSecPoint ?? IPoint.Create(Length.Zero, Length.Zero);
      var subComponent = ISubComponent.Create(section.Section, offset);
      var subGoo = new AdSecSubComponentGoo(subComponent, section.LocalPlane, section.DesignCode, section._codeName,
        section._materialName);
      DA.SetData(0, subGoo);
    }
  }
}
