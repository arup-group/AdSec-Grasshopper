using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSectionGoo : GH_GeometricGoo<AdSecSection>, IGH_PreviewData, IGH_BakeAwareObject {
    public override BoundingBox Boundingbox => Value?.SolidBrep?.GetBoundingBox(false) ?? BoundingBox.Empty;
    public BoundingBox ClippingBox => Boundingbox;
    public override bool IsValid => Value?.SolidBrep != null && Value.IsValid && Value.SolidBrep.IsValid;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Section";

    public AdSecSectionGoo() {
      UpdateGeometryRepresentation();
    }

    public AdSecSectionGoo(AdSecSection section) {
      Value = section?.Duplicate();
      UpdateGeometryRepresentation();
    }

    public void UpdateGeometryRepresentation(bool isNotSelected = true) {
      DrawInstructionsList.Clear();
      DrawInstructionsList.AddRange(UpdateDrawInstructions(isNotSelected));
    }

    public override bool CastFrom(object source) {
      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecSectionGoo))) {
        target = Value == null ? default : (Q)(object)Value.Duplicate();
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(AdSecSection))) {
        target = Value == null ? default : (Q)(object)new AdSecSection(Value.Section, Value.DesignCode, Value._codeName,
          Value._materialName, Value.LocalPlane);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(AdSecProfileGoo))) {
        target = Value == null ? default : (Q)(object)new AdSecProfileGoo(Value.Section.Profile, Value.LocalPlane);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Brep))) {
        target = Value == null ? default : (Q)(object)Value.SolidBrep.DuplicateBrep();
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Brep))) {
        target = Value == null ? default : (Q)(object)Value.SolidBrep.DuplicateBrep();
        return true;
      }

      target = default;
      return false;
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
      if (!IsValid) {
        return;
      }
      args.Pipeline.DrawBrepShaded(Value.SolidBrep, Value.ProfileData.ProfileColour);

      var reinforcementDataRebars = Value.ReinforcementData.Rebars;
      for (int i = 0; i < reinforcementDataRebars.Count; i++) {
        args.Pipeline.DrawBrepShaded(reinforcementDataRebars[i], Value.ReinforcementData.RebarColours[i]);
      }
    }

    public List<DrawInstructions> DrawInstructionsList { get; private set; } = new List<DrawInstructions>();

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (!IsValid) {
        return;
      }

      DrawInstructionsList.Clear();
      DrawInstructionsList.AddRange(UpdateDrawInstructions(IsNotSelected(args)));

      foreach (var instruction in DrawInstructionsList) {
        DrawingHelper.Draw(args.Pipeline, instruction);
      }

      DrawingHelper.DrawLocalAxis(args, Value.previewZaxis, Value.previewXaxis, Value.previewYaxis);
    }

    private List<DrawInstructions> UpdateDrawInstructions(bool isNotSelected) {
      if (!IsValid) {
        return new List<DrawInstructions>();
      }

      var drawInstructions = new List<DrawInstructions>();

      var primaryColor = isNotSelected ? Colour.OasysBlue : Colour.OasysYellow;
      var secondaryColor = isNotSelected ? Color.Black : Colour.UILightGrey;
      var primaryThickness = isNotSelected ? 2 : 3;
      var secondaryThickness = isNotSelected ? 1 : 2;

      drawInstructions.Add(new DrawPolyline() { Polyline = Value.ProfileData.ProfileEdge, Color = primaryColor, Thickness = primaryThickness, });

      drawInstructions.AddRange(Value.ProfileData.ProfileVoidEdges.Select(polyline => new DrawPolyline() { Polyline = polyline, Color = secondaryColor, Thickness = secondaryThickness, }));

      drawInstructions.AddRange(Value.SubProfilesData.SubEdges.Select(polyline => new DrawPolyline() { Polyline = polyline, Color = primaryColor, Thickness = secondaryThickness, }));

      drawInstructions.AddRange(Value.SubProfilesData.SubVoidEdges.SelectMany(voids
        => voids.Select(polyline => new DrawPolyline() { Polyline = polyline, Color = secondaryColor, Thickness = secondaryThickness, })));

      drawInstructions.AddRange(Value.ReinforcementData.RebarEdges.Select(circle => new DrawCircle() { Circle = circle, Color = secondaryColor, Thickness = secondaryThickness, }));

      drawInstructions.AddRange(Value.ReinforcementData.LinkEdges.Select(curve => new DrawCurve() { Curve = curve, Color = secondaryColor, Thickness = secondaryThickness, }));

      return drawInstructions;
    }

    private static bool IsNotSelected(GH_PreviewWireArgs args) {
      var defaultColor = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      return args.Color.IsRgbEqualTo(defaultColor);
    }

    public AdSecSectionGoo DuplicateAdSecSection() {
      return !IsValid ? null : new AdSecSectionGoo(Value.Duplicate());
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return DuplicateAdSecSection();
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return !IsValid ? BoundingBox.Empty : Value.SolidBrep.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return !IsValid ? "Invalid AdSec Section" : $"AdSec {TypeName} {{{Value}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    private void Bake(RhinoDoc doc, List<Guid> obj_ids, ObjectAttributes attributes = null) {
      foreach (var drawInstruction in DrawInstructionsList) {
        switch (drawInstruction) {
          case DrawPolyline drawPolyline: {
              var objectAttributes = GetAttribute(attributes, drawPolyline);
              obj_ids.Add(doc.Objects.AddPolyline(drawPolyline.Polyline, objectAttributes));
              break;
            }

          case DrawCircle drawCircle: {
              var objectAttributes = GetAttribute(attributes, drawCircle);
              obj_ids.Add(doc.Objects.AddCircle(drawCircle.Circle, objectAttributes));
              break;
            }

          case DrawCurve drawCurve: {
              var objectAttributes = GetAttribute(attributes, drawCurve);
              obj_ids.Add(doc.Objects.AddCurve(drawCurve.Curve, objectAttributes));
              break;
            }
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
