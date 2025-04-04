using System;
using System.Linq;

using AdSecGH.Parameters;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Mesh;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Functions {
  public class SectionDesign {
    public ISection Section { get; set; }
    public DesignCode DesignCode { get; set; }
    public string CodeName { get; set; }
    public string MaterialName { get; set; }
    public OasysPlane LocalPlane { get; set; } = OasysPlane.PlaneYZ;
  }

  public class OasysPlane {
    public static readonly OasysPlane PlaneYZ = new OasysPlane {
      Origin = new OasysPoint { X = 0, Y = 0, Z = 0, },
      XAxis = new OasysPoint { X = 0, Y = 1, Z = 0, },
      YAxis = new OasysPoint { X = 0, Y = 0, Z = 1, },
    };
    public OasysPoint Origin { get; set; }
    public OasysPoint XAxis { get; set; }
    public OasysPoint YAxis { get; set; }
  }

  public class OasysPoint {
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
  }

  public class SectionSolution {
    public ISolution Solution { get; set; }
    public SectionDesign SectionDesign { get; set; } = new SectionDesign();
    public IStrength Strength => Solution.Strength;
    public IServiceability Serviceability => Solution.Serviceability;
  }

  public class CrackLoad {
    public ICrack Load { get; set; }
    public OasysPlane Plane { get; set; } = OasysPlane.PlaneYZ;
  }

  public class DoubleParameter : ParameterAttribute<double> { }
  public class DoubleArrayParameter : BaseArrayParameter<double> { }
  public class IntegerArrayParameter : BaseArrayParameter<int> { }
  public class SectionParameter : ParameterAttribute<SectionDesign> { }
  public class ProfileParameter : ParameterAttribute<ProfileDesign> { }

  public class ProfileDesign {
    public IProfile Profile { get; set; }
    public OasysPlane LocalPlane { get; set; } = OasysPlane.PlaneYZ;

    public static ProfileDesign From(SectionDesign sectionDesign) {
      return new ProfileDesign {
        Profile = sectionDesign.Section.Profile,
        LocalPlane = sectionDesign.LocalPlane,
      };
    }
  }

  public class NeutralAxis {
    public Length Offset { get; set; }
    public double Angle { get; set; }
    public SectionSolution Solution { get; set; }
  }

  public class SubComponent {
    public ISubComponent ISubComponent { get; set; }
    public SectionDesign SectionDesign { get; set; } = new SectionDesign();
  }

  public class PointArrayParameter : BaseArrayParameter<IPoint> { }
  public class PointParameter : ParameterAttribute<IPoint> { }
  public class StringArrayParam : BaseArrayParameter<string> { }
  public class StringParameter : ParameterAttribute<string> { }
  public class LengthParameter : ParameterAttribute<Length> { }
  public class SectionSolutionParameter : ParameterAttribute<SectionSolution> { }
  public class LoadSurfaceParameter : ParameterAttribute<ILoadSurface> { }
  public class SubComponentParameter : ParameterAttribute<SubComponent> { }

  public class SubComponentArrayParameter : BaseArrayParameter<SubComponent> {
    public SubComponent[] From(SectionDesign sectionDesign) {
      var sectionSubComponents = sectionDesign.Section.SubComponents;
      return sectionSubComponents.Select(x => new SubComponent {
        ISubComponent = ISubComponent.Create(x.Section, x.Offset),
        SectionDesign = sectionDesign,
      }).ToArray();
    }
  }

  public class IntegerParameter : ParameterAttribute<int> { }
  public class LoadParameter : ParameterAttribute<ILoad> { }
  public class MaterialParameter : ParameterAttribute<MaterialDesign> { }
  public class CrackParameter : ParameterAttribute<CrackLoad> { }
  public class DeformationParameter : ParameterAttribute<IDeformation> { }
  public class GenericParameter : ParameterAttribute<object> { }
  public class CrackArrayParameter : BaseArrayParameter<CrackLoad> { }
  public class SecantStiffnessParameter : ParameterAttribute<IStiffness> { }
  public class IntervalArrayParameter : BaseArrayParameter<Tuple<double, double>> { }

  public class MaterialDesign {
    public IMaterial Material { get; set; }
    public DesignCode DesignCode { get; set; }
    public string GradeName { get; set; }

    public static MaterialDesign From(SectionDesign sectionValue) {
      return new MaterialDesign {
        Material = sectionValue.Section.Material,
        DesignCode = DesignCodeParameter.From(sectionValue),
        GradeName = sectionValue.MaterialName,
      };
    }
  }

  public class RebarBundleParameter : ParameterAttribute<IBarBundle> { }
  public class RebarLayerParameter : BaseArrayParameter<ILayer> { }

  public class RebarGroupParameter : BaseArrayParameter<AdSecRebarGroup> { }

  public class DesignCodeParameter : ParameterAttribute<DesignCode> {
    public static DesignCode From(SectionDesign section) {
      return new DesignCode {
        IDesignCode = section.DesignCode.IDesignCode,
        DesignCodeName = section.DesignCode.DesignCodeName,
      };
    }
  }

  public class DesignCode {
    public IDesignCode IDesignCode { get; set; }
    public string DesignCodeName { get; set; }
  }

  public class GeometryParameter : ParameterAttribute<object> { }
  public class NeutralLineParameter : ParameterAttribute<NeutralAxis> { }
}
