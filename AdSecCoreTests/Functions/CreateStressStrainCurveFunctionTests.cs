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
      return _function.FailurePoint.Value = IStressStrainPoint.Create(stress, strain);
    }

    private IStressStrainPoint CreateYieldPoint() {
      var strain = Strain.FromRatio(0.002);
      var stress = Pressure.FromPascals(10);
      _function.YieldPoint.Value = IStressStrainPoint.Create(stress, strain);
      return _function.YieldPoint.Value;
    }

    private IStressStrainPoint[] CreateStressStressPoints() {
      var strain = Strain.FromRatio(0.002);
      var stress = Pressure.FromPascals(10);
      var list = new List<IStressStrainPoint>() { IStressStrainPoint.Create(Pressure.Zero, Strain.Zero), IStressStrainPoint.Create(stress, strain) };
      return _function.StressStrainPoints.Value = list.ToArray();
    }

    private double CreateInitialModulus() {
      return _function.InitialModulus.Value = 8;
    }

    private double CreateFailureStrain() {
      return _function.FailureStrain.Value = 0.0035;
    }

    private double CreateConfinedStress() {
      return _function.ConfinedStrength.Value = 2;
    }

    private double CreateUnConfinedStress() {
      return _function.UnconfinedStrength.Value = 1.5;
    }

    private IStressStrainPoint CreatePeakPoint() {
      var strain = Strain.FromRatio(0.002);
      var stress = Pressure.FromPascals(10);
      return _function.PeakPoint.Value = IStressStrainPoint.Create(stress, strain);
    }

    private void AssertStressAndStrain(IStressStrainPoint expected, IStressStrainPoint actual) {
      Assert.Equal(expected.Strain.As(StrainUnit.Ratio), actual.Strain.As(StrainUnit.Ratio));
      Assert.Equal(expected.Stress.As(PressureUnit.Pascal), actual.Stress.As(PressureUnit.Pascal));
    }

    [Fact]
    public void TestLinearCurve() {
      _function.SelectedCurveType = StressStrainCurveType.Linear;
      var failurePoint = CreateFailurePoint();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(outputCurve);
      var linearCurve = (ILinearStressStrainCurve)outputCurve.IStressStrainCurve;
      AssertStressAndStrain(failurePoint, linearCurve.FailurePoint);
    }

    [Fact]
    public void TestBiLinearCurve() {
      _function.SelectedCurveType = StressStrainCurveType.Bilinear;
      var failurePoint = CreateFailurePoint();
      var yieldPoint = CreateYieldPoint();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(_function.OutputCurve.Value);
      var biLinearCurve = (IBilinearStressStrainCurve)outputCurve.IStressStrainCurve;

      AssertStressAndStrain(failurePoint, biLinearCurve.FailurePoint);
      AssertStressAndStrain(yieldPoint, biLinearCurve.YieldPoint);
    }

    [Fact]
    public void TestExplicitCurve() {
      _function.SelectedCurveType = StressStrainCurveType.Explicit;
      var stressStressPoints = CreateStressStressPoints();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(_function.OutputCurve.Value);
      var explicitCurve = (IExplicitStressStrainCurve)outputCurve.IStressStrainCurve;
      AssertStressAndStrain(stressStressPoints[0], explicitCurve.Points[0]);
      AssertStressAndStrain(stressStressPoints[1], explicitCurve.Points[1]);
    }

    [Fact]
    public void TestFibModelCurve() {
      _function.SelectedCurveType = StressStrainCurveType.FibModelCode;
      var initialModulus = CreateInitialModulus();
      var peakPoint = CreatePeakPoint();
      var failureStrain = CreateFailureStrain();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(outputCurve);
      var fibCurve = (IFibModelCodeStressStrainCurve)outputCurve.IStressStrainCurve;
      AssertStressAndStrain(peakPoint, fibCurve.PeakPoint);
      Assert.Equal(Pressure.From(initialModulus, _function.StressUnitResult).Value, fibCurve.InitialModulus.As(_function.StressUnitResult));
      Assert.Equal(Strain.From(failureStrain, _function.StrainUnitResult).Value, fibCurve.FailureStrain.As(_function.StrainUnitResult));
    }

    [Fact]
    public void TestManderConfinedModelCurve() {
      _function.SelectedCurveType = StressStrainCurveType.ManderConfined;
      var initialModulus = CreateInitialModulus();
      var failureStrain = 1;
      var confined = CreateConfinedStress();
      var unconfined = CreateUnConfinedStress();
      _function.FailureStrain.Value = failureStrain;
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(outputCurve);
      var manderCurve = (IManderConfinedStressStrainCurve)outputCurve.IStressStrainCurve;
      Assert.Equal(Pressure.From(confined, _function.StressUnitResult).Value, manderCurve.ConfinedStrength.As(_function.StressUnitResult));
      Assert.Equal(Pressure.From(unconfined, _function.StressUnitResult).Value, manderCurve.UnconfinedStrength.As(_function.StressUnitResult));
      Assert.Equal(Pressure.From(initialModulus, _function.StressUnitResult).Value, manderCurve.InitialModulus.As(_function.StressUnitResult));
      Assert.Equal(Strain.From(failureStrain, _function.StrainUnitResult).Value, manderCurve.FailureStrain.As(_function.StrainUnitResult));
    }
  }
}
