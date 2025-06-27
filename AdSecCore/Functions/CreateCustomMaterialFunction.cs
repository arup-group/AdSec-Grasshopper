using System;

using AdSecCore.Parameters;

using AdSecGHCore.Constants;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {

  public class CreateCustomMaterialFunction : Function, IVariableInput, IDropdownOptions {

    public MaterialType CurrentMaterialType { get; set; }
    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Custom Material",
      NickName = "CustomMaterial",
      Description = "Create a custom AdSec Material",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat1()
    };

    public DesignCodeParameter DesignCode { get; set; }
      = Default.DesignCode(optional: true, description: "[Optional] Set the Material's DesignCode");

    public StressStrainCurveParameter UlsCompressionCurve { get; set; } = new StressStrainCurveParameter() {
      Name = "ULS Comp. Crv",
      NickName = "U_C",
      Description = "ULS Stress Strain Curve for Compression",
    };

    public StressStrainCurveParameter UlsTensionCurve { get; set; } = new StressStrainCurveParameter() {
      Name = "ULS Tens. Crv",
      NickName = "U_T",
      Description = "ULS Stress Strain Curve for Tension",
    };

    public StressStrainCurveParameter SlsCompressionCurve { get; set; } = new StressStrainCurveParameter() {
      Name = "SLS Comp. Crv",
      NickName = "S_C",
      Description = "SLS Stress Strain Curve for Compression",
    };

    public StressStrainCurveParameter SlsTensionCurve { get; set; } = new StressStrainCurveParameter() {
      Name = "SLS Tens. Crv",
      NickName = "S_T",
      Description = "SLS Stress Strain Curve for Tension",
    };
    public CrackCalcParameter CrackCalcParams { get; set; } = new CrackCalcParameter() {
      Name = "Crack Calc Params",
      NickName = "CCP",
      Description = "[Optional] Material's Crack Calculation Parameters",
      Optional = true,
    };
    public MaterialParameter Material { get; set; } = Default.Material(description: "Custom AdSec Material");

    public override Attribute[] GetAllInputAttributes() {
      if (CurrentMaterialType == MaterialType.Concrete) {
        return new Attribute[] {
          DesignCode,
          UlsCompressionCurve,
          UlsTensionCurve,
          SlsCompressionCurve,
          SlsTensionCurve,
          CrackCalcParams,
        };
      }

      return new Attribute[] {
        DesignCode,
        UlsCompressionCurve,
        UlsTensionCurve,
        SlsCompressionCurve,
        SlsTensionCurve,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { Material };
    }

    public override void Compute() {
      DoubleComparer comparer = new DoubleComparer();
      var stressStrainCurve = UlsTensionCurve.Value;
      if (comparer.Equals(0, stressStrainCurve.IStressStrainCurve.FailureStrain.Value)) {
        WarningMessages.Add($"ULS Stress Strain Curve for Tension has zero failure strain.{Environment.NewLine}The curve has been changed to a simulate a material with no tension capacity (ε = 1, σ = 0)");
        stressStrainCurve.IStressStrainCurve = ILinearStressStrainCurve.Create(IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal),
          new Strain(1, StrainUnit.Ratio)));
      }

      var strength = ITensionCompressionCurve.Create(stressStrainCurve.IStressStrainCurve, UlsCompressionCurve.Value.IStressStrainCurve);
      var serviceability = ITensionCompressionCurve.Create(SlsTensionCurve.Value.IStressStrainCurve, SlsCompressionCurve.Value.IStressStrainCurve);

      Material.Value = new MaterialDesign() {
        DesignCode = DesignCode.Value,
        GradeName = DesignCode.Value.DesignCodeName,
      };

      switch (CurrentMaterialType) {
        case MaterialType.Concrete:
          if (CrackCalcParams.Value == null) {
            Material.Value.Material = IConcrete.Create(strength, serviceability);
            break;
          }

          Material.Value.Material = IConcrete.Create(strength, serviceability, CrackCalcParams.Value);
          break;
        case MaterialType.Tendon:
        case MaterialType.Rebar:
          Material.Value.Material = IReinforcement.Create(strength, serviceability);
          break;
        case MaterialType.Steel:
          Material.Value.Material = ISteel.Create(strength, serviceability);
          break;
        case MaterialType.FRP:
          Material.Value.Material = IFrp.Create(strength, serviceability);
          break;
      }
    }

    public event Action OnVariableInputChanged;

    public IOptions[] Options() {
      return new IOptions[] {
        new EnumOptions() {
          Description = "Material Type",
          EnumType = typeof(MaterialType),
        }
      };
    }

    public void SetMaterialType(MaterialType rebar) {
      if (CurrentMaterialType != rebar) {
        CurrentMaterialType = rebar;
        OnVariableInputChanged?.Invoke();
      }
    }
  }

  public enum MaterialType {
    Concrete,
    Rebar,
    Tendon,
    Steel,
    FRP,
  }

}
