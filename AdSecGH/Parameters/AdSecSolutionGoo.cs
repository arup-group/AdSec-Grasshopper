using System;
using Grasshopper.Kernel.Types;
using Oasys.AdSec;
using Rhino.Geometry;

namespace AdSecGH.Parameters
{
  public class AdSecSolutionGoo : GH_Goo<ISolution>
  {
    public override bool IsValid => true;
    public override string TypeName => "Results";
    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";
    internal AdSecSection m_section;
    internal Plane LocalPlane
    {
      get
      {
        return m_plane;
      }
    }
    internal Polyline ProfileEdge
    {
      get
      {
        return m_profile;
      }
    }
    private Plane m_plane;
    private Polyline m_profile;

    public AdSecSolutionGoo(ISolution solution, AdSecSection section) : base(solution)
    {
      m_section = section;
      m_plane = m_section.LocalPlane;
      m_profile = m_section.m_profileEdge;
    }

    public override IGH_Goo Duplicate()
    {
      return new AdSecSolutionGoo(this.Value, m_section);
    }

    public override string ToString()
    {
      return "AdSec " + TypeName;
    }
  }
}
