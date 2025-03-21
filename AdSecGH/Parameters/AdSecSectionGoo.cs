using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSectionGoo : GH_GeometricGoo<AdSecSection>, IGH_PreviewData, IGH_BakeAwareObject {
    public override BoundingBox Boundingbox {
      get {
        if (Value == null) {
          return BoundingBox.Empty;
        }

        if (Value.SolidBrep == null) {
          return BoundingBox.Empty;
        }

        return Value.SolidBrep.GetBoundingBox(false);
      }
    }
    public BoundingBox ClippingBox => Boundingbox;
    public override bool IsValid {
      get {
        if (Value == null) {
          return false;
        }

        if (Value.SolidBrep == null || Value.IsValid == false) {
          return false;
        }

        return true;
      }
    }
    public override string IsValidWhyNot {
      get {
        if (Value.IsValid) {
          return string.Empty;
        }

        return Value.IsValid.ToString();
      }
    }
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Section";

    public AdSecSectionGoo() {
      _drawInstructions.Clear();
      _drawInstructions.AddRange(UpdateDrawInstructions(true));
    }

    public AdSecSectionGoo(AdSecSection section) {
      if (section == null) {
        Value = null;
      } else {
        Value = section.Duplicate();
      }

      _drawInstructions.Clear();
      _drawInstructions.AddRange(UpdateDrawInstructions(true));
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

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
        if (Value == null) {
          target = default;
        } else {
          target = (Q)(object)Value.Duplicate();
        }

        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(AdSecSection))) {
        if (Value == null) {
          target = default;
        } else {
          target = (Q)(object)new AdSecSection(Value.Section, Value.DesignCode, Value._codeName, Value._materialName,
            Value.LocalPlane);
        }

        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(AdSecProfileGoo))) {
        if (Value == null) {
          target = default;
        } else {
          target = (Q)(object)new AdSecProfileGoo(Value.Section.Profile, Value.LocalPlane);
        }

        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Brep))) {
        if (Value == null) {
          target = default;
        } else {
          target = (Q)(object)Value.SolidBrep.DuplicateBrep();
        }

        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Brep))) {
        if (Value == null) {
          target = default;
        } else {
          target = (Q)(object)Value.SolidBrep.DuplicateBrep();
        }

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

    private static void Draw(DisplayPipeline pipeline, DrawInstructions geometry) {
      if (geometry is DrawPolyline drawPolyline) {
        pipeline.DrawPolyline(drawPolyline.Polyline, drawPolyline.Color, drawPolyline.Thickness);
      } else if (geometry is DrawCircle drawCircle) {
        pipeline.DrawCircle(drawCircle.Circle, drawCircle.Color, drawCircle.Thickness);
      } else if (geometry is DrawCurve drawCurve) {
        pipeline.DrawCurve(drawCurve.Curve, drawCurve.Color, drawCurve.Thickness);
      }
    }

    public class DrawInstructions {
      public Color Color { get; set; }
      public int Thickness { get; set; }
      public virtual object Geometry { get; set; }
    }

    public class DrawPolyline : DrawInstructions {
      public Polyline Polyline { get; set; }
      public override object Geometry => Polyline;
    }

    public class DrawCircle : DrawInstructions {
      public Circle Circle { get; set; }
      public override object Geometry => Circle;
    }

    public class DrawCurve : DrawInstructions {
      public Curve Curve { get; set; }
      public override object Geometry => Curve;
    }

    public readonly List<DrawInstructions> _drawInstructions = new List<DrawInstructions>();

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value == null) {
        return;
      }

      _drawInstructions.Clear();
      _drawInstructions.AddRange(UpdateDrawInstructions(IsNotSelected(args)));

      foreach (var instruction in _drawInstructions) {
        Draw(args.Pipeline, instruction);
      }

      // local axis
      args.Pipeline.DrawLine(Value.previewZaxis, Color.FromArgb(255, 244, 96, 96), 1);
      args.Pipeline.DrawLine(Value.previewXaxis, Color.FromArgb(255, 96, 244, 96), 1);
      args.Pipeline.DrawLine(Value.previewYaxis, Color.FromArgb(255, 96, 96, 234), 1);
    }

    private List<DrawInstructions> UpdateDrawInstructions(bool isNotSelected) {
      if (Value == null) {
        return new List<DrawInstructions>();
      }

      var drawInstructions = new List<DrawInstructions>();

      var primaryColor = isNotSelected ? UI.Colour.OasysBlue : UI.Colour.OasysYellow;
      var secondaryColor = isNotSelected ? Color.Black : UI.Colour.UILightGrey;
      var primaryThickness = isNotSelected ? 2 : 3;
      var secondaryThickness = isNotSelected ? 1 : 2;

      drawInstructions.Add(new DrawPolyline() { Polyline = Value.m_profileEdge, Color = primaryColor, Thickness = primaryThickness });

      foreach (Polyline polyline in Value.m_profileVoidEdges) {
        drawInstructions.Add(new DrawPolyline() { Polyline = polyline, Color = secondaryColor, Thickness = secondaryThickness });
      }

      foreach (Polyline polyline in Value.m_subEdges) {
        drawInstructions.Add(new DrawPolyline() { Polyline = polyline, Color = primaryColor, Thickness = secondaryThickness });
      }

      foreach (List<Polyline> voids in Value.m_subVoidEdges) {
        foreach (Polyline polyline in voids) {
          drawInstructions.Add(new DrawPolyline() { Polyline = polyline, Color = secondaryColor, Thickness = secondaryThickness });
        }
      }

      foreach (Circle crv in Value.m_rebarEdges) {
        drawInstructions.Add(new DrawCircle() { Circle = crv, Color = secondaryColor, Thickness = secondaryThickness });
      }

      foreach (Curve crv in Value.m_linkEdges) {
        drawInstructions.Add(new DrawCurve() { Curve = crv, Color = secondaryColor, Thickness = secondaryThickness });
      }

      return drawInstructions;
    }

    private static bool IsNotSelected(GH_PreviewWireArgs args) {
      Color defaultCol = Grasshopper.Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      return args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B;
    }

    public AdSecSectionGoo DuplicateAdSecSection() {
      if (Value == null) {
        return null;
      } else {
        return new AdSecSectionGoo(Value.Duplicate());
      }
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return DuplicateAdSecSection();
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      if (Value == null) {
        return BoundingBox.Empty;
      }

      if (Value.SolidBrep == null) {
        return BoundingBox.Empty;
      }

      return Value.SolidBrep.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      //return new AdSecSectionGoo(Value.Morph(xmorph));
      return null;
    }

    public override string ToString() {
      if (Value == null) {
        return "Null AdSec Section";
      } else {
        return $"AdSec {TypeName} {{{Value}}}";
      }
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      //return new AdSecSectionGoo(Value.Transform(xform));
      return null;
    }

    public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids) { Bake(doc, obj_ids); }

    private void Bake(RhinoDoc doc, List<Guid> obj_ids, ObjectAttributes attributes = null) {
      foreach (var drawInstruction in _drawInstructions) {
        if (drawInstruction is DrawPolyline drawPolyline) {
          var objectAttributes = GetAttribute(attributes, drawPolyline);
          obj_ids.Add(doc.Objects.AddPolyline(drawPolyline.Polyline, objectAttributes));
        } else if (drawInstruction is DrawCircle drawCircle) {
          var objectAttributes = GetAttribute(attributes, drawCircle);
          obj_ids.Add(doc.Objects.AddCircle(drawCircle.Circle, objectAttributes));
        } else if (drawInstruction is DrawCurve drawCurve) {
          var objectAttributes = GetAttribute(attributes, drawCurve);
          obj_ids.Add(doc.Objects.AddCurve(drawCurve.Curve, objectAttributes));
        }
      }
    }

    private static ObjectAttributes GetAttribute(ObjectAttributes attributes, DrawInstructions instructions) {
      var objectAttributes = attributes?.Duplicate() ?? new ObjectAttributes();
      objectAttributes.ColorSource = ObjectColorSource.ColorFromObject;
      objectAttributes.ObjectColor = instructions.Color;
      return objectAttributes;
    }

    public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids) { Bake(doc, obj_ids, att); }

    public bool IsBakeCapable => Value != null;
  }
}
