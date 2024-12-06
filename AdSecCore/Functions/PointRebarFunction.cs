using System;

using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class PointRebarFunction : IFunction {
    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Vertex Point",
      NickName = "Vertex Point",
      Description = "Create a 2D vertex in local yz-plane for AdSec Profile and Reinforcement",
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3(),
      Hidden = false,
    };
    public Attribute[] GetAllInputAttributes() { throw new NotImplementedException(); }

    public Attribute[] GetAllOutputAttributes() { throw new NotImplementedException(); }

    public void Compute() { throw new NotImplementedException(); }
  }
}
