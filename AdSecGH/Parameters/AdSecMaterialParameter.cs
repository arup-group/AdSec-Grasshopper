using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace AdSecGH.Parameters
{
  public class AdSecMaterialParameter : GH_PersistentParam<AdSecMaterialGoo>
  {
    public override Guid ComponentGuid => new Guid("cf5636e2-628d-4794-ab29-97f83002db34");
    public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.MaterialParam;

    public AdSecMaterialParameter() : base(new GH_InstanceDescription(
      "Material",
      "Mat",
      "AdSec Material Parameter",
      Components.Ribbon.CategoryName.Name(),
      Components.Ribbon.SubCategoryName.Cat9()))
    { }

    protected override GH_GetterResult Prompt_Plural(ref List<AdSecMaterialGoo> values)
    {
      return GH_GetterResult.cancel;
    }

    protected override GH_GetterResult Prompt_Singular(ref AdSecMaterialGoo value)
    {
      return GH_GetterResult.cancel;
    }

    protected override ToolStripMenuItem Menu_CustomSingleValueItem()
    {
      ToolStripMenuItem item = new ToolStripMenuItem
      {
        Text = "Not available",
        Visible = false
      };
      return item;
    }

    protected override ToolStripMenuItem Menu_CustomMultiValueItem()
    {
      ToolStripMenuItem item = new ToolStripMenuItem
      {
        Text = "Not available",
        Visible = false
      };
      return item;
    }

    #region preview methods
    public bool Hidden
    {
      get { return true; }
      //set { m_hidden = value; }
    }

    public bool IsPreviewCapable
    {
      get { return false; }
    }
    #endregion
  }
}
