using AdSecGH.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
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
  }

}
