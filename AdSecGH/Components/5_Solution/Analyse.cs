using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class AnalyseGh : AnalyseFunction {
    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();

    public AnalyseGh() {
      var adSecSection = AdSecSection as Attribute;
      Section.Update(ref adSecSection);
      AdSecSection.OnValueChanged += goo => {
        if (goo.Value != null) {
          Section.Value = new SectionDesign() {
            Section = goo.Value.Section,
            DesignCode = new DesignCode() {
              IDesignCode = goo.Value.DesignCode,
              DesignCodeName = goo.Value._codeName,
            },
            LocalPlane = goo.Value.LocalPlane.ToOasys()
          };
        }
      };
    }

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        AdSecSection,
      };
    }
  }

  public class Analyse : ComponentAdapter<AnalyseGh> {

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("4621cc01-0b76-4f58-b24e-81e32ae24f92");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Solution;
  }
}
