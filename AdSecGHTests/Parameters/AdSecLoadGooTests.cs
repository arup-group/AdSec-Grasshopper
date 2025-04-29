using System.Diagnostics.CodeAnalysis;

using AdSecGH.Parameters;

using Oasys.AdSec;

using OasysUnits;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  [SuppressMessage("Major Bug", "S4143:Collection elements should not be replaced unconditionally",
    Justification = "<Pending>")]
  public class AdSecLoadGooTests {
    private AdSecLoadGoo _testGoo;

    public AdSecLoadGooTests() {
      _testGoo = new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment()), Plane.WorldXY);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsNotNullAndPointIsValid() {
      Assert.True(_testGoo.IsValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIsNull() {
      _testGoo = new AdSecLoadGoo(null);

      Assert.False(_testGoo.IsValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenPointIsInvalid() {
      var localPlane = Plane.Unset;
      _testGoo = new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment()), localPlane);

      Assert.False(_testGoo.IsValid);
    }

    [Fact]
    public void Duplicate_ReturnsNewInstance_WhenIsValidIsTrue() {
      var result = _testGoo.Duplicate();

      Assert.NotNull(result);
      Assert.IsType<AdSecLoadGoo>(result);
      Assert.Equal(_testGoo.Value, ((AdSecLoadGoo)result).Value);
    }

    [Fact]
    public void Duplicate_ReturnsNull_WhenIsValidIsFalse() {
      _testGoo = new AdSecLoadGoo(null);
      var result = _testGoo.Duplicate();

      Assert.Null(result);
    }

    [Fact]
    public void DuplicateGeometry_ReturnsNewInstance_WhenIsValidIsTrue() {
      var result = _testGoo.DuplicateGeometry();

      Assert.NotNull(result);
      Assert.IsType<AdSecLoadGoo>(result);
      Assert.Equal(_testGoo.Value, ((AdSecLoadGoo)result).Value);
    }

    [Fact]
    public void DuplicateGeometry_ReturnsNull_WhenIsValidIsFalse() {
      _testGoo = new AdSecLoadGoo(null);
      var result = _testGoo.DuplicateGeometry();

      Assert.Null(result);
    }

    [Fact]
    public void GetBoundingBox_ReturnsEmpty_WhenIsValidIsFalse() {
      _testGoo = new AdSecLoadGoo(null);
      var result = _testGoo.GetBoundingBox(Transform.Identity);

      Assert.Equal(BoundingBox.Empty, result);
    }

    [Fact]
    public void GetBoundingBox_ReturnsNonEmpty_WhenIsValidIsTrue() {
      var result = _testGoo.GetBoundingBox(Transform.Translation(1, 0, 0));

      Assert.True(result.IsValid);
    }

    [Fact]
    public void Morph_ReturnsNull_Always() {
      var result = _testGoo.Morph(new TestSpaceMorph());

      Assert.Null(result);
    }

    [Fact]
    public void Transform_ReturnsNull_Always() {
      var result = _testGoo.Transform(Transform.Identity);

      Assert.Null(result);
    }

    private class TestSpaceMorph : SpaceMorph {
      public override Point3d MorphPoint(Point3d point) {
        return new Point3d(point.X + 1, point.Y, point.Z);
      }
    }
  }
}
