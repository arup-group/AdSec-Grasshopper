using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace AdSecGH.Parameters {
  public class AdSecSectionGoo : GH_GeometricGoo<AdSecSection>, IGH_PreviewData {
    public override BoundingBox Boundingbox {
      get {
        if (Value == null)
          return BoundingBox.Empty;
        if (Value.SolidBrep == null)
          return BoundingBox.Empty;
        return Value.SolidBrep.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox {
      get {
        return Boundingbox;
      }
    }
    public override bool IsValid {
      get {
        if (Value == null)
          return false;
        if (Value.SolidBrep == null || Value.IsValid == false)
          return false;
        return true;
      }
    }
    public override string IsValidWhyNot {
      get {
        if (Value.IsValid)
          return string.Empty;
        return Value.IsValid.ToString();
      }
    }
    public override string TypeDescription => "AdSec " + TypeName + " Parameter";
    public override string TypeName => "Section";

    public AdSecSectionGoo() {
    }

    public AdSecSectionGoo(AdSecSection section) {
      if (section == null)
        Value = null;
      else
        Value = section.Duplicate();
    }

    public override bool CastFrom(object source) {
      if (source == null)
        return false;

      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      //if (InteropAdSecComputeTypes.IsPresent())
      //{
      //  Type type = InteropAdSecComputeTypes.GetType(typeof(IAdSecSection));
      //  if (typeof(Q).IsAssignableFrom(type))
      //  {
      //    if (Value == null)
      //      target = default;
      //    else
      //    {
      //      target = (Q)(object)InteropAdSecComputeTypes.CastToSection(Value.Section, Value.codeName, Value.materialName);
      //    }
      //    return true;
      //  }
      //}
      if (typeof(Q).IsAssignableFrom(typeof(AdSecSectionGoo))) {
        if (Value == null)
          target = default;
        else
          target = (Q)(object)Value.Duplicate();
        return true;
      }
      if (typeof(Q).IsAssignableFrom(typeof(AdSecSection))) {
        if (Value == null)
          target = default;
        else
          target = (Q)(object)new AdSecSection(Value.Section, Value.DesignCode, Value._codeName, Value._materialName, Value.LocalPlane);
        return true;
      }
      if (typeof(Q).IsAssignableFrom(typeof(AdSecProfileGoo))) {
        if (Value == null)
          target = default;
        else
          target = (Q)(object)new AdSecProfileGoo(Value.Section.Profile, Value.LocalPlane);
        return true;
      }
      if (typeof(Q).IsAssignableFrom(typeof(Brep))) {
        if (Value == null)
          target = default;
        else
          target = (Q)(object)Value.SolidBrep.DuplicateBrep();
        return true;
      }
      if (typeof(Q).IsAssignableFrom(typeof(GH_Brep))) {
        if (Value == null)
          target = default;
        else
          target = (Q)(object)Value.SolidBrep.DuplicateBrep();
        return true;
      }

      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
      //Draw shape.
      if (Value.SolidBrep != null) {
        // draw profile
        args.Pipeline.DrawBrepShaded(Value.SolidBrep, Value.m_profileColour);
        // draw subcomponents
        for (int i = 0; i < Value._subProfiles.Count; i++) {
          args.Pipeline.DrawBrepShaded(Value._subProfiles[i], Value.m_subColours[i]);
        }
        // draw rebars
        for (int i = 0; i < Value.m_rebars.Count; i++) {
          args.Pipeline.DrawBrepShaded(Value.m_rebars[i], Value.m_rebarColours[i]);
        }
      }
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value == null) { return; }

      Color defaultCol = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
      {
        args.Pipeline.DrawPolyline(Value.m_profileEdge, UI.Colour.OasysBlue, 2);
        if (Value.m_profileVoidEdges != null) {
          foreach (Polyline crv in Value.m_profileVoidEdges) {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
          }
        }
        if (Value.m_subEdges != null) {
          foreach (Polyline crv in Value.m_subEdges) {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
          }
        }
        if (Value.m_subVoidEdges != null) {
          foreach (List<Polyline> crvs in Value.m_subVoidEdges) {
            foreach (Polyline crv in crvs) {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysBlue, 1);
            }
          }
        }
        if (Value.m_rebarEdges != null) {
          foreach (Circle crv in Value.m_rebarEdges) {
            args.Pipeline.DrawCircle(crv, Color.Black, 1);
          }
        }
        if (Value.m_linkEdges != null) {
          foreach (Curve crv in Value.m_linkEdges) {
            args.Pipeline.DrawCurve(crv, Color.Black, 1);
          }
        }
      }
      else // selected
      {
        args.Pipeline.DrawPolyline(Value.m_profileEdge, UI.Colour.OasysYellow, 3);
        if (Value.m_profileVoidEdges != null) {
          foreach (Polyline crv in Value.m_profileVoidEdges) {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
          }
        }
        if (Value.m_subEdges != null) {
          foreach (Polyline crv in Value.m_subEdges) {
            args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
          }
        }
        if (Value.m_subVoidEdges != null) {
          foreach (List<Polyline> crvs in Value.m_subVoidEdges) {
            foreach (Polyline crv in crvs) {
              args.Pipeline.DrawPolyline(crv, UI.Colour.OasysYellow, 2);
            }
          }
        }
        if (Value.m_rebarEdges != null) {
          foreach (Circle crv in Value.m_rebarEdges) {
            args.Pipeline.DrawCircle(crv, UI.Colour.UILightGrey, 2);
          }
        }
        if (Value.m_linkEdges != null) {
          foreach (Curve crv in Value.m_linkEdges) {
            args.Pipeline.DrawCurve(crv, UI.Colour.UILightGrey, 2);
          }
        }
      }
      // local axis
      if (Value.previewXaxis != null) {
        args.Pipeline.DrawLine(Value.previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
        args.Pipeline.DrawLine(Value.previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
        args.Pipeline.DrawLine(Value.previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
      }
    }

    public AdSecSectionGoo DuplicateAdSecSection() {
      if (Value == null)
        return null;
      else
        return new AdSecSectionGoo(Value.Duplicate());
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return DuplicateAdSecSection();
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      if (Value == null)
        return BoundingBox.Empty;
      if (Value.SolidBrep == null)
        return BoundingBox.Empty;
      return Value.SolidBrep.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      //return new AdSecSectionGoo(Value.Morph(xmorph));
      return null;
    }

    public override string ToString() {
      if (Value == null)
        return "Null AdSec Section";
      else
        return "AdSec " + TypeName + " {" + Value.ToString() + "}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      //return new AdSecSectionGoo(Value.Transform(xform));
      return null;
    }
  }
}
