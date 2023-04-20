using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec;
using OasysGH;
using OasysGH.Parameters;
using Rhino.Geometry;
using System;
using System.Drawing;

namespace AdSecGH.Parameters {
  public class AdSecCrackGoo : GH_OasysGeometricGoo<ICrack>, IGH_PreviewData {
    public static string Description => "AdSec Crack Parameter";
    public static string Name => "Crack";
    public static string NickName => "Cr";
    public override BoundingBox Boundingbox {
      get {
        if (Value == null)
          return BoundingBox.Empty;
        if (m_line == null)
          return BoundingBox.Empty;
        LineCurve crv = new LineCurve(m_line);
        return crv.GetBoundingBox(false);
      }
    }
    public override BoundingBox ClippingBox {
      get { return Boundingbox; }
    }
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    private Line m_line;
    private Plane m_plane;
    private Point3d m_point = Point3d.Unset;

    public AdSecCrackGoo(ICrack item) : base(item) {
    }

    public AdSecCrackGoo(ICrack crack, Plane local) : base(crack) {
      m_value = crack;
      m_plane = local;

      // create point from crack position in global axis
      Point3d point = new Point3d(
          crack.Position.Y.Value,
          crack.Position.Z.Value,
          0);

      // remap to local coordinate system
      Transform mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, local);
      point.Transform(mapFromLocal);
      m_point = point;

      // move starting point of line by half the width
      Vector3d halfCrack = new Vector3d(local.ZAxis);
      halfCrack.Unitize();
      halfCrack = new Vector3d(
          halfCrack.X * crack.Width.Value / 2,
          halfCrack.Y * crack.Width.Value / 2,
          halfCrack.Z * crack.Width.Value / 2);

      Transform move = Rhino.Geometry.Transform.Translation(halfCrack);
      Point3d crackStart = new Point3d(m_point);
      crackStart.Transform(move);

      // create line in opposite direction from move point
      Vector3d crackWidth = new Vector3d(halfCrack);
      crackWidth.Unitize();
      crackWidth = new Vector3d(
          crackWidth.X * crack.Width.Value * -1,
          crackWidth.Y * crack.Width.Value * -1,
          crackWidth.Z * crack.Width.Value * -1);

      m_line = new Line(crackStart, crackWidth);
    }

    public override bool CastFrom(object source) {
      if (source == null)
        return false;
      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecCrackGoo))) {
        target = (Q)(object)new AdSecCrackGoo(Value, m_plane);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Point3d))) {
        target = (Q)(object)m_point;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Point))) {
        target = (Q)(object)new GH_Point(m_point);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Vector3d))) {
        target = (Q)(object)new Vector3d(Value.Width.Value, m_point.Y, m_point.Z);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Vector))) {
        target = (Q)(object)new GH_Vector(new Vector3d(Value.Width.Value, m_point.Y, m_point.Z));
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Line))) {
        target = (Q)(object)m_line;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Line))) {
        target = (Q)(object)new GH_Line(m_line);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_UnitNumber))) {
        target = (Q)(object)new GH_UnitNumber(Value.Width);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Number))) {
        target = (Q)(object)new GH_Number(Value.Width.Value);
        return true;
      }

      target = default(Q);
      return false;
    }

    public override void DrawViewportMeshes(GH_PreviewMeshArgs args) {
    }

    public override void DrawViewportWires(GH_PreviewWireArgs args) {
      if (m_point.IsValid) {
        Color defaultCol = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
        if (args.Color.R == defaultCol.R && args.Color.G == defaultCol.G && args.Color.B == defaultCol.B) // not selected
          args.Pipeline.DrawLine(m_line, UI.Colour.OasysBlue, 5);
        else
          args.Pipeline.DrawLine(m_line, UI.Colour.OasysYellow, 7);
      }
    }

    public override IGH_GeometricGoo Duplicate() => new AdSecCrackGoo(Value);

    public override IGH_GeometricGoo DuplicateGeometry() {
      AdSecCrackGoo dup = new AdSecCrackGoo(Value, m_plane);
      return dup;
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      if (Value == null)
        return BoundingBox.Empty;
      if (m_point == null)
        return BoundingBox.Empty;
      LineCurve crv = new LineCurve(m_line);
      return crv.GetBoundingBox(xform);
    }

    public override GeometryBase GetGeometry() {
      return null;
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      return "AdSec " + TypeName + " {"
          + "Y:" + Math.Round(Value.Position.Y.Value, 4) + Value.Position.Y.Unit + ", "
          + "Z:" + Math.Round(Value.Position.Z.Value, 4) + Value.Position.Z.Unit + ", "
          + "Width:" + Math.Round(Value.Width.Value, 4) + Value.Width.Unit + "}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }
  }
}
