using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using OasysGH.Units;

using OasysUnits;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecNeutralAxisGoo : GH_GeometricGoo<NeutralAxis>, IGH_PreviewData {
    internal Line AxisLine { get; private set; }
    public BoundingBox ClippingBox => Boundingbox;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Neutral Axis";

    public AdSecNeutralAxisGoo(NeutralAxis axis) {
      m_value = axis;
      AxisLine = CalculateNeutralAxis();
    }

    private Line CalculateNeutralAxis() {
      // Cache frequently used values
      var local = m_value.Solution.SectionDesign.LocalPlane.ToGh();
      var profile = new AdSecSolutionGoo(m_value.Solution).ProfileEdge;
      var offset = m_value.Offset.As(DefaultUnits.LengthUnitGeometry);
      var angleRadians = m_value.Angle;

      // Calculate direction vector once
      var direction = new Vector3d(local.XAxis);
      direction.Rotate(angleRadians, local.ZAxis);
      direction.Unitize();

      // Calculate profile width more efficiently
      var bbox = profile.ToPolylineCurve().GetBoundingBox(local);
      var width = 1.05 * bbox.PointAt(0, 0, 0).DistanceTo(bbox.PointAt(1, 0, 0));
      // Calculate start point and line in one step
      var start = local.Origin - direction * (width / 2);
      var line = new Line(start, direction, width);

      // offset vector
      var offsVec = new Vector3d(direction);
      offsVec.Rotate(Math.PI / 2, local.ZAxis);
      offsVec.Unitize();
      // move the line
      line.Transform(Rhino.Geometry.Transform.Translation(offsVec.X * offset, offsVec.Y * offset, offsVec.Z * offset));
      return line;

    }

    public override BoundingBox Boundingbox {
      get {
        return AxisLine.BoundingBox;
      }
    }

    public override string ToString() {
      var startPoint = AxisLine.PointAt(0);
      var endPoint = AxisLine.PointAt(1);
      var length = new Length(Point3d.Subtract(startPoint, endPoint).Length, DefaultUnits.LengthUnitGeometry);
      return $"Line(Length = {length.ToString()})";
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

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      var defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) {
        // not selected
        args.Pipeline.DrawLine(AxisLine, UI.Colour.OasysBlue);
      } else {
        args.Pipeline.DrawLine(AxisLine, UI.Colour.OasysYellow);
      }
    }

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) {
    }
  }
}
