using AdSecGHCore.Constants;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;

namespace AdSecCore.Functions {
  public class EditMaterialFunction : Function {
    // Input Parameters
    public MaterialParameter MaterialInput { get; set; } = new MaterialParameter {
      Name = "Material",
      NickName = "Mat",
      Description = "AdSet Material to Edit or get information from",
      Access = Access.Item,
      Optional = false
    };

    public DesignCodeParameter DesignCodeInput { get; set; } = new DesignCodeParameter {
      Name = "DesignCode",
      NickName = "Code",
      Description = "Overwrite the Material's DesignCode",
      Access = Access.Item,
      Optional = true
    };

    public StressStrainCurveParameter UlsCompressionCurveInput { get; set; } = new StressStrainCurveParameter {
      Name = "ULS Comp. Crv",
      NickName = "U_C",
      Description = "ULS Stress Strain Curve for Compression",
      Access = Access.Item,
      Optional = true
    };

    public StressStrainCurveParameter UlsTensionCurveInput { get; set; } = new StressStrainCurveParameter {
      Name = "ULS Tens. Crv",
      NickName = "U_T",
      Description = "ULS Stress Strain Curve for Tension",
      Access = Access.Item,
      Optional = true
    };

    public StressStrainCurveParameter SlsCompressionCurveInput { get; set; } = new StressStrainCurveParameter {
      Name = "SLS Comp. Crv",
      NickName = "S_C",
      Description = "SLS Stress Strain Curve for Compression",
      Access = Access.Item,
      Optional = true
    };

    public StressStrainCurveParameter SlsTensionCurveInput { get; set; } = new StressStrainCurveParameter {
      Name = "SLS Tens. Crv",
      NickName = "S_T",
      Description = "SLS Stress Strain Curve for Tension",
      Access = Access.Item,
      Optional = true
    };

    public CrackCalcParameter CrackCalcParamsInput { get; set; } = new CrackCalcParameter {
      Name = "Crack Calc Params",
      NickName = "CCP",
      Description = "Overwrite the Material's ConcreteCrackCalculationParameters",
      Access = Access.Item,
      Optional = true
    };

    // Output Parameters
    public MaterialParameter MaterialOutput { get; set; } = new MaterialParameter {
      Name = "Material",
      NickName = "Mat",
      Description = "Modified AdSec Material",
      Access = Access.Item
    };

    public DesignCodeParameter DesignCodeOutput { get; set; } = new DesignCodeParameter {
      Name = "DesignCode",
      NickName = "Code",
      Description = "DesignCode",
      Access = Access.Item
    };

    public StressStrainCurveParameter UlsCompressionCurveOutput { get; set; } = new StressStrainCurveParameter {
      Name = "ULS Comp. Crv",
      NickName = "U_C",
      Description = "ULS Stress Strain Curve for Compression",
      Access = Access.Item
    };

    public StressStrainCurveParameter UlsTensionCurveOutput { get; set; } = new StressStrainCurveParameter {
      Name = "ULS Tens. Crv",
      NickName = "U_T",
      Description = "ULS Stress Strain Curve for Tension",
      Access = Access.Item
    };

    public StressStrainCurveParameter SlsCompressionCurveOutput { get; set; } = new StressStrainCurveParameter {
      Name = "SLS Comp. Crv",
      NickName = "S_C",
      Description = "SLS Stress Strain Curve for Compression",
      Access = Access.Item
    };

    public StressStrainCurveParameter SlsTensionCurveOutput { get; set; } = new StressStrainCurveParameter {
      Name = "SLS Tens. Crv",
      NickName = "S_T",
      Description = "SLS Stress Strain Curve for Tension",
      Access = Access.Item
    };

