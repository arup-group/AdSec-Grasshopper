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
      XAxis = OasysPoint.YAxis,
      YAxis = OasysPoint.ZAxis,
    };
    public static readonly OasysPlane PlaneXY = new OasysPlane {
      XAxis = OasysPoint.XAxis,
      YAxis = OasysPoint.YAxis,
    };
    public OasysPoint Origin { get; set; } = new OasysPoint { };
    public OasysPoint XAxis { get; set; }
    public OasysPoint YAxis { get; set; }

    public override bool Equals(object obj) {
      if (obj is OasysPlane other) {
        return Origin.Equals(other.Origin) && XAxis.Equals(other.XAxis) && YAxis.Equals(other.YAxis);
      }

      return false;
    }

    public override int GetHashCode() {
      return Origin.GetHashCode() ^ XAxis.GetHashCode() ^ YAxis.GetHashCode();
    }
  }

  public class OasysPoint {
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public OasysPoint() { }

    public OasysPoint(double x, double y, double z) {
      X = x;
      Y = y;
      Z = z;
    }

    public static OasysPoint Zero { get; set; } = new OasysPoint() { };
    public static OasysPoint XAxis { get; set; } = new OasysPoint() { X = 1, Y = 0, Z = 0 };
    public static OasysPoint YAxis { get; set; } = new OasysPoint() { X = 0, Y = 1, Z = 0 };
    public static OasysPoint ZAxis { get; set; } = new OasysPoint() { X = 0, Y = 0, Z = 1 };

    public override bool Equals(object obj) {
      if (obj is OasysPoint other) {
        return Math.Abs(X - other.X) < 1e-6 && Math.Abs(Y - other.Y) < 1e-6 && Math.Abs(Z - other.Z) < 1e-6;
      }

      return false;
    }

    public override int GetHashCode() {
      return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
    }
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
  public class NullableDoubleParameter : ParameterAttribute<double?> { }
  public class DoubleArrayParameter : BaseArrayParameter<double> { }
  public class IntegerArrayParameter : BaseArrayParameter<int> { }
  public class SectionParameter : ParameterAttribute<SectionDesign> { }
  public class SectionArrayParameter : BaseArrayParameter<SectionDesign> { }
  public class ProfileParameter : ParameterAttribute<ProfileDesign> { }
  public class PathParameter : ParameterAttribute<string> { }
  public class PlaneParameter : ParameterAttribute<OasysPlane> { }

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
    public bool IsFailureNeutralAxis { get; set; } = false;
  }

  public class SubComponent {
    public ISubComponent ISubComponent { get; set; }
    public SectionDesign SectionDesign { get; set; } = new SectionDesign();
  }

  public class LoadSurfaceDesign {
    public ILoadSurface LoadSurface { get; set; }
    public OasysPlane LocalPlane { get; set; } = OasysPlane.PlaneYZ;
  }

  public class PointArrayParameter : BaseArrayParameter<IPoint> { }
  public class PointParameter : ParameterAttribute<IPoint> { }
  public class StringArrayParam : BaseArrayParameter<string> { }
  public class StringParameter : ParameterAttribute<string> { }
  public class LengthParameter : ParameterAttribute<Length> { }
  public class SectionSolutionParameter : ParameterAttribute<SectionSolution> { }
  public class LoadSurfaceParameter : ParameterAttribute<LoadSurfaceDesign> { }
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
  public class BooleanParameter : ParameterAttribute<bool?> { }
  public class RebarLayerParameter : BaseArrayParameter<ILayer> { }

  public class RebarGroupArrayParameter : BaseArrayParameter<AdSecRebarGroup> { }
  public class RebarGroupParameter : ParameterAttribute<AdSecRebarGroup> { }

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
  public class StrainParameter : ParameterAttribute<Strain> { }
  public class PressureParameter : ParameterAttribute<Pressure> { }
  public class StrainArrayParameter : BaseArrayParameter<Strain> { }
  public class PressureArrayParameter : BaseArrayParameter<Pressure> { }
  public class CurvatureParameter : ParameterAttribute<Curvature> { }
  public class ForceParameter : ParameterAttribute<Force> { }
  public class MomentParameter : ParameterAttribute<Moment> { }
}
