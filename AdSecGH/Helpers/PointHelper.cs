using Rhino.Geometry;

namespace AdSecGH.Helpers {
  public static class PointHelper {
    public static BoundingBox GetPointBoundingBox(Point3d value) {
      var point1 = new Point3d(value);
      point1.Z += 0.25;
      var point2 = new Point3d(value);
      point2.Z += -0.25;
      var line = new Line(point1, point2);
      var curve = new LineCurve(line);
      return curve.GetBoundingBox(false);
    }
  }
}
