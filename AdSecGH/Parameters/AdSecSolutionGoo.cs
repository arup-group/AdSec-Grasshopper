using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public class AdSecSolutionGh : AdSecCore.Functions.AdSecSolution {
    public AdSecSolutionGh(ISolution solution, AdSecSection section) : base(solution) {
      Section = section;
      LocalPlane = section.LocalPlane;
      ProfileEdge = section.m_profileEdge;

    }
    internal AdSecSection Section { get; private set; }
    internal Plane LocalPlane { get; private set; }
    internal Polyline ProfileEdge { get; private set; }
  }

  public class AdSecSolutionGoo : GH_Goo<AdSecSolutionGh> {
    public override bool IsValid => true;
    public override string TypeDescription => $"AdSec {TypeName} Parameter";
    public override string TypeName => "Results";

    internal Plane LocalPlane { get { return Value.LocalPlane; } }
    internal ISection Section { get { return Value.Section.Section; } }
    internal IDesignCode DesignCode { get { return Value.Section.DesignCode; } }
    internal Polyline ProfileEdge { get { return Value.ProfileEdge; } }
    internal IStrength Strength { get { return Value.Solution.Strength; } }
    internal IServiceability Serviceability { get { return Value.Solution.Serviceability; } }

    public AdSecSolutionGoo(AdSecSolutionGh solution) : base(solution) {
    }

    public AdSecSolutionGoo(ISolution solution, AdSecSection section) {
      Value = new AdSecSolutionGh(solution, section);
    }

    public override IGH_Goo Duplicate() {
      return new AdSecSolutionGoo(Value);
    }

    public override string ToString() {
      return $"AdSec {TypeName}";
    }
  }
}
