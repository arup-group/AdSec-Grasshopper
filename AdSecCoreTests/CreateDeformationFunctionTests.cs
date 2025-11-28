using AdSecCore.Functions;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class CreateDeformationLoadFunctionTests {
    private readonly CreateDeformationLoadFunction loadFunction;

    public CreateDeformationLoadFunctionTests() {
      loadFunction = new CreateDeformationLoadFunction();
    }

    private void AssertName() {
      Assert.Contains("εx [ε]", loadFunction.StrainInput.Name);
      Assert.Contains("κyy [m⁻¹]", loadFunction.CurvatureYInput.Name);
      Assert.Contains("κzz [m⁻¹]", loadFunction.CurvatureZInput.Name);
    }

    [Fact]
    public void ComputeWithValidInputsCreatesDeformation() {
      loadFunction.StrainInput.Value = Strain.FromRatio(0.002);
      loadFunction.CurvatureYInput.Value = Curvature.FromPerMeters(0.001);
      loadFunction.CurvatureZInput.Value = Curvature.FromPerMeters(0.001);

      loadFunction.Compute();

      var result = loadFunction.DeformationOutput.Value as IDeformation;
      Assert.NotNull(result);
      Assert.Equal(0.002, result.X.As(StrainUnit.Ratio));
      Assert.Equal(0.001, result.YY.As(CurvatureUnit.PerMeter));
      Assert.Equal(0.001, result.ZZ.As(CurvatureUnit.PerMeter));
    }

    [Fact]
    public void GetAllInputAttributesReturnsThreeParameters() {
      var inputs = loadFunction.GetAllInputAttributes();

      Assert.Equal(3, inputs.Length);
      Assert.Contains(inputs, x => x.Name.Contains("εx"));
      Assert.Contains(inputs, x => x.Name.Contains("κyy"));
      Assert.Contains(inputs, x => x.Name.Contains("κzz"));
    }

    [Fact]
    public void GetAllOutputAttributesReturnsOneParameters() {
      var outputs = loadFunction.GetAllOutputAttributes();
      Assert.Single(outputs);
      Assert.Contains(outputs, x => x.Name.Contains("Load"));
    }

    [Fact]
    public void UpdateParameterUpdatesUnitDisplayNamesCorrectly() {
      loadFunction.StrainUnitResult = StrainUnit.MicroStrain;
      loadFunction.CurvatureUnit = CurvatureUnit.PerMillimeter;

      Assert.Contains("εx [µε]", loadFunction.StrainInput.Name);
      Assert.Contains("κyy [mm⁻¹]", loadFunction.CurvatureYInput.Name);
      Assert.Contains("κzz [mm⁻¹]", loadFunction.CurvatureZInput.Name);
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenLengthUnitGeometryIsSet() {
      loadFunction.LengthUnitGeometry = LengthUnit.Meter;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenLengthUnitResultIsSet() {
      loadFunction.LengthUnitResult = LengthUnit.Millimeter;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenMomentUnitIsSet() {
      loadFunction.MomentUnit = MomentUnit.NewtonMeter;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenAxialStiffnessUnitIsSet() {
      loadFunction.AxialStiffnessUnit = AxialStiffnessUnit.Newton;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenBendingStiffnessUnitIsSet() {
      loadFunction.BendingStiffnessUnit = BendingStiffnessUnit.NewtonSquareMeter;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenStressUnitResultIsSet() {
      loadFunction.StressUnitResult = PressureUnit.Megapascal;
      AssertName();
    }

    [Theory]
    [InlineData(0.002, 0.001, 0.001)]
    [InlineData(-0.002, -0.001, -0.001)]
    [InlineData(0, 0, 0)]
    public void ComputeWithDifferentValuesCreatesCorrectDeformation(
      double strain, double curvY, double curvZ) {

      loadFunction.StrainInput.Value = Strain.FromRatio(strain);
      loadFunction.CurvatureYInput.Value = Curvature.FromPerMeters(curvY);
      loadFunction.CurvatureZInput.Value = Curvature.FromPerMeters(curvZ);

      loadFunction.Compute();

      var result = loadFunction.DeformationOutput.Value as IDeformation;
      Assert.NotNull(result);
      Assert.Equal(strain, result.X.As(StrainUnit.Ratio), 6);
      Assert.Equal(curvY, result.YY.As(CurvatureUnit.PerMeter), 6);
      Assert.Equal(curvZ, result.ZZ.As(CurvatureUnit.PerMeter), 6);
    }
  }
}
