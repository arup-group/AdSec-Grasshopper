using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;

namespace AdSecCoreTests.Functions {
  public class CreatePreLoadFunctionTests {
    private readonly CreatePreLoadFunction _function;

    public CreatePreLoadFunctionTests() {
      _function = new CreatePreLoadFunction();
      _function.PreLoadType = (PreLoadType)100;
      _function.RebarGroupInput.Value = new AdSecRebarGroup(new BuilderTemplateGroup().Build());
      _function.PreloadInput.Value = 10;
    }

    [Fact]
    public void ComputeWithValidForceCreatesPreload() {
      _function.PreLoadType = PreLoadType.Force;
      _function.Compute();
      var outPreLoad = (IPreForce)(((ILongitudinalGroup)_function.PreloadedRebarGroupOutput.Value.Group).Preload);
      double outputForce = outPreLoad.Force.As(_function.ForceUnit);
      Assert.Equal(10, outputForce);
    }

    [Fact]
    public void ComputeWithValidStrainCreatesPreload() {
      _function.PreLoadType = PreLoadType.Strain;
      _function.Compute();
      var outPreLoad = (IPreStrain)(((ILongitudinalGroup)_function.PreloadedRebarGroupOutput.Value.Group).Preload);
      double outputForce = outPreLoad.Strain.As(_function.MaterialStrainUnit);
      Assert.Equal(10, outputForce);
    }

    [Fact]
    public void ComputeWithValidPreStressCreatesPreload() {
      _function.PreLoadType = PreLoadType.Stress;
      _function.Compute();
      var outPreLoad = (IPreStress)(((ILongitudinalGroup)_function.PreloadedRebarGroupOutput.Value.Group).Preload);
      double outputForce = outPreLoad.Stress.As(_function.StressUnitResult);
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
      _function.PreLoadType = PreLoadType.Force;
      _function.PreloadInput.Value = preloadValue;
      _function.Compute();

      var outPreLoad = (IPreForce)(((ILongitudinalGroup)_function.PreloadedRebarGroupOutput.Value.Group).Preload);
      Assert.Equal(preloadValue, outPreLoad.Force.As(_function.ForceUnit), 6);
    }

    [Theory]
    [InlineData("Force")]
    [InlineData("Stress")]
    [InlineData("Strain")]
    public void CanUpdateParameterNameAndNickName(string forceType) {
      _function.PreLoadType = (PreLoadType)Enum.Parse(typeof(PreLoadType), forceType, true);
      Assert.Contains(forceType, _function.PreloadInput.Name);
    }

    [Fact]
    public void ComputeReportErrorForInvalidRebarGroup() {
      _function.RebarGroupInput.Value = null;
      _function.Compute();
      Assert.Single(_function.ErrorMessages);
    }

    [Fact]
    public void ComputeReportErrorForInvalidPreloadType() {
      _function.PreloadInput.Value = "InvalidType";
      _function.Compute();
      Assert.Single(_function.ErrorMessages);
    }

    [Fact]
    public void ComputeReportErrorMessageWhenRebarTemplateIsNull() {
      _function.RebarGroupInput.Value = new AdSecRebarGroup();
      _function.Compute();
      Assert.Single(_function.ErrorMessages);
      Assert.Contains("Invalid RebarGroup input", _function.ErrorMessages[0]);
    }
  }
}
