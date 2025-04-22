using System;
using System.Drawing;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysGH.Units;

using OasysUnits;

using Rhino.Display;
using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecLoadGoo : GH_GeometricGoo<ILoad>, IGH_PreviewData {
    public static string Description => "AdSec Load";
    public static string Name => "Load";
    public static string NickName => "Ld";
    public override BoundingBox Boundingbox {
      get {
        if (Value == null) {
          return BoundingBox.Empty;
        }

        if (_point == null) {
          return BoundingBox.Empty;
        }

        return PointHelper.GetPointBoundingBox(_point);
      }
    }
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Load";
    private Point3d _point = Point3d.Unset;

    public AdSecLoadGoo(ILoad load) : base(load) { }

    public AdSecLoadGoo(ILoad load, Plane local) {
      m_value = load;
      var point = new Point3d(load.ZZ.As(DefaultUnits.MomentUnit), load.YY.As(DefaultUnits.MomentUnit),
        load.X.As(DefaultUnits.ForceUnit));
      var mapFromLocal = Rhino.Geometry.Transform.PlaneToPlane(Plane.WorldXY, local);
      point.Transform(mapFromLocal);
      _point = point;
    }

    public BoundingBox ClippingBox => Boundingbox;

    public void DrawViewportMeshes(GH_PreviewMeshArgs args) { }

    public void DrawViewportWires(GH_PreviewWireArgs args) {
      if (_point.IsValid) {
        var defaultColor = Instances.Settings.GetValue("DefaultPreviewColour", Color.White);
        if (args.Color.R == defaultColor.R && args.Color.G == defaultColor.G && args.Color.B == defaultColor.B) {
          args.Pipeline.DrawPoint(_point, PointStyle.X, 7, Colour.ArupRed);
        } else {
          args.Pipeline.DrawPoint(_point, PointStyle.X, 8, Colour.UILightGrey);
        }
      }
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

      Point3d point;
      switch (source) {
        case Point3d pt:
          point = pt;
          break;
        case GH_Point ghPt:
          point = ghPt.Value;
          break;
        default:
          point = new Point3d();
          if (!GH_Convert.ToPoint3d(source, ref point, GH_Conversion.Both)) {
            return false;
          }

          break;
      }

      return SetLoadFromPoint(point);
    }

    private bool SetLoadFromPoint(Point3d point) {
      var load = ILoad.Create(new Force(point.X, DefaultUnits.ForceUnit), new Moment(point.Y, DefaultUnits.MomentUnit),
        new Moment(point.Z, DefaultUnits.MomentUnit));

      Value = new AdSecLoadGoo(load).Value;
      return true;
    }

    public override bool CastTo<Q>(out Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecLoadGoo))) {
        target = (Q)(object)new AdSecLoadGoo(Value);
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

      if (typeof(Q).IsAssignableFrom(typeof(ILoad))) {
        target = (Q)ILoad.Create(Value.X, Value.YY, Value.ZZ);
        return true;
      }

      target = default;
      return false;
    }

    public override IGH_Goo Duplicate() {
      return new AdSecLoadGoo(Value);
    }

    public override IGH_GeometricGoo DuplicateGeometry() {
      return new AdSecLoadGoo(Value) {
        _point = new Point3d(_point),
      };
    }

    public override BoundingBox GetBoundingBox(Transform xform) {
      if (Value == null) {
        return BoundingBox.Empty;
      }

      if (_point == null) {
        return BoundingBox.Empty;
      }

      var point1 = new Point3d(_point);
      point1.Z += 0.25;
      var point2 = new Point3d(_point);
      point2.Z += -0.25;
      var line = new Line(point1, point2);
      var lineCurve = new LineCurve(line);
      return lineCurve.GetBoundingBox(xform);
    }

    public override IGH_GeometricGoo Morph(SpaceMorph xmorph) {
      return null;
    }

    public override string ToString() {
      IQuantity quantityMoment = new Moment(0, DefaultUnits.MomentUnit);
      string unitMomentAbbreviation = string.Concat(quantityMoment.ToString().Where(char.IsLetter));
      IQuantity quantityForce = new Force(0, DefaultUnits.ForceUnit);
      string unitforceAbbreviation = string.Concat(quantityForce.ToString().Where(char.IsLetter));
      return
        $"AdSec {TypeName} {{{Math.Round(Value.X.As(DefaultUnits.ForceUnit), 4)}{unitforceAbbreviation}, {Math.Round(Value.YY.As(DefaultUnits.MomentUnit), 4)}{unitMomentAbbreviation}, {Math.Round(Value.ZZ.As(DefaultUnits.MomentUnit), 4)}{unitMomentAbbreviation}}}";
    }

    public override IGH_GeometricGoo Transform(Transform xform) {
      return null;
    }
  }
}
