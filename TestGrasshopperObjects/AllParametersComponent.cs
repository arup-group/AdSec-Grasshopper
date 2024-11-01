using System;

using Oasys.Business;
using Oasys.GH.Helpers;

using Attribute = Oasys.Business.Attribute;

namespace AdSecGHTests.Helpers {
  public class AllParameters : IBusinessComponent {

    public AdSecPointArrayParameter Points { get; set; } = new AdSecPointArrayParameter {
      Name = "Points",
      NickName = "P",
      Description = "Points description",
      Access = Access.List,
    };
    public IAdSecSectionParameter Section { get; set; } = new IAdSecSectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to get single rebars from",
      Access = Access.Item,
    };
    public ComponentAttribute Metadata { get; set; } = new ComponentAttribute {
      Name = "All Parameters",
      NickName = "AP",
      Description = "Get all parameters",
    };
    public ComponentOrganisation Organisation { get; set; } = new ComponentOrganisation {
      Category = "Test",
      SubCategory = "Test",
    };

    public Attribute[] GetAllInputAttributes() { return GetAllParams(); }

    public Attribute[] GetAllOutputAttributes() { return GetAllParams(); }

    public void UpdateInputValues(params object[] values) { throw new NotImplementedException(); }

    public void Compute() { }

    private Attribute[] GetAllParams() {
      return new Attribute[] {
        Section,
        Points,
      };
    }
  }

  public class AllParametersComponent : BusinessOasysGlue<AllParameters> {
    public override Guid ComponentGuid => new Guid("CAA08C9E-417C-42AE-B704-91F214C8C873");
  }
}
