using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

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

      var adSecOffset = AdSecOffset as Attribute;
      Offset.Update(ref adSecOffset);
      AdSecOffset.OnValueChanged += goo => {
        if (goo.Value != null) {
          Offset.Value = goo.AdSecPoint;
        }
      };
    }

    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();
    public AdSecPointParameter AdSecOffset { get; set; } = new AdSecPointParameter();

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        AdSecSection,
        AdSecOffset,
      };
    }
  }

  public class CreateSubcomponent : ComponentAdapter<CreateSubComponentComponentGh> {

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("38747c89-01a4-4388-921b-8c8d8cbca410");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.SubComponent;
  }
}
