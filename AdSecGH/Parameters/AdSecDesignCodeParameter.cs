using AdSecGH.Helpers.GH;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AdSecGH.Parameters {
  public class AdSecDesignCodeParameter : GH_PersistentParam<AdSecDesignCodeGoo> {
    public override Guid ComponentGuid => new Guid("6d656276-61f6-47ce-81bc-9fabdd39edc2");
    public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;
    public bool Hidden {
      get {
        return true;
      }
      //set { m_hidden = value; }
    }

    public bool IsPreviewCapable {
      get {
        return false;
      }
    }
    protected override System.Drawing.Bitmap Icon => Properties.Resources.DesignCodeParameter;

    public AdSecDesignCodeParameter() : base(new GH_InstanceDescription(
      "DesignCode",
      "Code",
      "AdSec DesignCode Parameter",
      CategoryName.Name(),
      SubCategoryName.Cat9())) {
    }

    protected override ToolStripMenuItem Menu_CustomMultiValueItem() {
      ToolStripMenuItem item = new ToolStripMenuItem {
        Text = "Not available",
        Visible = false
      };
      return item;
    }

    protected override ToolStripMenuItem Menu_CustomSingleValueItem() {
      ToolStripMenuItem item = new ToolStripMenuItem {
        Text = "Not available",
        Visible = false
      };
      return item;
    }

    protected override GH_GetterResult Prompt_Plural(ref List<AdSecDesignCodeGoo> values) {
      return GH_GetterResult.cancel;
    }

    protected override GH_GetterResult Prompt_Singular(ref AdSecDesignCodeGoo value) {
      return GH_GetterResult.cancel;
    }
  }
}
