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

  public StressStrainCurveParameter UlsCurve { get; set; } = new StressStrainCurveParameter() {
    Name = "ULS Comp. Crv",
    NickName = "U_C",
    Description = "ULS Stress Strain Curve for Compression",
  };

  public override Attribute[] GetAllInputAttributes() {
    return new Attribute[] {
      null,
      null,
      null,
      null,
      null,
      null,
    };
  }

  public override void Compute() { throw new System.NotImplementedException(); }
}
