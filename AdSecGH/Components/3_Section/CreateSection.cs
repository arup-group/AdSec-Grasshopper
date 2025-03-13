using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysGH;

using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class CreateSectionGh : CreateSectionFunction {
    public CreateSectionGh() {
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

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { AdSecSection, };
    }
  }

  public class CreateSection : ComponentAdapter<CreateSectionGh> {

    public override Guid ComponentGuid => new Guid("af6a8179-5e5f-498c-a83c-e98b90d4464c");

    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateSection;
  }
}