    public CrackCalcParameter CrackCalcParamsOutput { get; set; } = new CrackCalcParameter {
      Name = "Crack Calc Params",
      NickName = "CCP",
      Description = "ConcreteCrackCalculationParameters",
      Access = Access.Item
    };

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Edit Material",
      NickName = "EditMat",
      Description = "Modify AdSec Material",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat1(),
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
            MaterialInput,
            DesignCodeInput,
            UlsCompressionCurveInput,
            UlsTensionCurveInput,
            SlsCompressionCurveInput,
            SlsTensionCurveInput,
            CrackCalcParamsInput
        };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
            MaterialOutput,
            DesignCodeOutput,
            UlsCompressionCurveOutput,
            UlsTensionCurveOutput,
            SlsCompressionCurveOutput,
            SlsTensionCurveOutput,
            CrackCalcParamsOutput
        };
    }

    private MaterialDesign GetDuplicateMaterial(MaterialDesign material) {
      var duplicateMaterial = new MaterialDesign();
      duplicateMaterial.GradeName = material.GradeName;
      duplicateMaterial.DesignCode = material.DesignCode;

      //ULS Curve
      var ulsCompressionCurve = material.Material.Strength.Compression;
      if (UlsCompressionCurveInput.Value != null) {
        ulsCompressionCurve = UlsCompressionCurveInput.Value.IStressStrainCurve;
      }

      var ulsTensionCurve = material.Material.Strength.Tension;
      if (UlsTensionCurveInput.Value != null) {
        ulsTensionCurve = UlsTensionCurveInput.Value.IStressStrainCurve;
      }

      //SLS Curve
      var slsCompressionCurve = material.Material.Serviceability.Compression;
      if (SlsCompressionCurveInput.Value != null) {
        slsCompressionCurve = SlsCompressionCurveInput.Value.IStressStrainCurve;
      }

      var slsTensionCurve = material.Material.Serviceability.Tension;
      if (SlsTensionCurveInput.Value != null) {
        slsTensionCurve = SlsTensionCurveInput.Value.IStressStrainCurve;
      }

      //Crack Calculation Parameters
      IConcreteCrackCalculationParameters crackCalcParams = null;
      if (material is IConcrete concrete) {
        crackCalcParams = concrete.ConcreteCrackCalculationParameters;
      }

      if (CrackCalcParamsInput.Value != null) {
        crackCalcParams = CrackCalcParamsInput.Value;
      }

      var strength = ITensionCompressionCurve.Create(ulsTensionCurve, ulsCompressionCurve);
      var serviceability = ITensionCompressionCurve.Create(slsTensionCurve, slsCompressionCurve);

      switch (material.Material) {
        case IConcrete _:
          if (crackCalcParams != null) {
            duplicateMaterial.Material = IConcrete.Create(strength, serviceability, crackCalcParams);
          } else {
            duplicateMaterial.Material = IConcrete.Create(strength, serviceability);
          }
          break;
        case IReinforcement _:
          duplicateMaterial.Material = IReinforcement.Create(strength, serviceability);
          break;
        case ISteel _:
          duplicateMaterial.Material = ISteel.Create(strength, serviceability);
          break;
        case IFrp _:
          duplicateMaterial.Material = IFrp.Create(strength, serviceability);
          break;
      }
      return duplicateMaterial;
    }

    public override void Compute() {

      var material = MaterialInput.Value;
      if (material == null) {
        ErrorMessages.Add("Material input cannot be null.");
        return;
      }

      //Read input and create duplicate material
      var duplicateMaterial = GetDuplicateMaterial(material);
      if (DesignCodeInput.Value != null) {
        duplicateMaterial.DesignCode = DesignCodeInput.Value;
      }

      // Set output values
      MaterialOutput.Value = duplicateMaterial;
      DesignCodeOutput.Value = duplicateMaterial.DesignCode;
      UlsCompressionCurveOutput.Value = new StressStrainCurve() { IStressStrainCurve = duplicateMaterial.Material.Strength.Compression, IsCompression = true };
      UlsTensionCurveOutput.Value = new StressStrainCurve() { IStressStrainCurve = duplicateMaterial.Material.Strength.Tension, IsCompression = false };
      SlsCompressionCurveOutput.Value = new StressStrainCurve() { IStressStrainCurve = duplicateMaterial.Material.Serviceability.Compression, IsCompression = true }; ;
      SlsTensionCurveOutput.Value = new StressStrainCurve() { IStressStrainCurve = duplicateMaterial.Material.Serviceability.Tension, IsCompression = false }; ;
      if (duplicateMaterial is IConcrete concrete) {
        CrackCalcParamsOutput.Value = concrete.ConcreteCrackCalculationParameters;
      }
    }
  }
}
