using AdSecGH.Parameters;

using Rhino.Geometry;
using Rhino.Geometry.Morphs;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecStressStrainPointGooTests {
    private readonly AdSecStressStrainPointGoo stressStrainPointGoo;

    public AdSecStressStrainPointGooTests() {
      var point = new Point3d(0.1, 0.2, 0);
      stressStrainPointGoo = new AdSecStressStrainPointGoo(point);
    }

    [Fact]
    public void ShouldBeValid() {
      Assert.True(stressStrainPointGoo.IsValid);
    }

    [Fact]
    public void IsValid_ReturnsTrue_Always() { // as it is geometry
      Assert.True(stressStrainPointGoo.IsValid);
    }

    [Fact]
    public void DuplicateGeometry_ReturnsValidValue() {
      var result = stressStrainPointGoo.DuplicateGeometry();
      Assert.Equal(result.ToString(), stressStrainPointGoo.ToString());
      Assert.Equal(result.IsValid, stressStrainPointGoo.IsValid);
    }

    [Fact]
    public void GetBoundingBox_ReturnsValidValue() {
      var result = stressStrainPointGoo.GetBoundingBox(new Transform());
      Assert.NotEqual(BoundingBox.Empty, result);
    }

    [Fact]
    public void Morph_ReturnsNull_Always() {
      var result = stressStrainPointGoo.Morph(null);
      Assert.Null(result);

      var result2 = stressStrainPointGoo.Morph(new FlowSpaceMorph(new PolylineCurve(), new PolylineCurve(), false));
      Assert.Null(result2);
    }

    [Fact]
    public void Transform_ReturnsNull_Always() {
      var result = stressStrainPointGoo.Transform(new Transform());
      Assert.Null(result);

      var result2 = stressStrainPointGoo.Transform(Transform.Unset);
      Assert.Null(result2);
    }

  }
}
