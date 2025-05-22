using System.Drawing;

using AdSecGH.Parameters;
using AdSecGH.UI;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using OasysUnits;
using OasysUnits.Units;

using Rhino;
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
    public void CanCreateFromInvalidPoint() {
      var newPointGoo = new AdSecStressStrainPointGoo(Point3d.Unset);
      Assert.True(newPointGoo.IsValid);
    }

    [Fact]
    public void CanCreateFromOtherPointGoo() {
      var newPointGoo = new AdSecStressStrainPointGoo(stressStrainPointGoo);
      Assert.True(newPointGoo.IsValid);
    }

    [Fact]
    public void CanCreateFromPressureAndStressValues() {
      var pressure = new Pressure(0.1, PressureUnit.Pascal);
      var stress = new Strain(0.2, StrainUnit.Ratio);
      var newPointGoo = new AdSecStressStrainPointGoo(pressure, stress);
      Assert.True(newPointGoo.IsValid);
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
    public void BoundingBox_ReturnsValidValue() {
      var result = stressStrainPointGoo.Boundingbox;
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

    [Fact]
    public void ShouldDrawOnViewPort() {
      using var doc = RhinoDoc.Create(string.Empty);
      var ghPreviewWireArgs = ComponentTestHelper.CreatePreviewArgs(doc, Color.White);

      stressStrainPointGoo.DrawViewportWires(ghPreviewWireArgs);

      Assert.NotEmpty(stressStrainPointGoo.DrawInstructionsList);
      Assert.Single(stressStrainPointGoo.DrawInstructionsList);
      Assert.Equal(Colour.OasysYellow, stressStrainPointGoo.DrawInstructionsList[0].Color);
    }
  }
}
