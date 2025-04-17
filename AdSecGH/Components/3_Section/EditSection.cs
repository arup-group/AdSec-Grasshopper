using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class EditSectionGh : EditSectionFunction {

    public EditSectionGh() {
      var adSecSection = AdSecSection as Attribute;
      Section.Update(ref adSecSection);
      AdSecSection.OnValueChanged += goo => {
        if (goo.Value != null) {
          Section.Value = new SectionDesign {
            Section = goo.Value.Section,
            DesignCode = new DesignCode {
              IDesignCode = goo.Value.DesignCode,
              DesignCodeName = goo.Value._codeName,
            },
          };
        }
      };

      var adSecSectionOut = AdSecSectionOut as Attribute;
      SectionOut.Update(ref adSecSectionOut);
      SectionOut.OnValueChanged += design => {
        if (design != null) {
          AdSecSectionOut.Value = new AdSecSectionGoo(new AdSecSection(design));
        }
      };
    }

    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();
    public AdSecSectionParameter AdSecSectionOut { get; set; } = new AdSecSectionParameter();

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        AdSecSection, Profile, Material, DesignCode, RebarGroup, SubComponent,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        AdSecSectionOut, ProfileOut, MaterialOut, DesignCodeOut, RebarGroupOut, SubComponentOut, Geometry,
      };
    }
  }

  public class EditSection : ComponentAdapter<EditSectionGh> {

    public override Guid ComponentGuid => new Guid("9b0acde5-f57f-4a39-a9c3-cdc935037490");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.EditSection;
  }
}
