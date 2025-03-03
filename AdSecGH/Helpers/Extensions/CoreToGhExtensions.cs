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

    public static Point3d ToGhPoint(this OasysPoint point) {
      return new Point3d(point.X, point.Y, point.Z);
    }

    public static Vector3d ToGhVector(this OasysPoint point) {
      return new Vector3d(point.X, point.Y, point.Z);
    }

  }
}
