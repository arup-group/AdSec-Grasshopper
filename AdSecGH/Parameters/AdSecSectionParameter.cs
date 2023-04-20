using AdSecGH.Helpers.GH;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AdSecGH.Parameters {
  public class AdSecSectionParameter : GH_PersistentGeometryParam<AdSecSectionGoo>, IGH_PreviewObject {
    public BoundingBox ClippingBox {
      get {
        return Preview_ComputeClippingBox();
      }
    }
    public override Guid ComponentGuid => new Guid("fa647c2d-4767-49f1-a574-32bf66a66568");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public bool Hidden { get; set; }
    public bool IsPreviewCapable {
      get {
        return true;
      }
    }
    protected override Bitmap Icon => Properties.Resources.SectionParam;

    public AdSecSectionParameter() : base(new GH_InstanceDescription(
      "Section",
      "Sec",
      "Maintains a collection of AdSec Section data.",
      CategoryName.Name(),
      SubCategoryName.Cat9())) {
    }

    public void DrawViewportMeshes(IGH_PreviewArgs args) {
      //Use a standard method to draw gunk, you don't have to specifically implement 
      Preview_DrawMeshes(args);
    }

    public void DrawViewportWires(IGH_PreviewArgs args) {
      //Use a standard method to draw gunk, you don't have to specifically implement 
      Preview_DrawWires(args);
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

    //We do not allow users to pick parameter,
    //therefore the following 4 methods disable all this ui.
    protected override GH_GetterResult Prompt_Plural(ref List<AdSecSectionGoo> values) {
      return GH_GetterResult.cancel;
    }

    protected override GH_GetterResult Prompt_Singular(ref AdSecSectionGoo value) {
      return GH_GetterResult.cancel;
    }
  }
}
