using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class CreateRebarFunction : Function {

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Create Rebar",
      NickName = "Rebar",
      Description = "Create Rebar (single or bundle) for an AdSec Section",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      SubCategory = SubCategoryName.Cat3(),
      Category = CategoryName.Name()
    };
    public override void Compute() { }
  }
}
