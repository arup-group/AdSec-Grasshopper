using System;

using AdSecCore.Parameters;

using Oasys.Business;

using Attribute = Oasys.Business.Attribute;

namespace AdSecGHTests.Helpers {

  public class FakeBusiness : IBusinessComponent {

    public DoubleParameter Alpha { get; set; } = new DoubleParameter {
      Name = "Alpha",
      NickName = "A",
      Description = "Alpha description",
      Default = 1,
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
    };
    public DoubleParameter Beta { get; set; } = new DoubleParameter {
      Name = "Beta",
      NickName = "B",
      Description = "Beta description",
      Default = 2,
    };

    public ComponentAttribute Metadata { get; set; } = new ComponentAttribute {
      Name = "Dummy Business",
      NickName = "DB",
      Description = "Dummy Business Description",
    };
    public ComponentOrganisation Organisation { get; set; } = new ComponentOrganisation {
      Category = "Dummy Category",
      SubCategory = "Dummy SubCategory",
    };

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

    public void Compute() { Beta.Value = (Alpha.Value * 2) + 10; }

    public void GetDefaultValues() { throw new NotImplementedException(); }
  }
}
