using AdSecCore;
using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGHTests.Functions {
  public class RebarStressStrainFunctionTests {
    private readonly RebarStressStrainFunction _component;
    private static SectionSolution? Solution { get; set; } = null;
    private readonly DoubleComparer comparer = new DoubleComparer();

    public RebarStressStrainFunctionTests() {
      _component = new RebarStressStrainFunction();
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      _component.SolutionInput.Value = Solution;
      _component.LoadInput.Value = ILoad.Create(Force.FromKilonewtons(-600), Moment.FromKilonewtonMeters(50), Moment.Zero);
      _component.VertexInput.Value = IPoint.Create(new Length(120, LengthUnit.Millimeter), new Length(280, LengthUnit.Millimeter));

    }

    [Fact]
    public void ShouldHaveErrorMessageForNullSolution() {
      _component.SolutionInput.Value = null;
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveErrorMessageForNullSolutionParameter() {
      _component.SolutionInput = null;
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveErrorMessageForNullLoad() {
      _component.LoadInput.Value = null;
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveErrorMessageForNullLoadPrameter() {
      _component.LoadInput = null;
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveWarningForInvalidLoad() {
      _component.LoadInput.Value = ILoad.Create(Force.Zero, Moment.Zero, Moment.Zero);
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveWarningForInvalidDeformation() {
      _component.LoadInput.Value = IDeformation.Create(Strain.Zero, Curvature.Zero, Curvature.Zero);
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldComputeCorrectlyForLoad() {
      _component.Compute();

      // Check results exist
      Assert.NotEmpty(_component.Points);
      Assert.NotEmpty(_component.StrainsULS);
      Assert.NotEmpty(_component.StressesULS);
      Assert.NotEmpty(_component.StrainsSLS);
      Assert.NotEmpty(_component.StressesSLS);
      // Check first rebar values
      Assert.Equal(-1011.6, _component.StrainsULS[0].As(StrainUnit.MicroStrain), comparer);
      Assert.Equal(-326.7, _component.StrainsSLS[0].As(StrainUnit.MicroStrain), comparer);
      Assert.Equal(-202.3, _component.StressesULS[0].As(PressureUnit.Megapascal), comparer);
      Assert.Equal(-65.35, _component.StressesSLS[0].As(PressureUnit.Megapascal), comparer);
    }

    [Fact]
    public void ShouldComputeCorrectlyForDeformation() {
      _component.LoadInput.Value = IDeformation.Create(
        Strain.FromRatio(-0.003),
        Curvature.Zero,
        Curvature.Zero);
      _component.Compute();

      Assert.Equal(-3000, _component.StrainsULS[0].As(StrainUnit.MicroStrain), comparer);
      Assert.Equal(-3000, _component.StrainsSLS[0].As(StrainUnit.MicroStrain), comparer);
      Assert.Equal(-360.86, _component.StressesULS[0].As(PressureUnit.Megapascal), comparer);
      Assert.Equal(-415, _component.StressesSLS[0].As(PressureUnit.Megapascal), comparer);
    }

    [Fact]
    public void ShouldRefreshDescription() {
      _component.StrainUnitResult = StrainUnit.MicroStrain;
      _component.StressUnitResult = PressureUnit.Pascal;
      _component.UpdateOutputParameter();

      Assert.Contains("[µε]", _component.UlsStrainOutput.Name);
      Assert.Contains("[µε]", _component.SlsStrainOutput.Name);
      Assert.Contains("[Pa]", _component.UlsStressOutput.Name);
      Assert.Contains("[Pa]", _component.SlsStressOutput.Name);
    }

    [Fact]
    public void ShouldNotComputeIfLoadTypeIsNotCorrect() {
      _component.LoadInput.Value = 5;
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveEqualNumberOfResults() {
      _component.Compute();

      var count = _component.Points.Count;
      Assert.Equal(count, _component.StrainsULS.Count);
      Assert.Equal(count, _component.StressesULS.Count);
      Assert.Equal(count, _component.StrainsSLS.Count);
      Assert.Equal(count, _component.StressesSLS.Count);
    }
  }
}
