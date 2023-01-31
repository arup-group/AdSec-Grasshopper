using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace AdSecGH.Parameters
{
  public class AdSecRebarGroupParameter : GH_PersistentParam<AdSecRebarGroupGoo>
  {
    public override Guid ComponentGuid => new Guid("6d666276-61f6-47ce-81bc-9fabdd39edc2");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public bool Hidden
    {
      get
      {
        return true;
      }
      //set { m_hidden = value; }
    }
    public bool IsPreviewCapable
    {
      get
      {
        return false;
      }
    }
    protected override Bitmap Icon => Properties.Resources.RebarGroupParam;

    public AdSecRebarGroupParameter() : base(new GH_InstanceDescription(
      "RebarGroup",
      "RbG",
      "AdSec RebarGroup Parameter",
      Components.Ribbon.CategoryName.Name(),
      Components.Ribbon.SubCategoryName.Cat9()))
    {
    }

    #region methods
    protected override GH_GetterResult Prompt_Plural(ref List<AdSecRebarGroupGoo> values)
    {
      return GH_GetterResult.cancel;
    }

    protected override GH_GetterResult Prompt_Singular(ref AdSecRebarGroupGoo value)
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
    #endregion
  }
}
