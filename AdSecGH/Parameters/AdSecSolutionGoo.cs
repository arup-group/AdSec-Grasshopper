using AdSecCore.Functions;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSolutionGoo : GH_Goo<SectionSolution> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Results";
    internal Plane LocalPlane => _plane;
    internal Polyline ProfileEdge { get; }
    internal AdSecSection m_section;
    private readonly Plane _plane;

    public AdSecSolutionGoo(SectionSolution sectionSolutionParameter) {
      Value = sectionSolutionParameter;
      m_section = new AdSecSection(sectionSolutionParameter.SectionDesign);
      _plane = m_section.LocalPlane;
      ProfileEdge = m_section._profileEdge;
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
      m_section = section;
      _plane = m_section.LocalPlane;
      ProfileEdge = m_section._profileEdge;
    }

    public override IGH_Goo Duplicate() {
      return new AdSecSolutionGoo(Value);
    }

    public override string ToString() {
      return $"AdSec {TypeName}";
    }
  }
}
