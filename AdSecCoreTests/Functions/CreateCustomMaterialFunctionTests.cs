using AdSecCore.Functions;

using AdSecGHCore.Constants;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.StandardMaterials;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {

  public class CreateCustomMaterialFunctionTests {
    private readonly CreateCustomMaterialFunction _function;

    public CreateCustomMaterialFunctionTests() {
      _function = new CreateCustomMaterialFunction();
    }

    [Fact]
    public void ShouldHaveAName() {
      Assert.Equal("Custom Material", _function.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveANickName() {
      Assert.Equal("CustomMaterial", _function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveADescription() {
      Assert.Equal("Create a custom AdSec Material", _function.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveACategory() {
      Assert.Equal(CategoryName.Name(), _function.Organisation.Category);
    }

    [Fact]
    public void ShouldHaveNoSubCategory() {
      Assert.Equal(SubCategoryName.Cat1(), _function.Organisation.SubCategory);
    }

    [Fact]
    public void ShouldHaveSixInputs() {
      Assert.Equal(6, _function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveDesignCode() {
      Assert.Equal("DesignCode", _function.DesignCode.Name);
    }

    [Fact]
    public void ShouldExplainDesignCodeIsOptional() {
      Assert.Equal("[Optional] Set the Material's DesignCode", _function.DesignCode.Description);
    }

    [Fact]
    public void ShouldHaveDesignCodeOptional() {
      Assert.True(_function.DesignCode.Optional);
    }

    [Fact]
    public void ShouldHaveUlsCompressionCrv() {
      Assert.Equal("ULS Comp. Crv", _function.UlsCompressionCurve.Name);
      Assert.Equal("U_C", _function.UlsCompressionCurve.NickName);
      Assert.Equal("ULS Stress Strain Curve for Compression", _function.UlsCompressionCurve.Description);
    }

    [Fact]
    public void ShouldHaveUlsTensionCrv() {
      Assert.Equal("ULS Tens. Crv", _function.UlsTensionCurve.Name);
      Assert.Equal("U_T", _function.UlsTensionCurve.NickName);
      Assert.Equal("ULS Stress Strain Curve for Tension", _function.UlsTensionCurve.Description);
    }

    [Fact]
    public void ShouldHaveSlsCompressionCrv() {
      Assert.Equal("SLS Comp. Crv", _function.SlsCompressionCurve.Name);
      Assert.Equal("S_C", _function.SlsCompressionCurve.NickName);
      Assert.Equal("SLS Stress Strain Curve for Compression", _function.SlsCompressionCurve.Description);
    }

    [Fact]
    public void ShouldHaveSlsTensionCrv() {
      Assert.Equal("SLS Tens. Crv", _function.SlsTensionCurve.Name);
      Assert.Equal("S_T", _function.SlsTensionCurve.NickName);
      Assert.Equal("SLS Stress Strain Curve for Tension", _function.SlsTensionCurve.Description);
    }

    [Fact]
    public void ShouldHaveOptionalCrackCalcParams() {
      Assert.Equal("Crack Calc Params", _function.CrackCalcParams.Name);
      Assert.Equal("CCP", _function.CrackCalcParams.NickName);
      Assert.Equal("[Optional] Material's Crack Calculation Parameters", _function.CrackCalcParams.Description);
      Assert.True(_function.CrackCalcParams.Optional);
    }

    [Fact]
    public void ShouldHaveInputsAtFollowingOrder() {
      var inputs = _function.GetAllInputAttributes();
      Assert.Equal(_function.DesignCode, inputs[0]);
      Assert.Equal(_function.UlsCompressionCurve, inputs[1]);
      Assert.Equal(_function.UlsTensionCurve, inputs[2]);
      Assert.Equal(_function.SlsCompressionCurve, inputs[3]);
      Assert.Equal(_function.SlsTensionCurve, inputs[4]);
      Assert.Equal(_function.CrackCalcParams, inputs[5]);
    }

    [Fact]
    public void ShouldHaveMaterialOutput() {
      Assert.Equal("Material", _function.Material.Name);
    }

    [Fact]
    public void ShouldDescribeThatItIsCustomTheMaterial() {
      Assert.Equal("Custom AdSec Material", _function.Material.Description);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(_function.GetAllOutputAttributes());
    }

    [Fact]
    public void ShouldHaveOutputAtFirstPosition() {
      var outputs = _function.GetAllOutputAttributes();
      Assert.Equal(_function.Material, outputs[0]);
    }

    [Fact]
    public void ShouldHaveASingleDropdown() {
      Assert.Single(_function.Options());
    }

    [Fact]
    public void ShouldHaveDropdownOfTypeEnum() {
      var enumOption = _function.Options()[0] as EnumOptions;
      Assert.IsType<EnumOptions>(enumOption);
      Assert.Equal("Material Type", enumOption.Description);
      Assert.Equal(typeof(MaterialType), enumOption.EnumType);
    }

    [Theory]
    [InlineData(MaterialType.Concrete, 6)]
    [InlineData(MaterialType.Steel, 5)]
    [InlineData(MaterialType.Rebar, 5)]
    [InlineData(MaterialType.Tendon, 5)]
    [InlineData(MaterialType.FRP, 5)]
    public void ShouldHaveCrackCalcOnlyForConcrete(MaterialType type, int expectedInputCount) {
      _function.SetMaterialType(type);
      Assert.Equal(expectedInputCount, _function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldTriggerEventWhenMaterialChanged() {
      bool eventTriggered = false;
      _function.OnVariableInputChanged += () => { eventTriggered = true; };
      _function.SetMaterialType(MaterialType.Steel);
      Assert.True(eventTriggered);
    }

    [Fact]
    public void ShouldOnlyTriggerOnceForTheSameType() {
      int timesTriggered = 0;
      _function.OnVariableInputChanged += () => { timesTriggered++; };
      _function.SetMaterialType(MaterialType.Steel);
      _function.SetMaterialType(MaterialType.Steel);
      Assert.Equal(1, timesTriggered);
    }
  }

  public class CustomMaterialFunctionComputeTests {
    private readonly CreateCustomMaterialFunction _function;

    public CustomMaterialFunctionComputeTests() {
      _function = new CreateCustomMaterialFunction();
      _function.DesignCode.Value = new DesignCode() { IDesignCode = IS456.Edition_2000 };
      var linearStressStrainCurve
        = ILinearStressStrainCurve.Create(IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal),
          new Strain(1, StrainUnit.Ratio)));
      _function.UlsCompressionCurve.Value = linearStressStrainCurve;
      _function.UlsTensionCurve.Value = linearStressStrainCurve;
      _function.SlsCompressionCurve.Value = linearStressStrainCurve;
      _function.SlsTensionCurve.Value = linearStressStrainCurve;

      var pressure = new Pressure(-0.5, PressureUnit.Bar);
      var pressure2 = new Pressure(1, PressureUnit.Bar);
      _function.CrackCalcParams.Value = IConcreteCrackCalculationParameters.Create(pressure2, pressure, pressure2);
    }

    [Fact]
    public void ShouldCreateACustomSteel() {
      _function.SetMaterialType(MaterialType.Steel);
      _function.Compute();
      Assert.IsAssignableFrom<ISteel>(_function.Material.Value.Material);
    }

    [Fact]
    public void ShouldCreateACustomConcrete() {
      _function.SetMaterialType(MaterialType.Concrete);
      _function.Compute();
      Assert.IsAssignableFrom<IConcrete>(_function.Material.Value.Material);
    }

    [Fact]
    public void ShouldCreateACustomConcreteWithNullCrack() {
      _function.CrackCalcParams.Value = null;
      _function.SetMaterialType(MaterialType.Concrete);
      _function.Compute();
      Assert.IsAssignableFrom<IConcrete>(_function.Material.Value.Material);
    }

    [Fact]
    public void ShouldCreateACustomRebar() {
      _function.CrackCalcParams.Value = null;
      _function.SetMaterialType(MaterialType.Rebar);
      _function.Compute();
      Assert.NotNull(_function.Material.Value);
      Assert.IsAssignableFrom<IReinforcement>(_function.Material.Value.Material);
    }

    [Fact]
    public void ShouldCreateACustomTendon() {
      _function.CrackCalcParams.Value = null;
      _function.SetMaterialType(MaterialType.Tendon);
      _function.Compute();
      Assert.IsAssignableFrom<IReinforcement>(_function.Material.Value.Material);
    }

    [Fact]
    public void ShouldCreateACustomFRP() {
      _function.CrackCalcParams.Value = null;
      _function.SetMaterialType(MaterialType.FRP);
      _function.Compute();
      Assert.IsAssignableFrom<IFrp>(_function.Material.Value.Material);
    }

    [Fact]
    public void ShouldAddWarningWhenFailureStrainIsZero() {
      var linearStressStrainCurve
        = ILinearStressStrainCurve.Create(IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal),
          new Strain(0, StrainUnit.Ratio)));
      _function.UlsTensionCurve.Value = linearStressStrainCurve;
      _function.Compute();
      Assert.Single(_function.WarningMessages);
      Assert.Equal($"ULS Stress Strain Curve for Tension has zero failure strain.{Environment.NewLine}The curve has been changed to a simulate a material with no tension capacity (ε = 1, σ = 0)", _function.WarningMessages[0]);
    }

    [Fact]
    public void ShouldReplaceInvalidCurveWithAValidOne() {
      var linearStressStrainCurve
        = ILinearStressStrainCurve.Create(IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal),
          new Strain(0, StrainUnit.Ratio)));
      _function.UlsTensionCurve.Value = linearStressStrainCurve;
      _function.Compute();
      var valueMaterial = _function.Material.Value.Material;
      Assert.Equal(1, valueMaterial.Strength.Tension.FailureStrain.As(StrainUnit.Ratio));
    }
  }
}
