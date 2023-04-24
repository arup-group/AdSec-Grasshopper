using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AdSecGH.Helpers.GH;
using Grasshopper.Kernel;

namespace AdSecGH.Parameters {
  public class AdSecRebarGroupParameter : GH_PersistentParam<AdSecRebarGroupGoo> {
    public override Guid ComponentGuid => new Guid("6d666276-61f6-47ce-81bc-9fabdd39edc2");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public bool Hidden => true;
    public bool IsPreviewCapable => false;
    protected override Bitmap Icon => Properties.Resources.RebarGroupParam;

    public AdSecRebarGroupParameter() : base(new GH_InstanceDescription(
      "RebarGroup",
      "RbG",
      "AdSec RebarGroup Parameter",
      CategoryName.Name(),
      SubCategoryName.Cat9())) {
    }

    protected override ToolStripMenuItem Menu_CustomMultiValueItem() {
      var item = new ToolStripMenuItem {
        Text = "Not available",
        Visible = false
      };
      return item;
    }

    protected override ToolStripMenuItem Menu_CustomSingleValueItem() {
      var item = new ToolStripMenuItem {
        Text = "Not available",
        Visible = false
      };
      return item;
    }

    protected override GH_GetterResult Prompt_Plural(ref List<AdSecRebarGroupGoo> values) {
      return GH_GetterResult.cancel;
    }

    protected override GH_GetterResult Prompt_Singular(ref AdSecRebarGroupGoo value) {
      return GH_GetterResult.cancel;
    }
  }
}
