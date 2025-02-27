using System;

using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class AnalyseFunction : IFunction {
    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Description = "Analyse an AdSec Section",
      Name = "Analyse Section",
      NickName = "Analyse",
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat6(),
    };
    public SectionParameter Section { get; set; } = new SectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to analyse",
    };

    public SolutionParameter Solution { get; set; } = new SolutionParameter {
      Name = "Results",
      NickName = "Res",
      Description = "AdSec Results for a Section. Results object allows to calculate strength (ULS) and serviceability (SLS) results.",
    };

    public LoadSurfaceParameter LoadSurface { get; set; } = new LoadSurfaceParameter {
      Name = "Failure Surface",
      NickName = "Fail",
      Description = "Mesh representing the strength failure surface.",
    };

    public virtual Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Section,
      };
    }

    public Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Solution,
        LoadSurface,
      };
    }

    public void Compute() { throw new NotImplementedException(); }
  }
}
