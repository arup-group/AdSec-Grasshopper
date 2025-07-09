using AdSecCore.Functions;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.StandardMaterials;

using OasysUnits;

namespace AdSecCoreTests.Functions {
  public class EditMaterialFunctionTests {
    private readonly EditMaterialFunction _function;
    private readonly StressStrainCurveFunction _StressStrainfunction;
    public EditMaterialFunctionTests() {
      _function = new EditMaterialFunction();
      _StressStrainfunction = new StressStrainCurveFunction();
    }

    private void SetMaterialAndCode() {
      var inputMaterial = new MaterialDesign() {
        Material = Concrete.IS456.Edition_2000.M10,
        DesignCode = new DesignCode() { IDesignCode = IS456.Edition_2000 },
      };
      _function.MaterialInput.Value = inputMaterial;
      _function.DesignCodeInput.Value = inputMaterial.DesignCode;
    }

    private IStressStrainPoint CreateFailurePoint() {
      var strain = Strain.FromRatio(0.0034);
      var stress = Pressure.FromPascals(10);
      return _StressStrainfunction.FailurePoint.Value = IStressStrainPoint.Create(stress, strain);
    }

    private IStressStrainPoint CreateYieldPoint() {
      var strain = Strain.FromRatio(0.002);
      var stress = Pressure.FromPascals(10);
      return _StressStrainfunction.YieldPoint.Value = IStressStrainPoint.Create(stress, strain); ;
    }

    private IStressStrainPoint CreatePeakPoint() {
      var strain = Strain.FromRatio(0.001);
      var stress = Pressure.FromPascals(10);
      return _StressStrainfunction.PeakPoint.Value = IStressStrainPoint.Create(stress, strain);
    }

    private double CreateInitialModulus() {
      return _StressStrainfunction.InitialModulus.Value = 8;
    }

    private double CreateFailureStrain() {
      return _StressStrainfunction.FailureStrain.Value = 0.0034;
    }

    private void CreateFibSlsCurve() {
      _StressStrainfunction.SelectedCurveType = StressStrainCurveType.FibModelCode;
      CreateInitialModulus();
      CreatePeakPoint();
      CreateFailureStrain();
      _StressStrainfunction.Compute();
      _function.SlsCompressionCurveInput.Value = _StressStrainfunction.OutputCurve.Value;
    }

    private void CreateBiLinearUlsCurve() {
      _StressStrainfunction.SelectedCurveType = StressStrainCurveType.Bilinear;
      CreateFailurePoint();
      CreateYieldPoint();
      _StressStrainfunction.Compute();
      _function.UlsCompressionCurveInput.Value = _StressStrainfunction.OutputCurve.Value;
    }

    [Fact]
    public void MetaDataPropertyShouldBeCorrect() {
      Assert.Equal("Edit Material", _function.Metadata.Name);
      Assert.Equal("EditMat", _function.Metadata.NickName);
      Assert.Equal("Modify AdSec Material", _function.Metadata.Description);
      Assert.False(_function.MaterialInput.Optional);
      Assert.True(_function.DesignCodeInput.Optional);
    }

    [Fact]
    public void ComputeWithNullMaterialShouldHaveError() {
      _function.Compute();
      Assert.Contains("Material input cannot be null", _function.ErrorMessages[0]);
      Assert.Null(_function.MaterialOutput.Value);
    }

    [Fact]
    public void ShouldNotModifyUpstreamObject() {
      SetMaterialAndCode();
      CreateFibSlsCurve();
      CreateBiLinearUlsCurve();
      _function.DesignCodeInput.Value = new DesignCode() { IDesignCode = IRS.Edition_1997 };
      _function.Compute();
      var inputMaterial = _function.MaterialInput.Value;
      var outputMaterial = _function.MaterialOutput.Value;
      Assert.NotNull(outputMaterial);
      Assert.NotEqual(inputMaterial.Material.Serviceability.Compression.FailureStrain, outputMaterial.Material.Serviceability.Compression.FailureStrain);
      Assert.NotEqual(inputMaterial.Material.Strength.Compression.FailureStrain, outputMaterial.Material.Strength.Compression.FailureStrain);
      Assert.False(ReferenceEquals(inputMaterial.DesignCode.IDesignCode, outputMaterial.DesignCode.IDesignCode));
    }

    [Fact]
    public void ComputeWithNewDesignCodeShouldUpdateDesignCode() {
      SetMaterialAndCode();
      _function.DesignCodeInput.Value = new DesignCode() { DesignCodeName = "IRS", IDesignCode = IRS.Edition_1997 }; ;
      _function.Compute();
      Assert.Equal(_function.DesignCodeInput.Value.DesignCodeName, _function.DesignCodeOutput.Value.DesignCodeName);
      Assert.True(ReferenceEquals(_function.DesignCodeInput.Value.IDesignCode, _function.DesignCodeInput.Value.IDesignCode));
    }

