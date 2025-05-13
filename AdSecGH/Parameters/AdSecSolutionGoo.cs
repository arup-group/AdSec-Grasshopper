using AdSecCore.Functions;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSolutionGoo : GH_Goo<SectionSolution> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Results";
    internal Plane LocalPlane => plane;
    internal Polyline ProfileEdge { get; }
    internal AdSecSection section;
    private readonly Plane plane;

    public AdSecSolutionGoo(SectionSolution sectionSolutionParameter) {
      Value = sectionSolutionParameter;
      section = new AdSecSection(sectionSolutionParameter.SectionDesign);
      plane = section.LocalPlane;
      ProfileEdge = section.ProfileData.ProfileEdge;
    }

    public AdSecSolutionGoo(ISolution solution, AdSecSection section) {
      Value = new SectionSolution() {
        Solution = solution, SectionDesign = new SectionDesign() {
          Section = section.Section,
          DesignCode = new DesignCode() {
            IDesignCode = section.DesignCode,
            DesignCodeName = section._codeName,
          },
        },
      };
      this.section = section;
      plane = this.section.LocalPlane;
      ProfileEdge = this.section.ProfileData.ProfileEdge;
    }

    public override IGH_Goo Duplicate() {
      return new AdSecSolutionGoo(Value);
    }

    public override string ToString() {
      return $"AdSec {TypeName}";
    }
  }
}
