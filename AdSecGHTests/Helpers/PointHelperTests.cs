using AdSecGH.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class PointHelperTests {
    [Fact]
    public void GetPointBoundingBox_ShouldReturnCorrectBoundingBox_ForOriginPoint() {
      var point = new Point3d(0, 0, 0);
      var boundingBox = PointHelper.GetPointBoundingBox(point);

      Assert.Equal(0, boundingBox.Min.X, 5);
      Assert.Equal(0, boundingBox.Min.Y, 5);
      Assert.Equal(-0.25, boundingBox.Min.Z, 5);
      Assert.Equal(0, boundingBox.Max.X, 5);
      Assert.Equal(0, boundingBox.Max.Y, 5);
      Assert.Equal(0.25, boundingBox.Max.Z, 5);
    }

    [Fact]
    public void GetPointBoundingBox_ShouldReturnCorrectBoundingBox_ForPositiveZ() {
      var point = new Point3d(1, 2, 10);
      var boundingBox = PointHelper.GetPointBoundingBox(point);

      Assert.Equal(1, boundingBox.Min.X, 5);
      Assert.Equal(2, boundingBox.Min.Y, 5);
      Assert.Equal(9.75, boundingBox.Min.Z, 5);

      Assert.Equal(1, boundingBox.Max.X, 5);
      Assert.Equal(2, boundingBox.Max.Y, 5);
      Assert.Equal(10.25, boundingBox.Max.Z, 5);
    }

    [Fact]
    public void GetPointBoundingBox_ShouldReturnCorrectBoundingBox_ForNegativeZ() {
      var point = new Point3d(-5, 3, -4);
      var boundingBox = PointHelper.GetPointBoundingBox(point);

      Assert.Equal(-5, boundingBox.Min.X, 5);
      Assert.Equal(3, boundingBox.Min.Y, 5);
      Assert.Equal(-4.25, boundingBox.Min.Z, 5);

      Assert.Equal(-5, boundingBox.Max.X, 5);
      Assert.Equal(3, boundingBox.Max.Y, 5);
      Assert.Equal(-3.75, boundingBox.Max.Z, 5);
    }

    [Fact]
    public void GetPointBoundingBox_ReturnsCorrectBoundingBox_WhenTransformIsIdentity() {
      var point = new Point3d(1, 2, 3);
      var xform = Transform.Identity;

      var bbox = PointHelper.GetPointBoundingBox(point, xform);

      Assert.True(bbox.IsValid);
      Assert.Equal(1, bbox.Min.X, 5);
      Assert.Equal(2, bbox.Min.Y, 5);
      Assert.Equal(2.75, bbox.Min.Z, 5);

      Assert.Equal(1, bbox.Max.X, 5);
      Assert.Equal(2, bbox.Max.Y, 5);
      Assert.Equal(3.25, bbox.Max.Z, 5);
    }

    [Fact]
    public void GetPointBoundingBox_TransformsBoundingBox_WhenTransformIsTranslation() {
      var point = new Point3d(0, 0, 0);
      var xform = Transform.Translation(10, 20, 30);

      var bbox = PointHelper.GetPointBoundingBox(point, xform);

      Assert.True(bbox.IsValid);
      Assert.Equal(10, bbox.Min.X, 5);
      Assert.Equal(20, bbox.Min.Y, 5);
      Assert.Equal(29.75, bbox.Min.Z, 5);

      Assert.Equal(10, bbox.Max.X, 5);
      Assert.Equal(20, bbox.Max.Y, 5);
      Assert.Equal(30.25, bbox.Max.Z, 5);
    }

    [Fact]
    public void GetPointBoundingBox_ReturnsCorrectBoundingBox_WhenPointHasNegativeCoordinates() {
      var point = new Point3d(-5, -5, -5);
      var xform = Transform.Identity;

      var bbox = PointHelper.GetPointBoundingBox(point, xform);

      Assert.True(bbox.IsValid);
      Assert.Equal(-5, bbox.Min.X, 5);
      Assert.Equal(-5, bbox.Min.Y, 5);
      Assert.Equal(-5.25, bbox.Min.Z, 5);

      Assert.Equal(-5, bbox.Max.X, 5);
      Assert.Equal(-5, bbox.Max.Y, 5);
      Assert.Equal(-4.75, bbox.Max.Z, 5);
    }

    [Fact]
    public void GetPointBoundingBox_ReturnsInvalidBoundingBox_WhenTransformScalesToZero() {
      var point = new Point3d(1, 2, 3);
      var xform = Transform.Scale(new Point3d(0, 0, 0), 0);

      var bbox = PointHelper.GetPointBoundingBox(point, xform);

      Assert.True(bbox.IsValid);
      Assert.Equal(bbox.Min, bbox.Max);
    }

    [Fact]
    public void GetPointBoundingBox_ReturnsInvalidBoundingBox_WhenPointIsNaN() {
      var point = new Point3d(double.NaN, 0, 0);
      var xform = Transform.Identity;

      var bbox = PointHelper.GetPointBoundingBox(point, xform);

      Assert.False(bbox.IsValid);
    }
  }

}
