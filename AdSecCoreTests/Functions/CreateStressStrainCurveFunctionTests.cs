using AdSecCore.Functions;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;


namespace AdSecCoreTests.Functions {
  public class StressStrainCurveFunctionTests {
    private readonly StressStrainCurveFunction _function;
    public StressStrainCurveFunctionTests() {
      _function = new StressStrainCurveFunction();
    }

    private IStressStrainPoint CreateFailurePoint() {
      var strain = Strain.FromRatio(0.0035);
      var stress = Pressure.FromPascals(10);
      _function.FailurePoint.Value = IStressStrainPoint.Create(stress, strain);
      return _function.FailurePoint.Value;
    }

    [Fact]
    public void TestLinearCurve() {
      _function.SelectedCurveType = StressStrainCurveType.Linear;
      var failureStrain = CreateFailurePoint();
      _function.Compute();
      Assert.NotNull(_function.OutputCurve.Value);
      Assert.Equal(failureStrain.Strain.As(StrainUnit.Ratio), _function.OutputCurve.Value.IStressStrainCurve.FailureStrain.As(StrainUnit.Ratio));
      Assert.Equal(failureStrain.Stress.As(PressureUnit.Pascal), _function.OutputCurve.Value.IStressStrainCurve.StressAt(_function.OutputCurve.Value.IStressStrainCurve.FailureStrain).As(PressureUnit.Pascal));
    }
  }
}
