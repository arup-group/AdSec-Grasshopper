using AdSecCore.Functions;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.Mesh;
using Oasys.AdSec.StandardMaterials;

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
      var strain = Strain.FromRatio(0.001);
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
      Assert.Equal("Linear", StressStrainCurveFunction.GetCurveTypeFromInterface(linearCurve));
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
      Assert.Equal("Bilinear", StressStrainCurveFunction.GetCurveTypeFromInterface(biLinearCurve));
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
      Assert.Equal("Explicit", StressStrainCurveFunction.GetCurveTypeFromInterface(explicitCurve));
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
      Assert.Equal("FibModelCode", StressStrainCurveFunction.GetCurveTypeFromInterface(fibCurve));
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
      Assert.Equal("ManderConfined", StressStrainCurveFunction.GetCurveTypeFromInterface(manderCurve));
      Assert.Equal(Pressure.From(confined, _function.StressUnitResult).Value, manderCurve.ConfinedStrength.As(_function.StressUnitResult));
      Assert.Equal(Pressure.From(unconfined, _function.StressUnitResult).Value, manderCurve.UnconfinedStrength.As(_function.StressUnitResult));
      Assert.Equal(Pressure.From(initialModulus, _function.StressUnitResult).Value, manderCurve.InitialModulus.As(_function.StressUnitResult));
      Assert.Equal(Strain.From(failureStrain, _function.StrainUnitResult).Value, manderCurve.FailureStrain.As(_function.StrainUnitResult));
    }

    [Fact]
    public void TestManderModelCurve() {
      _function.SelectedCurveType = StressStrainCurveType.Mander;
      var initialModulus = CreateInitialModulus();
      var failureStrain = CreateFailureStrain();
      var peakPoint = CreatePeakPoint();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(outputCurve);
      var manderCurve = (IManderStressStrainCurve)outputCurve.IStressStrainCurve;
      Assert.Equal("Mander", StressStrainCurveFunction.GetCurveTypeFromInterface(manderCurve));
      AssertStressAndStrain(peakPoint, manderCurve.PeakPoint);
      Assert.Equal(Pressure.From(initialModulus, _function.StressUnitResult).Value, manderCurve.InitialModulus.As(_function.StressUnitResult));
      Assert.Equal(Strain.From(failureStrain, _function.StrainUnitResult).Value, manderCurve.FailureStrain.As(_function.StrainUnitResult));
    }

    [Fact]
    public void TestParabolaRectangleCurve() {
      _function.SelectedCurveType = StressStrainCurveType.ParabolaRectangle;
      var yieldPoint = CreateYieldPoint();
      var failureStrain = CreateFailureStrain();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(outputCurve);
      var parabolaCurve = (IParabolaRectangleStressStrainCurve)outputCurve.IStressStrainCurve;
      Assert.Equal("ParabolaRectangle", StressStrainCurveFunction.GetCurveTypeFromInterface(parabolaCurve));
      AssertStressAndStrain(yieldPoint, parabolaCurve.YieldPoint);
      Assert.Equal(Strain.From(failureStrain, _function.StrainUnitResult).Value, parabolaCurve.FailureStrain.As(_function.StrainUnitResult));
    }

    [Fact]
    public void TestParkCurve() {
      _function.SelectedCurveType = StressStrainCurveType.Park;
      var yieldPoint = CreateYieldPoint();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(outputCurve);
      var ParkCurve = (IParkStressStrainCurve)outputCurve.IStressStrainCurve;
      Assert.Equal("Park", StressStrainCurveFunction.GetCurveTypeFromInterface(ParkCurve));
      AssertStressAndStrain(yieldPoint, ParkCurve.YieldPoint);
    }

    [Fact]
    public void TestPopovicsCurve() {
      _function.SelectedCurveType = StressStrainCurveType.Popovics;
      var peakPoint = CreatePeakPoint();
      var failureStrain = CreateFailureStrain();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(outputCurve);
      var popovicsCurve = (IPopovicsStressStrainCurve)outputCurve.IStressStrainCurve;
      Assert.Equal("Popovics", StressStrainCurveFunction.GetCurveTypeFromInterface(popovicsCurve));
      AssertStressAndStrain(peakPoint, popovicsCurve.PeakPoint);
      Assert.Equal(Strain.From(failureStrain, _function.StrainUnitResult).Value, popovicsCurve.FailureStrain.As(_function.StrainUnitResult));

    }

    [Fact]
    public void TestRectangularCurve() {
      _function.SelectedCurveType = StressStrainCurveType.Rectangular;
      var yieldPoint = CreateYieldPoint();
      var failureStrain = CreateFailureStrain();
      _function.Compute();
      var outputCurve = _function.OutputCurve.Value;
      Assert.NotNull(outputCurve);
      var rectangularCurve = (IRectangularStressStrainCurve)outputCurve.IStressStrainCurve;
      Assert.Equal("Rectangular", StressStrainCurveFunction.GetCurveTypeFromInterface(rectangularCurve));
      AssertStressAndStrain(yieldPoint, rectangularCurve.YieldPoint);
      Assert.Equal(Strain.From(failureStrain, _function.StrainUnitResult).Value, rectangularCurve.FailureStrain.As(_function.StrainUnitResult));

    }

    [Theory]
    [InlineData(StressStrainCurveType.Bilinear, "Yield Point", "AdSec Stress Strain Point representing the Yield Point", "Failure Point", "AdSec Stress Strain Point representing the Failure Point")]
    [InlineData(StressStrainCurveType.Explicit, "StressStrainPoints", "AdSec Stress Strain Point representing the StressStrainCurve as a Polyline", "", "")]
    [InlineData(StressStrainCurveType.FibModelCode, "Peak Point", "AdSec Stress Strain Point representing the FIB model's Peak Point", "Initial Modulus [Pa]", "Initial Modulus")]
    [InlineData(StressStrainCurveType.Linear, "Failure Point", "AdSec Stress Strain Point representing the Failure Point", "", "")]
    [InlineData(StressStrainCurveType.ManderConfined, "Unconfined Strength [Pa]", "Unconfined strength for Mander Confined Model", "Confined Strength [Pa]", "Confined strength for Mander Confined Model")]
    [InlineData(StressStrainCurveType.Mander, "Peak Point", "AdSec Stress Strain Point representing the Mander model's Peak Point", "Initial Modulus [Pa]", "Initial Modulus from Mander")]
    [InlineData(StressStrainCurveType.ParabolaRectangle, "Yield Point", "AdSec Stress Strain Point representing the Yield Point", "Failure Strain [%]", "Failure Strain")]
    [InlineData(StressStrainCurveType.Park, "Yield Point", "AdSec Stress Strain Point representing the Yield Point", "Failure Strain [%]", "Failure Strain")]
    [InlineData(StressStrainCurveType.Popovics, "Peak Point", "AdSec Stress Strain Point representing the Peak Point", "Failure Strain [%]", "Failure strain from Popovic model")]
    [InlineData(StressStrainCurveType.Rectangular, "Yield Point", "AdSec Stress Strain Point representing the Yield Point", "Failure Strain [%]", "Failure Strain")]
    public void UpdateParameter_SetsParameterNamesAndDescriptions(
      StressStrainCurveType type,
      string expectedName1, string expectedDesc1,
      string expectedName2, string expectedDesc2) {

      var function = new StressStrainCurveFunction();
      function.SelectedCurveType = type;
      function.StressUnitResult = PressureUnit.Pascal;
      function.StrainUnitResult = StrainUnit.Percent;
      switch (type) {
        case StressStrainCurveType.Bilinear:
          Assert.Equal(expectedName1, function.YieldPoint.Name);
          Assert.Equal(expectedDesc1, function.YieldPoint.Description);
          Assert.Equal(expectedName2, function.FailurePoint.Name);
          Assert.Equal(expectedDesc2, function.FailurePoint.Description);
          break;
        case StressStrainCurveType.Explicit:
          Assert.Equal(expectedName1, function.StressStrainPoints.Name);
          Assert.Equal(expectedDesc1, function.StressStrainPoints.Description);
          break;
        case StressStrainCurveType.FibModelCode:
          Assert.Equal(expectedName1, function.PeakPoint.Name);
          Assert.Equal(expectedDesc1, function.PeakPoint.Description);
          Assert.Contains(expectedName2, function.InitialModulus.Name);
          Assert.Contains(expectedDesc2, function.InitialModulus.Description);
          Assert.Contains("Failure strain from FIB model code", function.FailureStrain.Description);
          Assert.Contains("Failure Strain [%]", function.FailureStrain.Name);
          break;
        case StressStrainCurveType.Linear:
          Assert.Equal(expectedName1, function.FailurePoint.Name);
          Assert.Equal(expectedDesc1, function.FailurePoint.Description);
          break;
        case StressStrainCurveType.ManderConfined:
          Assert.StartsWith(expectedName1, function.UnconfinedStrength.Name);
          Assert.Equal(expectedDesc1, function.UnconfinedStrength.Description);
          Assert.StartsWith(expectedName2, function.ConfinedStrength.Name);
          Assert.Equal(expectedDesc2, function.ConfinedStrength.Description);
          Assert.Contains("Initial Modulus [Pa]", function.InitialModulus.Name);
          Assert.Contains("Initial Modulus from Mander Confined Model", function.InitialModulus.Description);
          Assert.Contains("Failure Strain [%]", function.FailureStrain.Name);
          Assert.Contains("Failure strain from Mander Confined Model", function.FailureStrain.Description);
          break;
        case StressStrainCurveType.Mander:
          Assert.Equal(expectedName1, function.PeakPoint.Name);
          Assert.Equal(expectedDesc1, function.PeakPoint.Description);
          Assert.Contains(expectedName2, function.InitialModulus.Name);
          Assert.Contains(expectedDesc2, function.InitialModulus.Description);
          Assert.Contains("Failure Strain [%]", function.FailureStrain.Name);
          Assert.Contains("Failure strain from Mander", function.FailureStrain.Description);
          break;
        case StressStrainCurveType.ParabolaRectangle:
        case StressStrainCurveType.Park:
        case StressStrainCurveType.Rectangular:
          Assert.Equal(expectedName1, function.YieldPoint.Name);
          Assert.Equal(expectedDesc1, function.YieldPoint.Description);
          Assert.Contains("Failure Strain [%]", function.FailureStrain.Name);
          break;
        case StressStrainCurveType.Popovics:
          Assert.Equal(expectedName1, function.PeakPoint.Name);
          Assert.Equal(expectedDesc1, function.PeakPoint.Description);
          Assert.Contains(expectedName2, function.FailureStrain.Name);
          Assert.Contains(expectedDesc2, function.FailureStrain.Description);
          break;
      }
    }

    [Theory]
    [InlineData(StressStrainCurveType.Bilinear, 2)]
    [InlineData(StressStrainCurveType.Explicit, 1)]
    [InlineData(StressStrainCurveType.FibModelCode, 3)]
    [InlineData(StressStrainCurveType.Linear, 1)]
    [InlineData(StressStrainCurveType.ManderConfined, 4)]
    [InlineData(StressStrainCurveType.Mander, 3)]
    [InlineData(StressStrainCurveType.ParabolaRectangle, 2)]
    [InlineData(StressStrainCurveType.Park, 1)]
    [InlineData(StressStrainCurveType.Popovics, 2)]
    [InlineData(StressStrainCurveType.Rectangular, 2)]
    public void GetAllInputAttributesReturnsExpectedCount(StressStrainCurveType type, int expectedCount) {
      var function = new StressStrainCurveFunction();
      function.SelectedCurveType = type;
      var attrs = function.GetAllInputAttributes();
      Assert.Equal(expectedCount, attrs.Length);
    }

    [Fact]
    public void ShouldHaveOneOutputAttribute() {
      var function = new StressStrainCurveFunction();
      foreach (StressStrainCurveType type in System.Enum.GetValues(typeof(StressStrainCurveType))) {
        function.SelectedCurveType = type;
        var attrs = function.GetAllOutputAttributes();
        Assert.Single(attrs);
      }

    }

    [Fact]
    public void OutputCurveHasExpectedProperties() {
      var function = new StressStrainCurveFunction();
      var output = function.OutputCurve;
      Assert.NotNull(output);
      Assert.Equal("StressStrainCrv", output.Name);
      Assert.Equal("SCv", output.NickName);
      Assert.Equal("AdSec Stress Strain Curve", output.Description);
      Assert.False(output.Optional);
    }

    [Fact]
    public void MetadataHasExpectedProperties() {
      var function = new StressStrainCurveFunction();
      var meta = function.Metadata;
      Assert.NotNull(meta);
      Assert.Equal("Create StressStrainCrv", meta.Name);
      Assert.Equal("StressStrainCrv", meta.NickName);
      Assert.Equal("Create a Stress Strain Curve for AdSec Material", meta.Description);
    }
    [Fact]
    public void TestDefaultCurveType() {
      IReinforcement material = Reinforcement.Steel.IS456.Edition_2000.S415;
      Assert.Equal("DefaultCurve", StressStrainCurveFunction.GetCurveTypeFromInterface(material.Strength.Compression));
    }
  }
}
