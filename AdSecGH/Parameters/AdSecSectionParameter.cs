using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSectionParameter : GH_PersistentGeometryParam<AdSecSectionGoo>, IGH_PreviewObject,
    IGH_BakeAwareObject {

    public AdSecSectionParameter() : base(new GH_InstanceDescription("Section", "Sec",
      "Maintains a collection of AdSec Section data.", CategoryName.Name(), SubCategoryName.Cat9())) { }

    public override Guid ComponentGuid => new Guid("fa647c2d-4767-49f1-a574-32bf66a66568");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    protected override Bitmap Icon => Resources.SectionParam;
    public BoundingBox ClippingBox => Preview_ComputeClippingBox();
    public bool Hidden { get; set; }
    public bool IsPreviewCapable => true;

    public void DrawViewportMeshes(IGH_PreviewArgs args) {
      //Use a standard method to draw gunk, you don't have to specifically implement
      Preview_DrawMeshes(args);
    }

    public void DrawViewportWires(IGH_PreviewArgs args) {
      //Use a standard method to draw gunk, you don't have to specifically implement
      Preview_DrawWires(args);
    }

    protected override ToolStripMenuItem Menu_CustomMultiValueItem() {
      var item = new ToolStripMenuItem {
        Text = "Not available",
        Visible = false,
      };
      return item;
    }

    protected override ToolStripMenuItem Menu_CustomSingleValueItem() {
      var item = new ToolStripMenuItem {
        Text = "Not available",
        Visible = false,
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

    public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids) {
      foreach (var data in m_data) {
        if (data is AdSecSectionGoo goo) {
          goo.BakeGeometry(doc, obj_ids);
        }
      }
    }

    public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids) {
      foreach (var data in m_data) {
        if (data is AdSecSectionGoo goo) {
          goo.BakeGeometry(doc, att, obj_ids);
        }
      }
    }

    public bool IsBakeCapable { get; } = true;
  }
}
