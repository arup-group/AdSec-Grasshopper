using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class RebarGroupFunction : Function {

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Create Reinforcement Group",
      NickName = "Reinforcement Group",
      Description = "Create a Template Reinforcement Group for an AdSec Section"
    };

    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3()
    };
    public override void Compute() { throw new System.NotImplementedException(); }

    public RebarGroupParameter Layout { get; set; } = new RebarGroupParameter() {
      Name = "Layout",
      NickName = "RbG",
      Description = "Rebar Groups for AdSec Section",
    };

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Layout
      };
    }
  }
}
