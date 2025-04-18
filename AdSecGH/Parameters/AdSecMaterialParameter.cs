using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using AdSecGH.Helpers;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

namespace AdSecGH.Parameters {
  public class AdSecMaterialParameter : GH_PersistentParam<AdSecMaterialGoo> {

    public AdSecMaterialParameter() : base(new GH_InstanceDescription("Material", "Mat", "AdSec Material Parameter",
      CategoryName.Name(), SubCategoryName.Cat8())) { }

    public override Guid ComponentGuid => new Guid("cf5636e2-628d-4794-ab29-97f83002db34");
    public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;
    protected override Bitmap Icon => Resources.MaterialParam;
    public bool Hidden => true;
    public bool IsPreviewCapable => false;

    protected override ToolStripMenuItem Menu_CustomMultiValueItem() {
      return ToolStripMenuHelper.CreateInvisibleMenuItem();
    }

    protected override ToolStripMenuItem Menu_CustomSingleValueItem() {
      return ToolStripMenuHelper.CreateInvisibleMenuItem();
    }

    protected override GH_GetterResult Prompt_Plural(ref List<AdSecMaterialGoo> values) {
      return GH_GetterResult.cancel;
    }

    protected override GH_GetterResult Prompt_Singular(ref AdSecMaterialGoo value) {
      return GH_GetterResult.cancel;
    }
  }
}
