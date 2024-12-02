using System;

using AdSecCore.Functions;

using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGHTests.Helpers {
  public class MalformedFunction : IFunction {

    public MalformedParameter Malformed { get; set; } = new MalformedParameter {
      Name = "Malformed",
      NickName = "M",
      Description = "Malformed description",
    };
    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Malformed Component",
      NickName = "MC",
      Description = "Malformed Component",
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = "Test",
      SubCategory = "Test",
    };

    public Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Malformed,
      };
    }

    public Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Malformed,
      };
    }

    public void Compute() { }

    public void UpdateInputValues(params object[] values) { throw new NotImplementedException(); }
  }
}
