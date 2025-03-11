using AdSecCore.Functions;

using Rhino.Geometry;

namespace AdSecGH.Helpers {
  public static class CoreToGhExtensions {
    public static Plane ToGh(this OasysPlane plane) {
      var origin = plane.Origin.ToGhPoint();
      var x = plane.XAxis.ToGhVector();
      var y = plane.YAxis.ToGhVector();

      return new Plane(origin, x, y);
    }

    public static OasysPlane ToOasys(this Plane plane) {
      var oasysPlane = new OasysPlane();
      oasysPlane.Origin = new OasysPoint {
        X = plane.Origin.X,
        Y = plane.Origin.Y,
        Z = plane.Origin.Z
      };
      oasysPlane.XAxis = new OasysPoint {
        X = plane.XAxis.X,
        Y = plane.XAxis.Y,
        Z = plane.XAxis.Z
      };
      oasysPlane.YAxis = new OasysPoint {
        X = plane.YAxis.X,
        Y = plane.YAxis.Y,
        Z = plane.YAxis.Z
      };
      return oasysPlane;
    }

    public static Point3d ToGhPoint(this OasysPoint point) {
      return new Point3d(point.X, point.Y, point.Z);
    }

    public static Vector3d ToGhVector(this OasysPoint point) {
      return new Vector3d(point.X, point.Y, point.Z);
    }

  }
}
