using AdSecCore.Functions;

using AdSecGH.Helpers;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSolutionGoo : GH_Goo<SectionSolution> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Results";
    internal Plane LocalPlane => m_plane;
    internal Polyline ProfileEdge { get; }
    internal AdSecSection m_section;
    private Plane m_plane;

    public AdSecSolutionGoo(SectionSolution sectionSolutionParameter) {
      Value = sectionSolutionParameter;
      m_section = new AdSecSection(sectionSolutionParameter.SectionDesign);
      m_plane = m_section.LocalPlane;
      ProfileEdge = m_section.m_profileEdge;
    }

    public AdSecSolutionGoo(ISolution solution, AdSecSection section) {
      Value = new SectionSolution() {
        Solution = solution, SectionDesign = new SectionDesign() {
          Section = section.Section,
          DesignCode = section.DesignCode
        }
      };
      m_section = section;
      m_plane = m_section.LocalPlane;
      ProfileEdge = m_section.m_profileEdge;
    }

    public override IGH_Goo Duplicate() {
      return new AdSecSolutionGoo(Value);
    }

    public override string ToString() {
      return $"AdSec {TypeName}";
    }
  }
}
