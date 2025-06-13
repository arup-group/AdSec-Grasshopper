using AdSecCore.Functions;
using AdSecCore.Parameters;

using AdSecGHCore.Constants;

public class CreateCustomMaterialFunction : Function {

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

  public override Attribute[] GetAllInputAttributes() {
    return new Attribute[] {
      DesignCode,
      UlsCompressionCurve,
      UlsTensionCurve,
      SlsCompressionCurve,
      SlsTensionCurve,
      CrackCalcParams,
    };
  }

  public override void Compute() { throw new System.NotImplementedException(); }
}
