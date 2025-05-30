﻿using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using OasysGH.Units;

using OasysUnits;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecNeutralAxisGoo : GH_GeometricGoo<NeutralAxis>, IGH_PreviewData {
    public Line AxisLine { get; private set; }
    public BoundingBox ClippingBox => Boundingbox;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Neutral Axis";
    public List<DrawInstructions> DrawInstructions { get; private set; } = new List<DrawInstructions>();
    public AdSecNeutralAxisGoo(NeutralAxis axis) {
      m_value = axis;
      AxisLine = CalculateNeutralAxis();
    }

    private Line CalculateNeutralAxis() {
      var local = m_value.Solution.SectionDesign.LocalPlane.ToGh();
      var profile = new AdSecSolutionGoo(m_value.Solution).ProfileEdge;
      double offset = m_value.Offset.As(DefaultUnits.LengthUnitGeometry);
      double angleRadians = m_value.Angle;

      var direction = new Vector3d(local.XAxis);
      direction.Rotate(angleRadians, local.ZAxis);
      direction.Unitize();

      // Calculate profile width more efficiently
      var boundingBox = profile.ToPolylineCurve().GetBoundingBox(local);
      double width = 1.05 * boundingBox.Min.DistanceTo(boundingBox.Max);
      // Calculate start point and line in one stepZ
      var start = local.Origin - (direction * (width / 2));
      var line = new Line(start, direction, width);

      var offsetVector = new Vector3d(direction);
      offsetVector.Rotate(Math.PI / 2, local.ZAxis);
      offsetVector.Unitize();

      line.Transform(Rhino.Geometry.Transform.Translation(offsetVector.X * offset, offsetVector.Y * offset,
        offsetVector.Z * offset));
      return line;
    }

    public override BoundingBox Boundingbox => AxisLine.BoundingBox;

    public override string ToString() {
      var startPoint = AxisLine.PointAt(0);
      var endPoint = AxisLine.PointAt(1);
      var length = new Length(Point3d.Subtract(startPoint, endPoint).Length, DefaultUnits.LengthUnitGeometry);
      return $"Line(Length = {length})";
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecNeutralAxisGoo(m_value);
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      var newLine = Transform(xform);
      return newLine.Boundingbox;
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      var duplicateObject = new AdSecNeutralAxisGoo(m_value);
      var axisLine = duplicateObject.AxisLine;
      axisLine.Transform(xform);
      duplicateObject.AxisLine = axisLine;
      return duplicateObject;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      throw new NotImplementedException();
    }

    private static bool IsNotSelected(GH_PreviewWireArgs args) {
      var defaultColor = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      return args.Color.IsRgbEqualTo(defaultColor);
    }

    private List<DrawInstructions> UpdateDrawInstructions(bool isNotSelected) {
      var drawInstructions = new List<DrawInstructions>();
      var primaryColor = isNotSelected ? Colour.OasysBlue : Colour.OasysYellow;
      if (m_value.IsFailureNeutralAxis) {
        drawInstructions.Add(new DrawDottedLine() { Curve = AxisLine, Color = primaryColor });
      } else {
        drawInstructions.Add(new DrawSolidLine() { Curve = AxisLine, Color = primaryColor });
      }
      return drawInstructions;
    }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      DrawInstructions.Clear();
      DrawInstructions.AddRange(UpdateDrawInstructions(IsNotSelected(args)));
      foreach (var instruction in DrawInstructions) {
        DrawingHelper.Draw(args.Pipeline, instruction);
      }
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
      // mandatory method
    }
  }
}
