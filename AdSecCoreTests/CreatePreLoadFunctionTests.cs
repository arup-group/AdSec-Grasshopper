using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.Reinforcement.Preloads;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class CreatePreLoadFunctionTests {
    private readonly CreatePreLoadFunction _function;

    public CreatePreLoadFunctionTests() {
      _function = new CreatePreLoadFunction();
      _function.RebarGroupInput.Value = new AdSecRebarGroup(new BuilderTopTemplateGroup().Build());
      _function.PreloadInput.Value = 10;
    }

    [Theory]
    [InlineData("force")]
    [InlineData("strain")]
    [InlineData("stress")]
    [InlineData("invalid")]
    public void ComputeWithValidInputsCreatesPreload(string force) {
      _function.PreloadInput.Name = force;
      _function.Compute();

      if (force == "invalid") {
        Assert.Contains("Invalid Preload input type.", _function.ErrorMessages);
        return;
      }

      var outPreLoad = ((ILongitudinalGroup)_function.PreloadedRebarGroupOutput.Value.Group).Preload;
      double outputForce = 0;
      switch (outPreLoad) {
        case IPreForce preForce:
          outputForce = preForce.Force.As(_function.ForceUnit);
          break;

        case IPreStrain preStrain:
          outputForce = preStrain.Strain.As(_function.MaterialStrainUnit);
          break;

        case IPreStress preStress:
          outputForce = preStress.Stress.As(_function.StressUnitResult);
          break;
      }

      var result = _function.PreloadedRebarGroupOutput.Value as AdSecRebarGroup;
      Assert.NotNull(result);
      Assert.NotNull(result.Group as ILongitudinalGroup);
      Assert.Equal(10, outputForce);
    }

    [Fact]
    public void GetAllInputAttributesReturnsTwoParameters() {
      var inputs = _function.GetAllInputAttributes();
      Assert.Equal(2, inputs.Length);
    }

    [Fact]
    public void GetAllOutputAttributesReturnsOneParameter() {
      var outputs = _function.GetAllOutputAttributes();
      Assert.Single(outputs);
    }

    [Fact]
    public void ParametersHaveCorrectNames() {
      Assert.Equal("RebarGroup", _function.RebarGroupInput.Name);
      Assert.Equal("Force", _function.PreloadInput.Name);
      Assert.Equal("Prestressed RebarGroup", _function.PreloadedRebarGroupOutput.Name);
    }

    [Fact]
    public void ParametersHaveCorrectNickNames() {
      Assert.Equal("RbG", _function.RebarGroupInput.NickName);
      Assert.Equal("P", _function.PreloadInput.NickName);
      Assert.Equal("RbG", _function.PreloadedRebarGroupOutput.NickName);
    }

    [Fact]
    public void ParametersHaveCorrectDescriptions() {
      Assert.Contains("Reinforcement Group to apply Preload", _function.RebarGroupInput.Description);
      Assert.Contains("The preload value", _function.PreloadInput.Description);
      Assert.Contains("Preloaded Rebar Group for AdSec Section", _function.PreloadedRebarGroupOutput.Description);
    }

    [Fact]
    public void ParametersDataAccessAreCorrect() {
      Assert.Equal(Access.Item, _function.RebarGroupInput.Access);
      Assert.Equal(Access.Item, _function.PreloadInput.Access);
      Assert.Equal(Access.Item, _function.PreloadedRebarGroupOutput.Access);
    }

    [Fact]
    public void OrganisationHasCorrectValues() {
      Assert.Equal("AdSec", _function.Organisation.Category);
      Assert.Equal("Loads", _function.Organisation.SubCategory.Trim());
    }

    [Fact]
    public void MetaDataHasCorrectValues() {
      Assert.Equal("Create Prestress Load", _function.Metadata.Name);
      Assert.Equal("Prestress", _function.Metadata.NickName);
      Assert.Contains("Create an AdSec Prestress Load", _function.Metadata.Description);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(-50)]
    [InlineData(0)]
    public void ComputeWithDifferentPreloadValuesCreatesCorrectPreload(double preloadValue) {
      _function.PreloadInput.Value = preloadValue;
      _function.Compute();

      var outPreLoad = (IPreForce)(((ILongitudinalGroup)_function.PreloadedRebarGroupOutput.Value.Group).Preload);
      Assert.Equal(preloadValue, outPreLoad.Force.As(_function.ForceUnit), 6);
    }

    [Fact]
    public void ComputeThrowsErrorForInvalidRebarGroup() {
      _function.RebarGroupInput.Value = null;
      _function.Compute();
      Assert.Single(_function.ErrorMessages);
    }

    [Fact]
    public void ComputeThrowsInvalidCastExceptionForInvalidPreloadType() {
      _function.PreloadInput.Value = "InvalidType";
      Assert.Throws<InvalidCastException>(() => _function.Compute());
    }
  }
}
