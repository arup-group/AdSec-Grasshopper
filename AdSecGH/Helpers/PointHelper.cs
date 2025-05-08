using Rhino.Geometry;

namespace AdSecGH.Helpers {
  public static class PointHelper {
    public static BoundingBox GetPointBoundingBox(Point3d value, double offset, bool symmetric) {
      if (!value.IsValid) {
        return BoundingBox.Unset;
      }

      var curve = GetLineCurve(value, offset, symmetric);
      return curve.GetBoundingBox(false);
    }

    public static BoundingBox GetPointBoundingBox(Point3d value, Transform xform, double offset, bool symmetric) {
      if (!value.IsValid) {
        return BoundingBox.Unset;
      }

      var curve = GetLineCurve(value, offset, symmetric);
      return curve.GetBoundingBox(xform);
    }

    private static LineCurve GetLineCurve(Point3d value, double offset, bool symmetric) {
      Point3d point1,
        point2;
      if (symmetric) {
        point1 = new Point3d(value) { Z = value.Z + (offset / 2), };
        point2 = new Point3d(value) { Z = value.Z - (offset / 2), };
      } else {
        point1 = value;
        point2 = new Point3d(value) { Z = value.Z + offset, };
      }

      var line = new Line(point1, point2);
      return new LineCurve(line);
    }
  }
}
