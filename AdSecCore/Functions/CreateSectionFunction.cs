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
    public Attribute[] GetAllInputAttributes() { throw new System.NotImplementedException(); }

    public Attribute[] GetAllOutputAttributes() { throw new System.NotImplementedException(); }

    public void Compute() { throw new System.NotImplementedException(); }
  }
}