    [Fact]
    public void GetAllInputAttributesShouldReturnSevenAttributes() {
      var attributes = _function.GetAllInputAttributes();
      Assert.Equal(7, attributes.Length);
    }

    [Fact]
    public void GetAllOutputAttributesShouldReturnSevenAttributes() {
      var attributes = _function.GetAllOutputAttributes();
      Assert.Equal(7, attributes.Length);
    }

    [Fact]
    public void MaterialInput_Properties_ShouldBeCorrect() {
      Assert.Equal("Material", _function.MaterialInput.Name);
      Assert.Equal("Mat", _function.MaterialInput.NickName);
      Assert.Equal("AdSet Material to Edit or get information from", _function.MaterialInput.Description);
      Assert.Equal(Access.Item, _function.MaterialInput.Access);
      Assert.False(_function.MaterialInput.Optional);
    }

    [Fact]
    public void DesignCodeInput_Properties_ShouldBeCorrect() {
      Assert.Equal("DesignCode", _function.DesignCodeInput.Name);
      Assert.Equal("Code", _function.DesignCodeInput.NickName);
      Assert.Equal("Overwrite the Material's DesignCode", _function.DesignCodeInput.Description);
      Assert.Equal(Access.Item, _function.DesignCodeInput.Access);
      Assert.True(_function.DesignCodeInput.Optional);
    }

    [Theory]
    [InlineData("ULS Comp. Crv", "U_C", "ULS Stress Strain Curve for Compression")]
    [InlineData("ULS Tens. Crv", "U_T", "ULS Stress Strain Curve for Tension")]
    [InlineData("SLS Comp. Crv", "S_C", "SLS Stress Strain Curve for Compression")]
    [InlineData("SLS Tens. Crv", "S_T", "SLS Stress Strain Curve for Tension")]
    public void StressStrainCurveInputs_Properties_ShouldBeCorrect(string name, string nickname, string description) {
      var parameter = GetParameterByName(name);
      Assert.Equal(name, parameter.Name);
      Assert.Equal(nickname, parameter.NickName);
      Assert.Equal(description, parameter.Description);
      Assert.Equal(Access.Item, parameter.Access);
      Assert.True(parameter.Optional);
    }

    [Fact]
    public void CrackCalcParamsInput_Properties_ShouldBeCorrect() {
      Assert.Equal("Crack Calc Params", _function.CrackCalcParamsInput.Name);
      Assert.Equal("CCP", _function.CrackCalcParamsInput.NickName);
      Assert.Equal("Overwrite the Material's ConcreteCrackCalculationParameters", _function.CrackCalcParamsInput.Description);
      Assert.Equal(Access.Item, _function.CrackCalcParamsInput.Access);
      Assert.True(_function.CrackCalcParamsInput.Optional);
    }

    [Fact]
    public void MaterialOutput_Properties_ShouldBeCorrect() {
      Assert.Equal("Material", _function.MaterialOutput.Name);
      Assert.Equal("Mat", _function.MaterialOutput.NickName);
      Assert.Equal("Modified AdSec Material", _function.MaterialOutput.Description);
      Assert.Equal(Access.Item, _function.MaterialOutput.Access);
    }

    [Theory]
    [InlineData("ULS Comp. Crv", "U_C", "ULS Stress Strain Curve for Compression")]
    [InlineData("ULS Tens. Crv", "U_T", "ULS Stress Strain Curve for Tension")]
    [InlineData("SLS Comp. Crv", "S_C", "SLS Stress Strain Curve for Compression")]
    [InlineData("SLS Tens. Crv", "S_T", "SLS Stress Strain Curve for Tension")]
    public void StressStrainCurveOutputs_Properties_ShouldBeCorrect(string name, string nickname, string description) {
      var parameter = GetOutputParameterByName(name);
      Assert.Equal(name, parameter.Name);
      Assert.Equal(nickname, parameter.NickName);
      Assert.Equal(description, parameter.Description);
      Assert.Equal(Access.Item, parameter.Access);
    }

    private StressStrainCurveParameter GetParameterByName(string name) {
      return name switch {
        "ULS Comp. Crv" => _function.UlsCompressionCurveInput,
        "ULS Tens. Crv" => _function.UlsTensionCurveInput,
        "SLS Comp. Crv" => _function.SlsCompressionCurveInput,
        "SLS Tens. Crv" => _function.SlsTensionCurveInput,
        _ => throw new ArgumentException($"Unknown parameter name: {name}")
      };
    }

    private StressStrainCurveParameter GetOutputParameterByName(string name) {
      return name switch {
        "ULS Comp. Crv" => _function.UlsCompressionCurveOutput,
        "ULS Tens. Crv" => _function.UlsTensionCurveOutput,
        "SLS Comp. Crv" => _function.SlsCompressionCurveOutput,
        "SLS Tens. Crv" => _function.SlsTensionCurveOutput,
        _ => throw new ArgumentException($"Unknown parameter name: {name}")
      };
    }
  }
}
