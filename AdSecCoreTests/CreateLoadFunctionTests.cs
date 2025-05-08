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
    }

    [Fact]
    public void GetAllOutputAttributesReturnsOneParameters() {
      var outputs = _function.GetAllOutputAttributes();
      Assert.Single(outputs);
    }

    [Fact]
    public void ParametersHaveCorrectNames() {
      Assert.Equal("Fx", _function.ForceInput.Name);
      Assert.Equal("Myy", _function.MomentYInput.Name);
      Assert.Equal("Mzz", _function.MomentZInput.Name);
      Assert.Equal("Load", _function.LoadOutput.Name);
    }

    [Fact]
    public void ParametersHaveCorrectNickNames() {
      Assert.Equal("X", _function.ForceInput.NickName);
      Assert.Equal("YY", _function.MomentYInput.NickName);
      Assert.Equal("ZZ", _function.MomentZInput.NickName);
      Assert.Equal("Ld", _function.LoadOutput.NickName);
    }

    [Fact]
    public void ParametersHaveCorrectDescriptions() {
      Assert.Contains("The axial force", _function.ForceInput.Description);
      Assert.Contains("The moment about local y-axis", _function.MomentYInput.Description);
      Assert.Contains("The moment about local z-axis", _function.MomentZInput.Description);
      Assert.Contains("AdSec Load", _function.LoadOutput.Description);
    }

    [Fact]
    public void ParametersOptionalPropertiesAreCorrect() {
      Assert.True(_function.ForceInput.Optional);
      Assert.True(_function.MomentYInput.Optional);
      Assert.True(_function.MomentZInput.Optional);
    }

    [Fact]
    public void ParametersDataAccessAreCorrect() {
      Assert.Equal(Access.Item, _function.ForceInput.Access);
      Assert.Equal(Access.Item, _function.MomentYInput.Access);
      Assert.Equal(Access.Item, _function.MomentZInput.Access);
      Assert.Equal(Access.Item, _function.LoadOutput.Access);
    }

    [Fact]
    public void OrganisationHasCorrectValues() {
      Assert.Equal("AdSec", _function.Organisation.Category);
      Assert.Equal("Loads", _function.Organisation.SubCategory.Trim());
    }

    [Fact]
    public void MetaDataHasCorrectValues() {
      Assert.Equal("Create Load", _function.Metadata.Name);
      Assert.Equal("Load", _function.Metadata.NickName);
      Assert.Contains("Create an AdSec Load", _function.Metadata.Description);
    }

    [Fact]
    public void UpdatesForceUnitDisplayNamesCorrectly() {
      _function.ForceUnit = ForceUnit.KiloPond;
      Assert.Contains("Fx [kp]", _function.ForceInput.Name);
      Assert.Contains("Myy [N·m]", _function.MomentYInput.Name);
      Assert.Contains("Mzz [N·m]", _function.MomentZInput.Name);
    }

    [Fact]
    public void UpdatesMomentUnitDisplayNamesCorrectly() {
      _function.MomentUnit = MomentUnit.KilopoundForceFoot;
      Assert.Contains("Fx [N]", _function.ForceInput.Name);
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
