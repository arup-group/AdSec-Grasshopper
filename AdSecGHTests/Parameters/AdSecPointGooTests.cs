using System.Diagnostics.CodeAnalysis;

using AdSecGH.Parameters;

using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;
using Rhino.Geometry.Morphs;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  [SuppressMessage("Major Bug", "S4143:Collection elements should not be replaced unconditionally",
    Justification = "<Pending>")]
  public class AdSecPointGooTests {
    private AdSecPointGoo _testGoo;

    public AdSecPointGooTests() {
      _testGoo = new AdSecPointGoo(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Meter));
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsValid_AndAdSecPointIsNotNull() {
      Assert.True(_testGoo.IsValid);
      Assert.True(_testGoo.Value.IsValid);
      Assert.NotNull(_testGoo.AdSecPoint);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIsInvalid() {
      _testGoo.Value = new Point3d(double.NaN, 2, 3);

      Assert.False(_testGoo.IsValid);
      Assert.False(_testGoo.Value.IsValid);
      Assert.NotNull(_testGoo.AdSecPoint);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenAdSecPointIsNull() {
      _testGoo = new AdSecPointGoo((IPoint)null);

      Assert.False(_testGoo.IsValid);
      Assert.Null(_testGoo.AdSecPoint);
      Assert.True(_testGoo.Value.IsValid);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIsInvalid_AndAdSecPointIsNull() {
      _testGoo = new AdSecPointGoo((AdSecPointGoo)null) { Value = new Point3d(double.NaN, 2, 3), };

      Assert.False(_testGoo.IsValid);
      Assert.Null(_testGoo.AdSecPoint);
      Assert.False(_testGoo.Value.IsValid);
    }

    [Fact]
    public void GetBoundingBox_ReturnsEmpty_WhenIsValidIsFalse() {
      _testGoo.Value = Point3d.Unset;
      var result = _testGoo.GetBoundingBox(Transform.Identity);

      Assert.False(_testGoo.IsValid);
      Assert.Equal(BoundingBox.Empty, result);
    }

    [Fact]
    public void GetBoundingBox_ReturnsValidBoundingBox_WhenIsValidIsTrue() {
      var result = _testGoo.GetBoundingBox(Transform.Identity);

      Assert.True(_testGoo.IsValid);
      Assert.True(result.IsValid);
    }

    [Fact]
    public void Transform_ReturnsNull() {
      var result = _testGoo.Transform(Transform.Identity);

      Assert.Null(result);
    }

    [Fact]
    public void Morph_ReturnsNull() {
      var result = _testGoo.Morph(new BendSpaceMorph(Point3d.Origin, Point3d.Unset, Point3d.Origin, true,
        true)); // should always return null, whatever spaceMorph is used

      Assert.Null(result);
    }

    [Fact]
    public void DuplicateGeometry_ReturnsNull_WhenIsValidIsFalse() {
      _testGoo.Value = Point3d.Unset;

      Assert.Null(_testGoo.DuplicateGeometry());
    }

    [Fact]
    public void DuplicateGeometry_ReturnsNewGoo_WhenIsValidIsTrue() {
      var duplicated = _testGoo.DuplicateGeometry();

      Assert.NotNull(duplicated);
      Assert.IsType<AdSecPointGoo>(duplicated);
      Assert.Equal(_testGoo.Value, ((AdSecPointGoo)duplicated).Value);
      Assert.Equal(_testGoo.AdSecPoint.Y, ((AdSecPointGoo)duplicated).AdSecPoint.Y);
      Assert.Equal(_testGoo.AdSecPoint.Z, ((AdSecPointGoo)duplicated).AdSecPoint.Z);
    }

  }
}
