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
      return new OasysPlane() {
        Origin = plane.Origin.ToOasysPoint(),
        XAxis = plane.XAxis.ToOasysPoint(),
        YAxis = plane.YAxis.ToOasysPoint()
      };
    }

    public static Point3d ToGhPoint(this OasysPoint point) {
      return new Point3d(point.X, point.Y, point.Z);
    }

    public static OasysPoint ToOasysPoint(this Point3d point) {
      return new OasysPoint() { X = point.X, Y = point.Y, Z = point.Z };
    }

    public static Vector3d ToGhVector(this OasysPoint point) {
      return new Vector3d(point.X, point.Y, point.Z);
    }

    public static OasysPoint ToOasysPoint(this Vector3d vector) {
      return new OasysPoint() { X = vector.X, Y = vector.Y, Z = vector.Z };
    }

  }
}
