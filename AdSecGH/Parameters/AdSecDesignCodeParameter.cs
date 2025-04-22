using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using AdSecGH.Helpers;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

namespace AdSecGH.Parameters {
  public class AdSecDesignCodeParameter : GH_PersistentParam<AdSecDesignCodeGoo> {

    public AdSecDesignCodeParameter() : base(new GH_InstanceDescription("DesignCode", "Code",
      "AdSec DesignCode Parameter", CategoryName.Name(), SubCategoryName.Cat8())) { }

    public override Guid ComponentGuid => new Guid("6d656276-61f6-47ce-81bc-9fabdd39edc2");
    public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;
    public static bool Hidden => true;
    public static bool IsPreviewCapable => false;

    protected override Bitmap Icon => Resources.DesignCodeParameter;

    protected override ToolStripMenuItem Menu_CustomMultiValueItem() {
      return ToolStripMenuHelper.CreateInvisibleMenuItem();
    }

    protected override ToolStripMenuItem Menu_CustomSingleValueItem() {
      return ToolStripMenuHelper.CreateInvisibleMenuItem();
    }

    protected override GH_GetterResult Prompt_Plural(ref List<AdSecDesignCodeGoo> values) {
      return GH_GetterResult.cancel;
    }

    protected override GH_GetterResult Prompt_Singular(ref AdSecDesignCodeGoo value) {
      return GH_GetterResult.cancel;
    }
  }
}
