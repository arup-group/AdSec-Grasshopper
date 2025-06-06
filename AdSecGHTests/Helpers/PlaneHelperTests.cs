using AdSecGH.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class PlaneHelperTests {
    [Fact]
    public void ShouldReturnFalseForWorldXY() {
      bool result = PlaneHelper.IsNotParallelToWorldXYZ(Plane.WorldXY);
      Assert.False(result);
    }

    [Fact]
    public void ShouldReturnFalseForWorldYZ() {
      bool result = PlaneHelper.IsNotParallelToWorldXYZ(Plane.WorldYZ);
      Assert.False(result);
    }

    [Fact]
    public void ShouldReturnFalseForWorldZX() {
      bool result = PlaneHelper.IsNotParallelToWorldXYZ(Plane.WorldZX);
      Assert.False(result);
    }

    [Fact]
    public void ShouldReturnFalseForInvalidPlane() {
      var invalidPlane = new Plane(); // default, IsValid == false
      bool result = PlaneHelper.IsNotParallelToWorldXYZ(invalidPlane);
      Assert.False(result);
    }

    [Fact]
    public void ShouldReturnTrueForCustomValidPlane() {
      var plane = new Plane(new Point3d(1, 2, 3), Vector3d.XAxis, Vector3d.ZAxis); // not equal to standard planes
      bool result = PlaneHelper.IsNotParallelToWorldXYZ(plane);
      Assert.True(result);
    }
  }

}
