using System.Linq;

using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Moq;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests {
    private CreateStressStrainPoint component;
    private Mock<IGH_DataAccess> dataAccess;

    public AdSecInputTests() {
      component = new CreateStressStrainPoint();
      dataAccess = new Mock<IGH_DataAccess>();
    }

    [Fact]
    public void StressStrainCurveGooReturnsNull() {
      var result = AdSecInput.StressStrainCurveGoo(null, dataAccess.Object, 0, false, true);
      Assert.Null(result);
    }

    [Fact]
    public void StressStrainCurveGooReturnsWarningWhenInputIsNonOptional() {
      var result = AdSecInput.StressStrainCurveGoo(component, dataAccess.Object, 0, false);
      Assert.Null(result);

      var runtimeWarnings = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeWarnings);
      Assert.Contains("failed", runtimeWarnings.First());
    }

    [Fact]
    public void StressStrainCurveGooReturnsErrorWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      var result = AdSecInput.TryCastToStressStrainCurve(component, 0, false, objwrap);
      Assert.Null(result);

      var runtimeErrors = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeErrors);
      Assert.Equal("Unable to convert ε to StressStrainCurve", runtimeErrors.First());
    }

    [Fact]
    public void StressStrainCurveGooReturnsGooFromPolyline() {
      IStressStrainCurve crv = ILinearStressStrainCurve.Create(
        IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
      var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, false);

      var objwrap = new GH_ObjectWrapper(tuple.Item1);
      var result = AdSecInput.TryCastToStressStrainCurve(component, 0, false, objwrap);

      Assert.NotNull(result);
      Assert.True(result.Value.IsPolyline());
      Assert.Equal(2, result.ControlPoints.Count);
    }

    [Fact]
    public void StressStrainCurveGooReturnsGooFromAnotherGoo() {
      var exCrv = IExplicitStressStrainCurve.Create();
      var tuple = AdSecStressStrainCurveGoo.Create(exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit,
        false);
      var goo = new AdSecStressStrainCurveGoo(tuple.Item1, exCrv,
        AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);

      var objwrap = new GH_ObjectWrapper(goo);
      var result = AdSecInput.TryCastToStressStrainCurve(component, 0, false, objwrap);

      Assert.NotNull(result);
      Assert.Null(result.Value);
      Assert.Empty(result.ControlPoints);
    }
  }
}
