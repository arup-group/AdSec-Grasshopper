using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

using IFlange = Oasys.Profiles.IFlange;
using IWebConstant = Oasys.Profiles.IWebConstant;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecParametersReflectionTests {
    public AdSecParametersReflectionTests() {
      InitInstances();
    }

    public Type[] GoosWithNickname = {
      typeof(AdSecConcreteCrackCalculationParametersGoo),
      typeof(AdSecCrackGoo),
      typeof(AdSecDesignCodeGoo),
      typeof(AdSecLoadGoo),
      typeof(AdSecMaterialGoo),
    };

    public Type[] GoosWithoutNickname = {
      typeof(AdSecDeformationGoo),
      typeof(AdSecFailureSurfaceGoo),
      typeof(AdSecInteractionDiagramGoo),
      typeof(AdSecPointGoo),
      typeof(AdSecProfileFlangeGoo),
      typeof(AdSecProfileGoo),
      typeof(AdSecProfileWebGoo),
      typeof(AdSecRebarBundleGoo),
      typeof(AdSecRebarGroupGoo),
      typeof(AdSecRebarLayerGoo),
      typeof(AdSecSectionGoo),
      typeof(AdSecSolutionGoo),
      typeof(AdSecStressStrainCurveGoo),
      typeof(AdSecStressStrainPointGoo),
      typeof(AdSecSubComponentGoo),
    };

    public List<IGH_Goo> InstanceOfGoos = new List<IGH_Goo>();

    private static bool IsNullOrEmptyOrWhitespace(string value) {
      return string.IsNullOrWhiteSpace(value) || value == string.Empty;
    }

    private static Curvature GetCurvatureOne() {
      return Curvature.From(1, CurvatureUnit.PerCentimeter);
    }

    private static Strain GetStrainOne() {
      return Strain.From(1, StrainUnit.Ratio);
    }

    private static Pressure GetPressureOne() {
      return Pressure.From(1, PressureUnit.Atmosphere);
    }

    private static Pressure GetNegativePressure() {
      return Pressure.From(-1, PressureUnit.Atmosphere);
    }

    private void InitInstances() {
      var concreteCrackCalculationParameters
        = IConcreteCrackCalculationParameters.Create(GetPressureOne(), GetNegativePressure(), GetPressureOne());
      var length = new Length(1, LengthUnit.Meter);
      var thickness = new Length(0.2, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(thickness, length),
        new WebConstant(thickness)));
      var section = ISection.Create(profile, Concrete.ACI318.Edition_2002.Metric.MPa_20);
      var designCode = new AdSecDesignCode(ACI318.Edition_2002.Metric, "test");
      var adSecSection = new AdSecSection(section, designCode.DesignCode, "", "", Plane.WorldXY);
      var adSec = IAdSec.Create(adSecSection.DesignCode);
      var solution = adSec.Analyse(adSecSection.Section);
      var solutionBuilder = new SolutionBuilder().Build();
      var stressStrainPoint
        = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio));
      IStressStrainCurve curve = ILinearStressStrainCurve.Create(stressStrainPoint);
      var tensionCompressionCurve = ITensionCompressionCurve.Create(curve, curve);
      var load = ILoad.Create(Force.FromKilonewtons(100), Moment.FromKilonewtonMeters(100), Moment.Zero);
      //-----------------
      InstanceOfGoos.Add(new AdSecConcreteCrackCalculationParametersGoo(concreteCrackCalculationParameters));
      InstanceOfGoos.Add(new AdSecCrackGoo(new CrackLoad() { Plane = OasysPlane.PlaneYZ, Load = solutionBuilder.Serviceability.Check(load).MaximumWidthCrack, }));
      InstanceOfGoos.Add(
        new AdSecDeformationGoo(IDeformation.Create(GetStrainOne(), GetCurvatureOne(), GetCurvatureOne())));
      InstanceOfGoos.Add(new AdSecDesignCodeGoo(designCode));
      InstanceOfGoos.Add(new AdSecFailureSurfaceGoo(solution.Strength.GetFailureSurface(), Plane.WorldXY));
      InstanceOfGoos.Add(new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment())));
      InstanceOfGoos.Add(new AdSecMaterialGoo(new MaterialDesign()));
      InstanceOfGoos.Add(new AdSecInteractionDiagramGoo(
        solution.Strength.GetForceMomentInteractionCurve(new Angle())[0], Angle.FromRadians(0), new Rectangle3d()));
      InstanceOfGoos.Add(new AdSecPointGoo(length, length));
      InstanceOfGoos.Add(new AdSecProfileFlangeGoo(IFlange.Create(length, thickness)));
      InstanceOfGoos.Add(new AdSecProfileGoo(profile, Plane.WorldXY));
      InstanceOfGoos.Add(new AdSecProfileWebGoo(IWebConstant.Create(thickness)));
      var barBundle
        = IBarBundle.Create(IReinforcement.Create(tensionCompressionCurve, tensionCompressionCurve), length);
      InstanceOfGoos.Add(new AdSecRebarBundleGoo(barBundle));
      InstanceOfGoos.Add(new AdSecRebarGroupGoo(new AdSecRebarGroup(ICircleGroup.Create(IPoint.Create(length, length),
        length, Angle.Zero, ILayerByBarCount.Create(1, barBundle)))));
      InstanceOfGoos.Add(new AdSecRebarLayerGoo(ILayerByBarCount.Create(1, barBundle)));
      InstanceOfGoos.Add(new AdSecSectionGoo(adSecSection));
      InstanceOfGoos.Add(new AdSecSolutionGoo(solution, adSecSection));
      InstanceOfGoos.Add(new AdSecStressStrainCurveGoo(new PolylineCurve(), curve,
        AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, new List<Point3d>()));
      InstanceOfGoos.Add(new AdSecStressStrainPointGoo(stressStrainPoint));
      InstanceOfGoos.Add(new AdSecSubComponentGoo(section, Plane.WorldXY, IPoint.Create(length, length),
        designCode.DesignCode, "test", "test1"));
    }

    [Fact]
    public void CheckInstancesDuplicate() {
      foreach (var goo in InstanceOfGoos) {
        var gooDuplicate = goo.Duplicate();
        Assert.NotNull(gooDuplicate);
        Assert.Equal(goo.ToString(), gooDuplicate.ToString());
      }
    }

    [Fact]
    public void NicknameProperty_ReturnsValidString() {
      foreach (var type in GoosWithNickname) {
        var property = type.GetProperty("NickName", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.False(IsNullOrEmptyOrWhitespace(value), $"Failed for {type}");
      }
    }

    [Fact]
    public void NameProperty_ReturnsValidString() {
      foreach (var type in GoosWithNickname) {
        var property = type.GetProperty("Name", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.False(string.IsNullOrEmpty(value), $"Failed for {type}");
      }
    }

    [Fact]
    public void DescriptionProperty_ReturnsValidString() {
      foreach (var type in GoosWithNickname) {
        var property = type.GetProperty("Description", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.False(string.IsNullOrEmpty(value), $"Failed for {type}");
      }
    }

    [Fact]
    public void TypeNameProperty_ReturnsValidString() {
      foreach (var type in GoosWithoutNickname) {
        var instance = InstanceOfGoos.Find(goo => goo.GetType() == type);
        var property = instance.GetType().GetProperty("TypeName");
        string value = property?.GetValue(instance) as string;

        Assert.False(string.IsNullOrEmpty(value), $"Failed for {type}");
      }
    }

    [Fact]
    public void TypeDescriptionProperty_ReturnsValidString() {
      foreach (var type in GoosWithoutNickname) {
        var instance = InstanceOfGoos.Find(goo => goo.GetType() == type);
        var property = instance.GetType().GetProperty("TypeDescription");
        string value = property?.GetValue(instance) as string;

        Assert.False(string.IsNullOrEmpty(value), $"Failed for {type}");
      }
    }

    [Fact]
    public void AllGoosAreTested() {
      //get all classes from this namespace with "goo" at the end of the name
      var types = typeof(AdSecConcreteCrackCalculationParametersGoo).Assembly.GetTypes()
       .Where(t => t.Namespace == "AdSecGH.Parameters" && t.Name.EndsWith("Goo"));

      Assert.Equal(types.Count(), InstanceOfGoos.Count);
    }
  }
}
