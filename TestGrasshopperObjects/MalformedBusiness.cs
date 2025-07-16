using AdSecCore.Functions;

namespace AdSecGHTests.Helpers {
  public class MalformedFunction : Function {

    public MalformedParameter Malformed { get; set; } = new MalformedParameter {
      Name = "Malformed",
      NickName = "M",
      Description = "Malformed description",
    };
    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Malformed Component",
      NickName = "MC",
      Description = "Malformed Component",
    };
    public override Organisation Organisation { get; set; } = new Organisation {
      Category = "Test",
      SubCategory = "Test",
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Malformed,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Malformed,
      };
    }

    public override void Compute() { /* need to implement the interface but is not needed for the test */
    }
  }
}
