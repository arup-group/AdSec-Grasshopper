using System.Collections.Generic;

using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Oasys.AdSec.Materials.StressStrainCurves;

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
    public void SolveInternalWhereBilinearModeWhereInputIsItemReturnsNotNullValues() {
      _component.SetSelected(0, 0);
      var point1 = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      var point2 = IStressStrainPoint.Create(new Pressure(2, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      ComponentTestHelper.SetInput(_component, point1, 0);
      ComponentTestHelper.SetInput(_component, point2, 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);

      Assert.NotNull(result);
    }

    [Fact]
    public void SolveInternalWhereExplicitModeWhereInputIsItemReturnsNull() {
      _component.SetSelected(0, 1);
      var point1 = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      ComponentTestHelper.SetInput(_component, point1, 0);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);

      Assert.Null(result);
    }

    [Fact]
    public void SolveInternalWhereBilinearModeWhereInputIsItemReturnsNull() {
      _component.SetSelected(0, 0);
      var point1 = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      var point2 = IStressStrainPoint.Create(new Pressure(2, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      ComponentTestHelper.SetInput(_component, new List<IStressStrainPoint>() {
        point1,
      }, 0);
      ComponentTestHelper.SetInput(_component, point2, 1);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);

      Assert.Null(result);
    }

    [Fact]
    public void SolveInternalWhereExplicitModeWhereInputIsItemReturnsNotNullValues() {
      _component.SetSelected(0, 1);
      var point1 = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      var point2 = IStressStrainPoint.Create(new Pressure(2, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));
      var points = new List<object>() {
        point1,
        point2,
      };

      ComponentTestHelper.SetInput(_component, points, 0);
      var result = (AdSecStressStrainCurveGoo)ComponentTestHelper.GetOutput(_component);

      Assert.NotNull(result);
    }
  }
}
