using System;
using System.Drawing;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using OasysGH;
using OasysGH.Parameters;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecCrackGoo : GH_OasysGeometricGoo<CrackLoad>, IGH_PreviewData {
    public static string Description => "AdSec Crack Parameter";
    public static string Name => "Crack";
    public static string NickName => "Cr";
    public override BoundingBox Boundingbox
      => IsValidCrack() ? new LineCurve(_line).GetBoundingBox(false) : BoundingBox.Empty;
    public override BoundingBox ClippingBox => Boundingbox;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    private readonly Line _line;
    private Point3d _point;

    public AdSecCrackGoo(CrackLoad crackLoad) : base(crackLoad) {
      var plane = Value.Plane.ToGh();
      // create point from crack position in global axis
      var point3d = new Point3d(m_value.Load.Position.Y.Value, m_value.Load.Position.Z.Value, 0);

      // remap to local coordinate system
      var mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, plane);
      point3d.Transform(mapFromLocal);
      _point = point3d;

      // move starting point of line by half the width
      var halfCrack = new Vector3d(plane.ZAxis);
      halfCrack.Unitize();
      halfCrack = new Vector3d(
          halfCrack.X * m_value.Load.Width.Value / 2,
          halfCrack.Y * m_value.Load.Width.Value / 2,
          halfCrack.Z * m_value.Load.Width.Value / 2);

      var move = Rhino.Geometry.Transform.Translation(halfCrack);
      var crackStart = new Point3d(_point);
      crackStart.Transform(move);

      // create line in opposite direction from move point
      var crackWidth = new Vector3d(halfCrack);
      crackWidth.Unitize();
      crackWidth = new Vector3d(
          crackWidth.X * m_value.Load.Width.Value * -1,
          crackWidth.Y * m_value.Load.Width.Value * -1,
          crackWidth.Z * m_value.Load.Width.Value * -1);

      _line = new Line(crackStart, crackWidth);
    }
    public override bool CastFrom(object source) {
      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecCrackGoo))) {
        target = (Q)(object)new AdSecCrackGoo(Value);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Point3d))) {
        target = (Q)(object)_point;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Point))) {
        target = (Q)(object)new GH_Point(_point);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Vector3d))) {
        target = (Q)(object)new Vector3d(Value.Load.Width.Value, _point.Y, _point.Z);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Vector))) {
        target = (Q)(object)new GH_Vector(new Vector3d(Value.Load.Width.Value, _point.Y, _point.Z));
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Line))) {
        target = (Q)(object)_line;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Line))) {
        target = (Q)(object)new GH_Line(_line);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_UnitNumber))) {
        target = (Q)(object)new GH_UnitNumber(Value.Load.Width);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Number))) {
        target = (Q)(object)new GH_Number(Value.Load.Width.Value);
        return true;
      }

      target = default;
      return false;
    }

    public override void DrawViewportMeshes(GH_PreviewMeshArgs args) {
    }

    public override void DrawViewportWires(GH_PreviewWireArgs args) {
      if (!IsValidCrack()) {
        return;
      }

      var defaultColor = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
      if (args.Color.IsRgbEqualTo(defaultColor)) {
        // not selected
        args.Pipeline.DrawLine(_line, Colour.OasysBlue, 5);
      } else {
        args.Pipeline.DrawLine(_line, Colour.OasysYellow, 7);
      }
    }

    public override IGH_GeometricGoo Duplicate() {
      return DuplicateGeometry();
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return IsValidCrack() ? new AdSecCrackGoo(Value) : null;
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      if (!IsValidCrack()) {
        return BoundingBox.Empty;
      }

      var crv = new LineCurve(_line);
      return crv.GetBoundingBox(xform);
    }

    public override GeometryBase GetGeometry() {
      return null;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return
        $"AdSec {TypeName} {{Y:{Math.Round(Value.Load.Position.Y.Value, 4)}{Value.Load.Position.Y.Unit}, Z:{Math.Round(Value.Load.Position.Z.Value, 4)}{Value.Load.Position.Z.Unit}, Width:{Math.Round(Value.Load.Width.Value, 4)}{Value.Load.Width.Unit}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    public bool IsValidCrack() {
      return Value != null && _line.IsValid;
    }
  }
}
