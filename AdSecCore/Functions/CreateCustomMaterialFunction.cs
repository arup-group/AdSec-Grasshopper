using AdSecCore.Functions;

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
  public override void Compute() { throw new System.NotImplementedException(); }
}
