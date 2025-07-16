using System;

using AdSecCore.Functions;

using Oasys.GH.Helpers;

using Attribute = AdSecCore.Functions.Attribute;

namespace AdSecGHTests.Helpers {
  public class MalformedParameter : ParameterAttribute<object> { }

  public class MalformedComponent : ComponentAdapter<MalformedFunction> {
    public override Guid ComponentGuid => new Guid("D3A8C9E-417C-42AE-B704-91F214C8C873");
  }

  public class AllParameters : Function {

    public AdSecPointArrayParameter Points { get; set; } = new AdSecPointArrayParameter {
      Name = "Points",
      NickName = "P",
      Description = "Points description",
    };
    public AdSecSectionParameter Section { get; set; } = new AdSecSectionParameter {
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
    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "All Parameters",
      NickName = "AP",
      Description = "Get all parameters",
    };
    public override Organisation Organisation { get; set; } = new Organisation {
      Category = "Test",
      SubCategory = "Test",
    };

    public override void Compute() { }

    public override Attribute[] GetAllInputAttributes() { return GetAllParams(); }

    public override Attribute[] GetAllOutputAttributes() { return GetAllParams(); }

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

  public class AllParametersComponent : ComponentAdapter<AllParameters> {
    public override Guid ComponentGuid => new Guid("CAA08C9E-417C-42AE-B704-91F214C8C873");
  }
}
