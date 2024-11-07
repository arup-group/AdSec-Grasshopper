using System;

using AdSecCore.Parameters;

using Oasys.Business;
using Oasys.GH.Helpers;

using Attribute = Oasys.Business.Attribute;

namespace AdSecGHTests.Helpers {
  public class MalformedParameter : ParameterAttribute<object> { }

  public class MalformedComponent : BusinessOasysGlue<MalformedBusiness> {
    public override Guid ComponentGuid => new Guid("D3A8C9E-417C-42AE-B704-91F214C8C873");
  }

  public class AllParameters : IBusinessComponent {

    public AdSecPointArrayParameter Points { get; set; } = new AdSecPointArrayParameter {
      Name = "Points",
      NickName = "P",
      Description = "Points description",
    };
    public IAdSecSectionParameter Section { get; set; } = new IAdSecSectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to get single rebars from",
    };

    public AdSecMaterialArrayParam Material { get; set; } = new AdSecMaterialArrayParam {
      Name = "Materials",
      NickName = "Mats",
      Description = "Material description",
    };

    public StringArrayParam MatString { get; set; } = new StringArrayParam {
      Name = "MatString",
      NickName = "MS",
      Description = "Material description",
    };

    public IntegerArrayParameter BundleCount { get; set; } = new IntegerArrayParameter {
      Name = "Bundle Count",
      NickName = "BC",
      Description = "Bundle Count description",
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
        Material,
        BundleCount,
        MatString,
      };
    }
  }

  public class AllParametersComponent : BusinessOasysGlue<AllParameters> {
    public override Guid ComponentGuid => new Guid("CAA08C9E-417C-42AE-B704-91F214C8C873");
  }
}
