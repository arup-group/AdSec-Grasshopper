using AdSecCore;
using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGHTests.Functions {
  public class ConcreteStressStrainFunctionTests {
    private readonly ConcreteStressStrainFunction _component;
    private static SectionSolution? Solution { get; set; } = null;
    private readonly DoubleComparer comparer = new DoubleComparer();
    public ConcreteStressStrainFunctionTests() {
      _component = new ConcreteStressStrainFunction();
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      _component.SolutionInput.Value = Solution;
      _component.LoadInput.Value = ILoad.Create(Force.FromKilonewtons(-600), Moment.FromKilonewtonMeters(50), Moment.Zero);
      _component.VertexInput.Value = IPoint.Create(new Length(120, LengthUnit.Millimeter), new Length(280, LengthUnit.Millimeter));
    }

    [Fact]
    public void ErrorMessageWhenVertexInputIsNull() {
      _component.VertexInput.Value = null;
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
    public void ShouldHaveErrorMessageForNullSolution() {
      _component.SolutionInput.Value = null;
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
    public void ShouldComputeCorrectlyForLoad() {
      _component.Compute();
      Assert.Equal(-457.65, _component.UlsStrainOutput.Value.As(StrainUnit.MicroStrain), comparer);
      Assert.Equal(-68.29, _component.SlsStrainOutput.Value.As(StrainUnit.MicroStrain), comparer);
      Assert.Equal(-1.81, _component.UlsStressOutput.Value.As(PressureUnit.Megapascal), comparer);
      Assert.Equal(-1.08, _component.SlsStressOutput.Value.As(PressureUnit.Megapascal), comparer);
    }

    [Fact]
    public void ShouldComputeCorrectlyForDeformation() {
      _component.LoadInput.Value = IDeformation.Create(Strain.FromRatio(-0.003), Curvature.Zero, Curvature.Zero);
      _component.Compute();
      Assert.Equal(-3000, _component.UlsStrainOutput.Value.As(StrainUnit.MicroStrain), comparer);
      Assert.Equal(-3000, _component.SlsStrainOutput.Value.As(StrainUnit.MicroStrain), comparer);
      Assert.Equal(-4.47, _component.UlsStressOutput.Value.As(PressureUnit.Megapascal), comparer);
      Assert.Equal(-47.43, _component.SlsStressOutput.Value.As(PressureUnit.Megapascal), comparer);
    }

    [Fact]
    public void ShouldRefreshDescription() {
      _component.StrainUnitResult = StrainUnit.MicroStrain;
      _component.StressUnitResult = PressureUnit.Pascal;
      _component.UpdateParameter();
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
  }
}
