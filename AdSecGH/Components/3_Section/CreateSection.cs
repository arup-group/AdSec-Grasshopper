using System;
using System.Collections.Specialized;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

// using AdSecMaterialParameter = Oasys.GH.Helpers.AdSecMaterialParameter;
using AdSecSectionParameter = Oasys.GH.Helpers.AdSecSectionParameter;
using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGH.Components {

  public class CreateSectionGh : CreateSectionFunction {
    public CreateSectionGh() {
      var adSecSection = AdSecSection as Attribute;
      Section.Update(ref adSecSection);

      Section.OnValueChanged += goo => {
        if (goo.Section != null) {
          AdSecSection.Value = new AdSecSectionGoo(new AdSecSection(goo));
        }
      };
    }

    public AdSecSectionParameter AdSecSection { get; set; } = new AdSecSectionParameter();

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] { Profile, Material, RebarGroup, SubComponent };
    }

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
