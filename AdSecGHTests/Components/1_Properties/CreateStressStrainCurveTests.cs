using System.Collections.Generic;

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

    [Fact]
    public void BilinearCurveReturnNotNullValueWhenInputIsCorrect() {
      _component.SetSelected(0, 0);
      var point1 = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      var point2 = IStressStrainPoint.Create(new Pressure(2, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      ComponentTestHelper.SetInput(_component, point1, 0);
      ComponentTestHelper.SetInput(_component, point2, 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);

      Assert.NotNull(result);
    }

    [Fact]
    public void ExplicitCurveReturnsNullWhenSinglePointUsedToCreateCurve() {
      _component.SetSelected(0, 1);
      var point = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      ComponentTestHelper.SetInput(_component, point, 0);
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("Stress-strain points list must have at least two points", _component.RuntimeMessages(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error)[0]);

    }

    [Fact]
    public void SolveInternalWhereBilinearModeWhereInputIsIsNotCorrect() {
      _component.SetSelected(0, 0);
      var point1 = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      var point2 = IStressStrainPoint.Create(new Pressure(2, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      ComponentTestHelper.SetInput(_component, new List<IStressStrainPoint>() {
        point1,
      }, 0);
      ComponentTestHelper.SetInput(_component, point2, 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Contains("Input type mismatch", _component.RuntimeMessages(Grasshopper.Kernel.GH_RuntimeMessageLevel.Error)[0]);
      Assert.Null(result);
    }

    [Fact]
    public void ExplicitCurveTypeNotNullValuesWhenInputIsCorrect() {
      _component.SetSelected(0, 1);
      var point1 = IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(0, StrainUnit.Ratio));
      var point2 = IStressStrainPoint.Create(new Pressure(2, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      var points = new List<object>() {
        point1,
        point2,
      };

      ComponentTestHelper.SetInput(_component, points, 0);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);
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
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.StressStrainCrv));
    }
  }
}
