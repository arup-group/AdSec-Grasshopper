using AdSecGHCore.Constants;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

namespace AdSecCore.Functions {
  public class AnalyseFunction : Function {
    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Description = "Analyse an AdSec Section",
      Name = "Analyse Section",
      NickName = "Analyse",
    };
    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat6(),
    };
    public SectionParameter Section { get; set; } = new SectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to analyse",
      Value = new SectionDesign(),
    };

    public SectionSolutionParameter Solution { get; set; } = new SectionSolutionParameter {
      Name = "Results",
      NickName = "Res",
      Description
        = "AdSec Results for a Section. Results object allows to calculate strength (ULS) and serviceability (SLS) results.",
    };

    public LoadSurfaceParameter LoadSurface { get; set; } = new LoadSurfaceParameter {
      Name = "FailureSurface",
      NickName = "Fail",
      Description = "Mesh representing the strength failure surface.",
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Section,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Solution,
        LoadSurface,
      };
    }
    public override void Compute() {
      var adSec = IAdSec.Create(Section.Value.DesignCode.IDesignCode);

      var solution = adSec.Analyse(Section.Value.Section);

      foreach (var warning in solution.Warnings) {
        WarningMessages.Add(warning.Description);
      }

      var sectionSolution = new SectionSolution() {
        Solution = solution,
        SectionDesign = Section.Value,
      };

      Solution.Value = sectionSolution;
      LoadSurface.Value = solution.Strength.GetFailureSurface();
    }
  }
}
