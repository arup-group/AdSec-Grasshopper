using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class CreateSectionFunction : IFunction {

    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Section",
      NickName = "Section",
      Description = "Create an AdSec Section"
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };
    public ProfileParameter Profile { get; set; } = new ProfileParameter() {
      Name = "Profile",
      NickName = "Pf",
      Description = "AdSec Profile defining the Section solid boundary",
      Access = Access.Item
    };
    public virtual Attribute[] GetAllInputAttributes() { return new Attribute[] { Profile }; }

    public SectionParameter Section { get; set; } = new SectionParameter() {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section",
      Access = Access.Item
    };
    public virtual Attribute[] GetAllOutputAttributes() { return new Attribute[] { Section }; }

    public void Compute() { }
  }
}
