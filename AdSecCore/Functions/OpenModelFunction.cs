using AdSecCore.Functions;

using AdSecGHCore.Constants;

public class OpenModelFunction : Function {

  public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
    Description = "Open an existing AdSec .ads file",
    Name = "Open Model",
    NickName = "Open",
  };
  public override Organisation Organisation { get; set; } = new Organisation() {
    Category = CategoryName.Name(),
    SubCategory = SubCategoryName.Cat0(),
  };
  public override void Compute() { }
}
