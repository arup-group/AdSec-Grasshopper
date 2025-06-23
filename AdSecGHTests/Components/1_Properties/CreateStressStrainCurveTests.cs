using System.Collections.Generic;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.GH.Helpers;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components.Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateStressStrainCurveTests {
    private readonly CreateStressStrainCurve _component;

    public CreateStressStrainCurveTests() {
      _component = new CreateStressStrainCurve();
    }

    private static IStressStrainPoint CreateFailurePoint() {
      var strain = Strain.FromRatio(0.0035);
      var stress = Pressure.FromPascals(10);
      return IStressStrainPoint.Create(stress, strain);
    }

    private static IStressStrainPoint CreateYieldPoint() {
      var strain = Strain.FromRatio(0.002);
      var stress = Pressure.FromPascals(10);
      return IStressStrainPoint.Create(stress, strain);
    }

    private static List<object> CreateStressStressPoints() {
      var strain = Strain.FromRatio(0.002);
      var stress = Pressure.FromPascals(10);
      var list = new List<IStressStrainPoint>() { IStressStrainPoint.Create(Pressure.Zero, Strain.Zero), IStressStrainPoint.Create(stress, strain) };
      return list.Cast<object>().ToList();
    }

    private static double CreateInitialModulus() {
      return 8;
    }

    private double CreateFailureStrain() {
      _component.SetSelected(1, 2);
      return 0.0035;
    }

    private static double CreateConfinedStress() {
      return 2;
    }

    private static double CreateUnConfinedStress() {
      return 1.5;
    }

    private static IStressStrainPoint CreatePeakPoint() {
      var strain = Strain.FromRatio(0.001);
      var stress = Pressure.FromPascals(11);
      return IStressStrainPoint.Create(stress, strain);
    }

    [Fact]
    public void TestBilinearModelForInputAndOutput() {
      _component.SetSelected(0, 0);
      ComponentTestHelper.SetInput(_component, CreateYieldPoint(), 0);
      ComponentTestHelper.SetInput(_component, CreateFailurePoint(), 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.Bilinear, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void BilinearModeReturnNullOutputWhenInputIsIsNotCorrect() {
      _component.SetSelected(0, 0);
      ComponentTestHelper.SetInput(_component, CreateYieldPoint(), 0);
      ComponentTestHelper.SetInput(_component, CreateInitialModulus(), 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Contains("Input type mismatch", _component.RuntimeMessages(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error)[0]);
      Assert.Null(result);
    }

    [Fact]
    public void TestExplicitModelForInputAndOutput() {
      _component.SetSelected(0, 1);
      var point = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      ComponentTestHelper.SetInput(_component, point, 0);
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("Stress-strain points list must have at least two points", _component.RuntimeMessages(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error)[0]);
    }

    [Fact]
    public void ExplicitCurveReturnNotNullValueWhenInputIsCorrect() {
      _component.SetSelected(0, 1);
      ComponentTestHelper.SetInput(_component, CreateStressStressPoints());
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      var v1 = _component.RuntimeMessages(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error);
      Assert.NotNull(result);
    }

    [Fact]
    public void ExplicitCurveReportErrorWhenInputIsNotCorrectToFormCurve() {
      _component.SetSelected(0, 1);
      var point1 = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio));
      var point2 = IStressStrainPoint.Create(new Pressure(2, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      var points = new List<object>() {
        point1,
        point2,
      };
      ComponentTestHelper.SetInput(_component, points, 0);
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("The first point in the stress-strain points list must be (0, 0)", _component.RuntimeMessages(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error)[0]);
    }

    [Fact]
    public void TestFibModelForInputAndOutput() {
      _component.SetSelected(0, 2);
      ComponentTestHelper.SetInput(_component, CreatePeakPoint(), 0);
      ComponentTestHelper.SetInput(_component, CreateInitialModulus(), 1);
      ComponentTestHelper.SetInput(_component, CreateFailureStrain(), 2);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.FibModelCode, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void TestLinearModelForInputAndOutput() {
      _component.SetSelected(0, 3);
      ComponentTestHelper.SetInput(_component, CreateFailurePoint(), 0);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.Linear, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void TestManderConfinedModelForInputAndOutput() {
      _component.SetSelected(0, 4);
      _component.SetSelected(1, 2);
      ComponentTestHelper.SetInput(_component, CreateUnConfinedStress(), 0);
      ComponentTestHelper.SetInput(_component, CreateConfinedStress(), 1);
      ComponentTestHelper.SetInput(_component, CreateInitialModulus(), 2);
      ComponentTestHelper.SetInput(_component, 1, 3);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.ManderConfined, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void TestManderModelForInputAndOutput() {
      _component.SetSelected(0, 5);
      ComponentTestHelper.SetInput(_component, CreatePeakPoint(), 0);
      ComponentTestHelper.SetInput(_component, CreateInitialModulus(), 1);
      ComponentTestHelper.SetInput(_component, CreateFailureStrain(), 2);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.Mander, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void TestParabolaRectangleModelForInputAndOutput() {
      _component.SetSelected(0, 6);
      ComponentTestHelper.SetInput(_component, CreateYieldPoint(), 0);
      ComponentTestHelper.SetInput(_component, CreateFailureStrain(), 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.ParabolaRectangle, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void TestParkModelForInputAndOutput() {
      _component.SetSelected(0, 7);
      ComponentTestHelper.SetInput(_component, CreateYieldPoint(), 0);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.Park, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void TestPopovicsModelForInputAndOutput() {
      _component.SetSelected(0, 8);
      ComponentTestHelper.SetInput(_component, CreatePeakPoint(), 0);
      ComponentTestHelper.SetInput(_component, CreateFailureStrain(), 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.Popovics, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void TestRectangleModelForInputAndOutput() {
      _component.SetSelected(0, 9);
      ComponentTestHelper.SetInput(_component, CreateYieldPoint(), 0);
      ComponentTestHelper.SetInput(_component, CreateFailureStrain(), 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal(StressStrainCurveType.Rectangular, _component.BusinessComponent.SelectedCurveType);
      Assert.NotNull(result);
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.StressStrainCrv));
    }
  }
}
