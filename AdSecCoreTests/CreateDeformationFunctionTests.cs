using AdSecCore.Functions;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class CreateDeformationFunctionTests {
    private readonly CreateDeformationFunction _function;

    public CreateDeformationFunctionTests() {
      _function = new CreateDeformationFunction();
    }

    private void AssertName() {
      Assert.Contains("εx [ε]", _function.StrainInput.Name);
      Assert.Contains("κyy [m⁻¹]", _function.CurvatureYInput.Name);
      Assert.Contains("κzz [m⁻¹]", _function.CurvatureZInput.Name);
    }

    [Fact]
    public void ComputeWithValidInputsCreatesDeformation() {
      _function.StrainInput.Value = Strain.FromRatio(0.002);
      _function.CurvatureYInput.Value = Curvature.FromPerMeters(0.001);
      _function.CurvatureZInput.Value = Curvature.FromPerMeters(0.001);

      _function.Compute();

      var result = _function.DeformationOutput.Value as IDeformation;
      Assert.NotNull(result);
      Assert.Equal(0.002, result.X.As(StrainUnit.Ratio));
      Assert.Equal(0.001, result.YY.As(CurvatureUnit.PerMeter));
      Assert.Equal(0.001, result.ZZ.As(CurvatureUnit.PerMeter));
    }

    [Fact]
    public void GetAllInputAttributesReturnsThreeParameters() {
      var inputs = _function.GetAllInputAttributes();

      Assert.Equal(3, inputs.Length);
      Assert.Contains(inputs, x => x.Name.Contains("εx"));
      Assert.Contains(inputs, x => x.Name.Contains("κyy"));
      Assert.Contains(inputs, x => x.Name.Contains("κzz"));
    }

    [Fact]
    public void GetAllOutputAttributesReturnsOneParameters() {
      var outputs = _function.GetAllOutputAttributes();
      Assert.Single(outputs);
      Assert.Contains(outputs, x => x.Name.Contains("Load"));
    }

    [Fact]
    public void UpdateParameterUpdatesUnitDisplayNamesCorrectly() {
      _function.StrainUnitResult = StrainUnit.MicroStrain;
      _function.CurvatureUnit = CurvatureUnit.PerMillimeter;

      Assert.Contains("εx [µε]", _function.StrainInput.Name);
      Assert.Contains("κyy [mm⁻¹]", _function.CurvatureYInput.Name);
      Assert.Contains("κzz [mm⁻¹]", _function.CurvatureZInput.Name);
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenLengthUnitGeometryIsSet() {
      _function.LengthUnitGeometry = LengthUnit.Meter;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenLengthUnitResultIsSet() {
      _function.LengthUnitResult = LengthUnit.Millimeter;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenMomentUnitIsSet() {
      _function.MomentUnit = MomentUnit.NewtonMeter;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenAxialStiffnessUnitIsSet() {
      _function.AxialStiffnessUnit = AxialStiffnessUnit.Newton;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenBendingStiffnessUnitIsSet() {
      _function.BendingStiffnessUnit = BendingStiffnessUnit.NewtonSquareMeter;
      AssertName();
    }

    [Fact]
    public void ParameterShouldDisplayDefaultNamesWhenStressUnitResultIsSet() {
      _function.StressUnitResult = PressureUnit.Megapascal;
      AssertName();
    }

    [Theory]
    [InlineData(0.002, 0.001, 0.001)]
    [InlineData(-0.002, -0.001, -0.001)]
    [InlineData(0, 0, 0)]
    public void ComputeWithDifferentValuesCreatesCorrectDeformation(
      double strain, double curvY, double curvZ) {

      _function.StrainInput.Value = Strain.FromRatio(strain);
      _function.CurvatureYInput.Value = Curvature.FromPerMeters(curvY);
      _function.CurvatureZInput.Value = Curvature.FromPerMeters(curvZ);

      _function.Compute();

      var result = _function.DeformationOutput.Value as IDeformation;
      Assert.NotNull(result);
      Assert.Equal(strain, result.X.As(StrainUnit.Ratio), 6);
      Assert.Equal(curvY, result.YY.As(CurvatureUnit.PerMeter), 6);
      Assert.Equal(curvZ, result.ZZ.As(CurvatureUnit.PerMeter), 6);
    }
  }
}
