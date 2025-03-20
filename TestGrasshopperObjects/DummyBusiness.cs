using System;

using AdSecCore.Functions;

using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGHTests.Helpers {

  public class FakeBusiness : IFunction {

    public DoubleParameter Alpha { get; set; } = new DoubleParameter {
      Name = "Alpha",
      NickName = "A",
      Description = "Alpha description",
      Default = 1,
      Optional = false,
    };
    public DoubleArrayParameter Gama { get; set; } = new DoubleArrayParameter {
      Name = "Gama",
      NickName = "G",
      Description = "Game description",
      Default = new double[] {
        1,
        2,
        3,
      },
      Optional = false,
    };
    public DoubleParameter Beta { get; set; } = new DoubleParameter {
      Name = "Beta",
      NickName = "B",
      Description = "Beta description",
      Default = 2,
    };

    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Dummy Business",
      NickName = "DB",
      Description = "Dummy Business Description",
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = "Dummy Category",
      SubCategory = "Dummy SubCategory",
    };

    public void Compute() { Beta.Value = (Alpha.Value * 2) + 10; }

    public Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Alpha,
        Gama,
      };
    }

    public Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Beta,
      };
    }

    public void UpdateInputValues(params object[] values) { }

    public void GetDefaultValues() { throw new NotImplementedException(); }
  }
}
