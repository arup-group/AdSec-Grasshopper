using System;
using System.Drawing;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.Collections;
using Oasys.Profiles;

using OasysGH.Units;

using OasysUnits;

using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecPointGoo : GH_GeometricGoo<Point3d>, IGH_PreviewData {
    public IPoint AdSecPoint { get; private set; }
    public override BoundingBox Boundingbox => PointHelper.GetPointBoundingBox(Value, 0.5d, true);

    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Vertex";

    public AdSecPointGoo(Point3d point) : base(point) {
      m_value = point;
      AdSecPoint = IPoint.Create(new Length(m_value.Y, DefaultUnits.LengthUnitGeometry),
        new Length(m_value.Z, DefaultUnits.LengthUnitGeometry));
    }

    public AdSecPointGoo(AdSecPointGoo adsecPoint) {
      if (adsecPoint == null) {
        return;
      }

      AdSecPoint = adsecPoint.AdSecPoint;
      m_value = new Point3d(Value);
    }

    public AdSecPointGoo(IPoint adsecPoint) {
      if (adsecPoint == null) {
        return;
      }

      AdSecPoint = adsecPoint;
      m_value = new Point3d(0, AdSecPoint.Y.As(DefaultUnits.LengthUnitGeometry),
        AdSecPoint.Z.As(DefaultUnits.LengthUnitGeometry));
    }

    public AdSecPointGoo(Length y, Length z) {
      AdSecPoint = IPoint.Create(y, z);
      m_value = new Point3d(0, AdSecPoint.Y.As(DefaultUnits.LengthUnitGeometry),
        AdSecPoint.Z.As(DefaultUnits.LengthUnitGeometry));
    }

    public BoundingBox ClippingBox => Boundingbox;

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (args.Color == Instances.Settings.GetValue("DefaultPreviewColour", Color.White)) {
        args.Pipeline.DrawPoint(Value, PointStyle.RoundControlPoint, 3, Colour.OasysBlue);
      } else {
        args.Pipeline.DrawPoint(Value, PointStyle.RoundControlPoint, 5, Colour.OasysYellow);
      }
    }

    public static IPoint CreateFromPoint3d(Point3d point, Plane plane) {
      var mapToLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldYZ, plane);
      var transformation = new Point3d(point);
      transformation.Transform(mapToLocal);
      return IPoint.Create(new Length(transformation.Y, DefaultUnits.LengthUnitGeometry),
        new Length(transformation.Z, DefaultUnits.LengthUnitGeometry));
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

      if (source is Point3d d) {
        var adSecPointGoo = new AdSecPointGoo(d);
        m_value = adSecPointGoo.Value;
        AdSecPoint = adSecPointGoo.AdSecPoint;
        return true;
      }

      if (source is IPoint point) {
        var adSecPointGoo = new AdSecPointGoo(point);
        m_value = adSecPointGoo.Value;
        AdSecPoint = adSecPointGoo.AdSecPoint;
        return true;
      }

      if (source is GH_Point ptGoo) {
        var adSecPointGoo = new AdSecPointGoo(ptGoo.Value);
        m_value = adSecPointGoo.Value;
        AdSecPoint = adSecPointGoo.AdSecPoint;
        return true;
      }

      var point3d = new Point3d();
      if (GH_Convert.ToPoint3d(source, ref point3d, GH_Conversion.Both)) {
        var adSecPointGoo = new AdSecPointGoo(point3d);
        m_value = adSecPointGoo.Value;
        AdSecPoint = adSecPointGoo.AdSecPoint;
        return true;
      }

      return false;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecPointGoo))) {
        target = (Q)(object)new AdSecPointGoo(Value);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(Point3d))) {
        target = (Q)(object)Value;
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(GH_Point))) {
        target = (Q)(object)new GH_Point(Value);
        return true;
      }

      if (typeof(Q).IsAssignableFrom(typeof(IPoint))) {
        target = (Q)(object)IPoint.Create(new Length(Value.X, DefaultUnits.LengthUnitGeometry),
          new Length(Value.Y, DefaultUnits.LengthUnitGeometry));
        return true;
      }

      target = default;
      return false;
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecPointGoo(new Point3d(Value));
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      return PointHelper.GetPointBoundingBox(Value, xform, 0.001, false);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      IQuantity quantity = new Length(0, DefaultUnits.LengthUnitGeometry);
      string unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
      return
        $"AdSec {TypeName} {{{Math.Round(AdSecPoint.Y.As(DefaultUnits.LengthUnitGeometry), 4)}{unitAbbreviation}, {Math.Round(AdSecPoint.Z.As(DefaultUnits.LengthUnitGeometry), 4)}{unitAbbreviation}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }

    internal static IList<IPoint> PtsFromPolylineCurve(PolylineCurve curve) {
      curve.TryGetPolyline(out var tempCurve);
      Plane.FitPlaneToPoints(tempCurve.ToList(), out var plane);
      var mapToLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, plane);

      var points = IList<IPoint>.Create();
      for (int j = 0; j < curve.PointCount; j++) {
        var point3d = curve.Point(j);
        point3d.Transform(mapToLocal);
        var point = IPoint.Create(new Length(point3d.X, DefaultUnits.LengthUnitGeometry),
          new Length(point3d.Y, DefaultUnits.LengthUnitGeometry));
        points.Add(point);
      }

      return points;
    }
  }
}
