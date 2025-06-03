using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class CreateRebarSpacingFunction : Function {

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Create Rebar Spacing",
      NickName = "Spacing",
      Description = "Create Rebar spacing (by Count or Pitch) for an AdSec Section",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3()
    };
    public override void Compute() { }
  }
}
