using System;

using Oasys.Business;

using Attribute = Oasys.Business.Attribute;

namespace AdSecGHTests.Helpers {
  public class MalformedBusiness : IBusinessComponent {

    public MalformedParameter Malformed { get; set; } = new MalformedParameter {
      Name = "Malformed",
      NickName = "M",
      Description = "Malformed description",
    };
    public ComponentAttribute Metadata { get; set; } = new ComponentAttribute {
      Name = "Malformed Component",
      NickName = "MC",
      Description = "Malformed Component",
    };
    public ComponentOrganisation Organisation { get; set; } = new ComponentOrganisation {
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

    public void UpdateInputValues(params object[] values) { throw new NotImplementedException(); }

    public void Compute() { }
  }
}
