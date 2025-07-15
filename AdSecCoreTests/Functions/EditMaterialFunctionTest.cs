using AdSecCore.Functions;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.StandardMaterials;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class EditMaterialFunctionTests {
    private readonly EditMaterialFunction _function;
    private readonly StressStrainCurveFunction _stressStrainfunction;
    public EditMaterialFunctionTests() {
      _function = new EditMaterialFunction();
      _stressStrainfunction = new StressStrainCurveFunction();
    }

    private void SetConcreMaterialAndCode(bool withoutCrackParameter = false) {
      var inputMaterial = new MaterialDesign() {
        Material = Concrete.IS456.Edition_2000.M10,
        DesignCode = new DesignCode() { IDesignCode = IS456.Edition_2000 },
      };
      if (withoutCrackParameter) {
        var concreteMaterial = inputMaterial.Material as IConcrete;
        if (concreteMaterial != null) {
          var strength = ITensionCompressionCurve.Create(concreteMaterial.Strength.Tension, concreteMaterial.Strength.Compression);
          var serviceability = ITensionCompressionCurve.Create(concreteMaterial.Serviceability.Tension, concreteMaterial.Serviceability.Compression);
          inputMaterial.Material = IConcrete.Create(strength, serviceability);
        }
      } else {
        _function.CrackCalcParamsInput.Value = IConcreteCrackCalculationParameters.Create(Pressure.FromMegapascals(10), Pressure.FromPascals(-10), Pressure.FromPascals(4));
      }
      _function.MaterialInput.Value = inputMaterial;
      _function.DesignCodeInput.Value = inputMaterial.DesignCode;
    }

    private void SetReinforcementMaterialAndCode() {
      var inputMaterial = new MaterialDesign() {
        Material = Reinforcement.Steel.IS456.Edition_2000.S500,
        DesignCode = new DesignCode() { IDesignCode = IS456.Edition_2000 },
      };
      _function.MaterialInput.Value = inputMaterial;
      _function.DesignCodeInput.Value = inputMaterial.DesignCode;
    }

    private void SetFrpMaterialAndCode() {
      var inputMaterial = new MaterialDesign() {
        Material = FRP.SikaCarboDur.M,
        DesignCode = new DesignCode() { IDesignCode = IS456.Edition_2000 },
      };
      _function.MaterialInput.Value = inputMaterial;
      _function.DesignCodeInput.Value = inputMaterial.DesignCode;
    }

    private void SetSteelMaterialAndCode() {
      var inputMaterial = new MaterialDesign() {
        Material = Steel.ASTM.A913_70,
        DesignCode = new DesignCode() { IDesignCode = IS456.Edition_2000 },
      };
      _function.MaterialInput.Value = inputMaterial;
      _function.DesignCodeInput.Value = inputMaterial.DesignCode;
    }

    private StressStrainCurve CreateExplicitCompressionCurve(bool uls = true) {
      var explicitCurve = IExplicitStressStrainCurve.Create();
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.Zero, Strain.Zero));
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.FromPascals(10), Strain.FromRatio(uls ? 0.0018 : 0.0021)));
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.FromPascals(10), Strain.FromRatio(uls ? 0.0036 : 0.0042)));
      return new StressStrainCurve() { IStressStrainCurve = explicitCurve, IsCompression = true };
    }

    private StressStrainCurve CreateExplicitTensionCurve(bool uls = true) {
      var explicitCurve = IExplicitStressStrainCurve.Create();
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.Zero, Strain.Zero));
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.FromPascals(12), Strain.FromRatio(uls ? 0.002 : 0.0022)));
      explicitCurve.Points.Add(IStressStrainPoint.Create(Pressure.FromPascals(24), Strain.FromRatio(uls ? 0.004 : 0.0044)));
      return new StressStrainCurve() { IStressStrainCurve = explicitCurve, IsCompression = false };
    }

    private IStressStrainPoint CreateFailurePoint() {
      var strain = Strain.FromRatio(0.0034);
      var stress = Pressure.FromPascals(10);
      return _stressStrainfunction.FailurePoint.Value = IStressStrainPoint.Create(stress, strain);
    }

    private IStressStrainPoint CreateYieldPoint() {
      var strain = Strain.FromRatio(0.002);
      var stress = Pressure.FromPascals(10);
      return _stressStrainfunction.YieldPoint.Value = IStressStrainPoint.Create(stress, strain);
    }

    private IStressStrainPoint CreatePeakPoint() {
      var strain = Strain.FromRatio(0.001);
      var stress = Pressure.FromPascals(10);
      return _stressStrainfunction.PeakPoint.Value = IStressStrainPoint.Create(stress, strain);
    }

    private double CreateInitialModulus() {
      return _stressStrainfunction.InitialModulus.Value = 8;
    }

    private double CreateFailureStrain() {
      return _stressStrainfunction.FailureStrain.Value = 0.0034;
    }

    private void CreateFibSlsCurve() {
      _stressStrainfunction.SelectedCurveType = StressStrainCurveType.FibModelCode;
      CreateInitialModulus();
      CreatePeakPoint();
      CreateFailureStrain();
      _stressStrainfunction.Compute();
      _function.SlsCompressionCurveInput.Value = _stressStrainfunction.OutputCurve.Value;
    }

    private void CreateBiLinearUlsCurve() {
      _stressStrainfunction.SelectedCurveType = StressStrainCurveType.Bilinear;
      CreateFailurePoint();
      CreateYieldPoint();
      _stressStrainfunction.Compute();
      _function.UlsCompressionCurveInput.Value = _stressStrainfunction.OutputCurve.Value;
    }

    private void AssignCurve() {
      _function.UlsCompressionCurveInput.Value = CreateExplicitCompressionCurve();
      _function.SlsCompressionCurveInput.Value = CreateExplicitCompressionCurve(true);
      _function.UlsTensionCurveInput.Value = CreateExplicitTensionCurve();
      _function.SlsTensionCurveInput.Value = CreateExplicitTensionCurve(true);
      _function.Compute();
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
    public void ShouldEditConcreteMaterial() {
      SetConcreMaterialAndCode();
      AssignCurve();
      AssertMaterialProperties(true);
    }

    [Fact]
    public void ShouldEditConcreteMaterialWithoutCrackParameter() {
      bool withoutCrackParameter = true;
      SetConcreMaterialAndCode(withoutCrackParameter);
      AssignCurve();
      AssertMaterialProperties(!withoutCrackParameter);
    }

    [Fact]
    public void ShouldEditReinforcementMaterial() {
      SetReinforcementMaterialAndCode();
      AssignCurve();
      AssertMaterialProperties();
    }

    [Fact]
    public void ShouldEditFrpMaterial() {
      SetFrpMaterialAndCode();
      AssignCurve();
      AssertMaterialProperties();
    }

    [Fact]
    public void ShouldEditSteelMaterial() {
      SetSteelMaterialAndCode();
      AssignCurve();
      AssertMaterialProperties();
    }

    private void AssertMaterialProperties(bool crackParameter = false) {
      var outputMaterial = _function.MaterialOutput.Value;
      var compressionCurveUls = CreateExplicitCompressionCurve();
      var compressionCurveSls = CreateExplicitCompressionCurve(true);
      var tensionCurveUls = CreateExplicitTensionCurve();
      var tensionCurveSls = CreateExplicitTensionCurve(true);
      Assert.NotNull(outputMaterial);
      Assert.Equal(compressionCurveSls.IStressStrainCurve.FailureStrain.ToUnit(StrainUnit.Ratio), outputMaterial.Material.Serviceability.Compression.FailureStrain.ToUnit(StrainUnit.Ratio));
      Assert.Equal(tensionCurveSls.IStressStrainCurve.FailureStrain.ToUnit(StrainUnit.Ratio), outputMaterial.Material.Serviceability.Tension.FailureStrain.ToUnit(StrainUnit.Ratio));
      Assert.Equal(compressionCurveUls.IStressStrainCurve.FailureStrain.ToUnit(StrainUnit.Ratio), outputMaterial.Material.Strength.Compression.FailureStrain.ToUnit(StrainUnit.Ratio));
      Assert.Equal(tensionCurveUls.IStressStrainCurve.FailureStrain.ToUnit(StrainUnit.Ratio), outputMaterial.Material.Strength.Tension.FailureStrain.ToUnit(StrainUnit.Ratio));
      if (crackParameter) {
        var concreteMaterial = outputMaterial.Material as IConcrete;
        Assert.NotNull(concreteMaterial);
        Assert.NotNull(concreteMaterial.ConcreteCrackCalculationParameters);
        Assert.Equal(Pressure.FromMegapascals(10), concreteMaterial.ConcreteCrackCalculationParameters.ElasticModulus.ToUnit(PressureUnit.Megapascal));
        Assert.Equal(Pressure.FromPascals(-10), concreteMaterial.ConcreteCrackCalculationParameters.CharacteristicCompressiveStrength.ToUnit(PressureUnit.Pascal));
        Assert.Equal(Pressure.FromPascals(4), concreteMaterial.ConcreteCrackCalculationParameters.CharacteristicTensileStrength.ToUnit(PressureUnit.Pascal));
      }
    }

    [Fact]
    public void ShouldNotModifyUpstreamObject() {
      SetConcreMaterialAndCode();
      CreateFibSlsCurve();
      CreateBiLinearUlsCurve();
      _function.DesignCodeInput.Value = new DesignCode() { IDesignCode = IRS.Edition_1997 };
      _function.Compute();
      var inputMaterial = _function.MaterialInput.Value;
      var outputMaterial = _function.MaterialOutput.Value;
      Assert.NotNull(outputMaterial);
      Assert.NotEqual(inputMaterial.Material.Serviceability.Compression.FailureStrain.ToUnit(StrainUnit.Ratio), outputMaterial.Material.Serviceability.Compression.FailureStrain.ToUnit(StrainUnit.Ratio));
      Assert.NotEqual(inputMaterial.Material.Strength.Compression.FailureStrain.ToUnit(StrainUnit.Ratio), outputMaterial.Material.Strength.Compression.FailureStrain.ToUnit(StrainUnit.Ratio));
      Assert.False(ReferenceEquals(inputMaterial.DesignCode.IDesignCode, outputMaterial.DesignCode.IDesignCode));
    }

    [Fact]
    public void ComputeWithNewDesignCodeShouldUpdateDesignCode() {
      SetConcreMaterialAndCode();
      _function.DesignCodeInput.Value = new DesignCode() { DesignCodeName = "IRS", IDesignCode = IRS.Edition_1997 };
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
    public void MaterialInputPropertiesShouldBeCorrect() {
      Assert.Equal("Material", _function.MaterialInput.Name);
      Assert.Equal("Mat", _function.MaterialInput.NickName);
      Assert.Equal("AdSec Material to Edit or get information from", _function.MaterialInput.Description);
      Assert.Equal(Access.Item, _function.MaterialInput.Access);
      Assert.False(_function.MaterialInput.Optional);
    }

    [Fact]
    public void DesignCodeInputPropertiesShouldBeCorrect() {
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
    public void StressStrainCurveInputsPropertiesShouldBeCorrect(string name, string nickname, string description) {
      var parameter = GetParameterByName(name);
      Assert.Equal(name, parameter.Name);
      Assert.Equal(nickname, parameter.NickName);
      Assert.Equal(description, parameter.Description);
      Assert.Equal(Access.Item, parameter.Access);
      Assert.True(parameter.Optional);
    }

    [Fact]
    public void CrackCalcParamsInputPropertiesShouldBeCorrect() {
      Assert.Equal("Crack Calc Params", _function.CrackCalcParamsInput.Name);
      Assert.Equal("CCP", _function.CrackCalcParamsInput.NickName);
      Assert.Equal("Overwrite the Material's ConcreteCrackCalculationParameters", _function.CrackCalcParamsInput.Description);
      Assert.Equal(Access.Item, _function.CrackCalcParamsInput.Access);
      Assert.True(_function.CrackCalcParamsInput.Optional);
    }

    [Fact]
    public void MaterialOutputPropertiesShouldBeCorrect() {
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
    public void StressStrainCurveOutputsPropertiesShouldBeCorrect(string name, string nickname, string description) {
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
