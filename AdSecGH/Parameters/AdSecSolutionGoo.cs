using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSolutionGoo : GH_Goo<ISolution> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Results";
    internal Plane LocalPlane => m_plane;
    internal Polyline ProfileEdge { get; }
    internal AdSecSection m_section;
    private Plane m_plane;

    public AdSecSolutionGoo(ISolution solution) : base(solution) {
    }
    public AdSecSolutionGoo(ISolution solution, AdSecSection section) : base(solution) {
      m_section = section;
      m_plane = m_section == null ? Plane.WorldXY : m_section.LocalPlane;
      ProfileEdge = m_section?.m_profileEdge;
    }

    public override IGH_Goo Duplicate() {
      return new AdSecSolutionGoo(Value, m_section);
    }

    public override string ToString() {
      return $"AdSec {TypeName}";
    }
  }
}
