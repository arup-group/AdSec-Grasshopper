using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class EditProfileFunction : Function {

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Edit Profile",
      NickName = "ProfileEdit",
      Description = "Modify an AdSec Profile",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat2()
    };
    public override void Compute() { }
  }
}
