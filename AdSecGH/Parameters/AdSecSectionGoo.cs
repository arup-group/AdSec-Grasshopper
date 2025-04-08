using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.UI;

using Grasshopper;
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

        if (Value.SolidBrep == null || !Value.IsValid) {
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

    public AdSecSectionGoo() { UpdateGeometryRepresentation(); }

    public AdSecSectionGoo(AdSecSection section) {
      if (section == null) {
        Value = null;
      } else {
        Value = section.Duplicate();
      }

      UpdateGeometryRepresentation();
    }

    public void UpdateGeometryRepresentation(bool isNotSelected = true) {
      DrawInstructionsList.Clear();
      DrawInstructionsList.AddRange(UpdateDrawInstructions(isNotSelected));
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

      return false;
    }

    public override bool CastTo<Q>(out Q target) {
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
      if (Value.SolidBrep != null) {
        args.Pipeline.DrawBrepShaded(Value.SolidBrep, Value._profileColour);
        for (int i = 0; i < Value._subProfiles.Count; i++) {
          args.Pipeline.DrawBrepShaded(Value._subProfiles[i], Value._subColours[i]);
        }

        for (int i = 0; i < Value._rebars.Count; i++) {
          args.Pipeline.DrawBrepShaded(Value._rebars[i], Value._rebarColours[i]);
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

    public List<DrawInstructions> DrawInstructionsList { get; private set; } = new List<DrawInstructions>();

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (Value == null) {
        return;
      }

      DrawInstructionsList.Clear();
      DrawInstructionsList.AddRange(UpdateDrawInstructions(IsNotSelected(args)));

      foreach (var instruction in DrawInstructionsList) {
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

      var primaryColor = isNotSelected ? Colour.OasysBlue : Colour.OasysYellow;
      var secondaryColor = isNotSelected ? Color.Black : Colour.UILightGrey;
      var primaryThickness = isNotSelected ? 2 : 3;
      var secondaryThickness = isNotSelected ? 1 : 2;

      drawInstructions.Add(new DrawPolyline() { Polyline = Value._profileEdge, Color = primaryColor, Thickness = primaryThickness, });

      foreach (var polyline in Value._profileVoidEdges) {
        drawInstructions.Add(new DrawPolyline() { Polyline = polyline, Color = secondaryColor, Thickness = secondaryThickness, });
      }

      foreach (var polyline in Value._subEdges) {
        drawInstructions.Add(new DrawPolyline() { Polyline = polyline, Color = primaryColor, Thickness = secondaryThickness, });
      }

      foreach (var voids in Value._subVoidEdges) {
        foreach (var polyline in voids) {
          drawInstructions.Add(new DrawPolyline() { Polyline = polyline, Color = secondaryColor, Thickness = secondaryThickness, });
        }
      }

      foreach (var circle in Value._rebarEdges) {
        drawInstructions.Add(new DrawCircle() { Circle = circle, Color = secondaryColor, Thickness = secondaryThickness, });
      }

      foreach (var curve in Value._linkEdges) {
        drawInstructions.Add(new DrawCurve() { Curve = curve, Color = secondaryColor, Thickness = secondaryThickness, });
      }

      return drawInstructions;
    }

    private static bool IsNotSelected(GH_PreviewWireArgs args) {
      var defaultColor = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      return AreEqual(defaultColor, args.Color);
    }

    private static bool AreEqual(Color defaultCol, Color color) {
      return color.R == defaultCol.R && color.G == defaultCol.G && color.B == defaultCol.B;
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
      return Value == null || Value.SolidBrep == null ? BoundingBox.Empty : Value.SolidBrep.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return Value == null ? "Null AdSec Section" : $"AdSec {TypeName} {{{Value}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    private void Bake(RhinoDoc doc, List<Guid> obj_ids, ObjectAttributes attributes = null) {
      foreach (var drawInstruction in DrawInstructionsList) {
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

    public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids) { Bake(doc, obj_ids); }
    public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids) { Bake(doc, obj_ids, att); }

    public bool IsBakeCapable => Value != null;
  }
}
