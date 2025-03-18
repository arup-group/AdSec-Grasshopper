using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class EditSectionFunction : Function {

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "EditSection",
      NickName = "EditSect",
      Description = "Edit an AdSec Section",
    };
    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };

    public override void Compute() { }
  }
}
