using AdSecCore.Functions;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class CreateLoadFunctionTests {
    private readonly CreateLoadFunction _function;

    public CreateLoadFunctionTests() {
      _function = new CreateLoadFunction();
    }

    [Fact]
    public void ComputeWithValidInputsCreatesLoad() {
      _function.ForceInput.Value = Force.FromNewtons(2);
      _function.MomentYInput.Value = Moment.FromKilonewtonMeters(3);
      _function.MomentZInput.Value = Moment.FromKilonewtonMeters(4);

      _function.Compute();

      var result = _function.LoadOutput.Value as ILoad;
      Assert.NotNull(result);
      Assert.Equal(2, result.X.As(ForceUnit.Newton));
      Assert.Equal(3, result.YY.As(MomentUnit.NewtonMeter));
      Assert.Equal(4, result.ZZ.As(MomentUnit.NewtonMeter));
    }

    [Fact]
    public void GetAllInputAttributesReturnsThreeParameters() {
      var inputs = _function.GetAllInputAttributes();

      Assert.Equal(3, inputs.Length);
      Assert.Contains(inputs, x => x.Name.Contains("Fx"));
      Assert.Contains(inputs, x => x.Name.Contains("Myy"));
      Assert.Contains(inputs, x => x.Name.Contains("Mzz"));
    }

    [Fact]
    public void GetAllOutputAttributesReturnsOneParameters() {
      var outputs = _function.GetAllOutputAttributes();
      Assert.Single(outputs);
      Assert.Contains(outputs, x => x.Name.Contains("Load"));
    }

    [Fact]
    public void UpdateParameterUpdatesUnitDisplayNamesCorrectly() {
      _function.ForceUnit = ForceUnit.KiloPond;
      _function.MomentUnit = MomentUnit.KilopoundForceFoot;

      Assert.Contains("Fx [kp]", _function.ForceInput.Name);
      Assert.Contains("Myy [kipf·ft]", _function.MomentYInput.Name);
      Assert.Contains("Mzz [kipf·ft]", _function.MomentZInput.Name);
    }


    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(-1, -2, -3)]
    [InlineData(0, 0, 0)]
    public void ComputeWithDifferentValuesCreatesCorrectLoad(
      double strain, double curvY, double curvZ) {

      _function.ForceInput.Value = Force.FromNewtons(strain);
      _function.MomentYInput.Value = Moment.FromNewtonMeters(curvY);
      _function.MomentZInput.Value = Moment.FromNewtonMeters(curvZ);

      _function.Compute();

      var result = _function.LoadOutput.Value as ILoad;
      Assert.NotNull(result);
      Assert.Equal(strain, result.X.As(ForceUnit.Newton), 6);
      Assert.Equal(curvY, result.YY.As(MomentUnit.NewtonMeter), 6);
      Assert.Equal(curvZ, result.ZZ.As(MomentUnit.NewtonMeter), 6);
    }
  }
}
