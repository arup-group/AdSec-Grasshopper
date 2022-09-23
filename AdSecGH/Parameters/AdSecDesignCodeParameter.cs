using System;
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace AdSecGH.Parameters
{
  /// <summary>
  /// This class provides a Parameter interface for the Data_GsaBool6 type.
  /// </summary>
  public class AdSecDesignCodeParameter : GH_PersistentParam<AdSecDesignCodeGoo>
  {
    public AdSecDesignCodeParameter()
      : base(new GH_InstanceDescription("DesignCode", "Code", "AdSec DesignCode Parameter", Components.Ribbon.CategoryName.Name(), Components.Ribbon.SubCategoryName.Cat9()))
    {
    }

    public override Guid ComponentGuid => new Guid("6d656276-61f6-47ce-81bc-9fabdd39edc2");

    public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;

    protected override System.Drawing.Bitmap Icon => Properties.Resources.DesignCodeParameter;

    protected override GH_GetterResult Prompt_Plural(ref List<AdSecDesignCodeGoo> values)
    {
      return GH_GetterResult.cancel;
    }
    protected override GH_GetterResult Prompt_Singular(ref AdSecDesignCodeGoo value)
    {
      return GH_GetterResult.cancel;
    }
    protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomSingleValueItem()
    {
      System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
      {
        Text = "Not available",
        Visible = false
      };
      return item;
    }
    protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomMultiValueItem()
    {
      System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
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
