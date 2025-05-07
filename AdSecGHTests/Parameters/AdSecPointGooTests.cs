using AdSecGH.Parameters;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;
using Rhino.Geometry.Morphs;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecPointGooTests {
    private readonly AdSecPointGoo _testGoo;

    public AdSecPointGooTests() {
      _testGoo = new AdSecPointGoo(new Length(1, LengthUnit.Inch), new Length(2, LengthUnit.Meter));
    }

    [Fact]
    public void GetBoundingBox_ReturnsValidBoundingBox_ForTransform() {
      var result = _testGoo.GetBoundingBox(Transform.Identity);

      Assert.True(_testGoo.IsValid);
      Assert.True(result.IsValid);
    }

    [Fact]
    public void GetBoundingBox_ReturnsValidBoundingBox_ForValidTransform() {
      var result = _testGoo.GetBoundingBox(new Transform(1));

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
    public void DuplicateGeometry_ReturnsNewGoo() {
      var duplicated = _testGoo.DuplicateGeometry();

      Assert.NotNull(duplicated);
      Assert.IsType<AdSecPointGoo>(duplicated);
      Assert.Equal(_testGoo.Value, ((AdSecPointGoo)duplicated).Value);
      Assert.Equal(_testGoo.AdSecPoint.Y, ((AdSecPointGoo)duplicated).AdSecPoint.Y);
      Assert.Equal(_testGoo.AdSecPoint.Z, ((AdSecPointGoo)duplicated).AdSecPoint.Z);
    }

  }
}
