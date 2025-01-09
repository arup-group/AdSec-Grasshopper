using System.Collections.Generic;
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

using Rhino.Geometry;

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
    public void TryCastToStressStrainCurveReturnsErrorWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      var result = AdSecInput.TryCastToStressStrainCurve(component, 0, false, objwrap);
      Assert.Null(result);

      var runtimeErrors = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeErrors);
      Assert.Equal("Unable to convert ε to StressStrainCurve", runtimeErrors.First());
    }

    [Fact]
    public void TryCastToStressStrainCurveReturnsGooFromPolyline() {
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
    public void TryCastToStressStrainCurveReturnsGooFromAnotherGoo() {
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

    [Fact]
    public void StressStrainPointReturnsNull() {
      var result = AdSecInput.StressStrainPoint(null, dataAccess.Object, 0, true);
      Assert.Null(result);
    }

    [Fact]
    public void StressStrainPointReturnsWarningWhenInputIsNonOptional() {
      var result = AdSecInput.StressStrainPoint(component, dataAccess.Object, 0);
      Assert.Null(result);

      var runtimeWarnings = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeWarnings);
      Assert.Contains("failed", runtimeWarnings.First());
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsErrorWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      var result = AdSecInput.TryCastToStressStrainPoint(component, 0, objwrap);
      Assert.Null(result);

      var runtimeErrors = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeErrors);
      Assert.Equal("Unable to convert ε to StressStrainPoint", runtimeErrors.First());
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsSamePoint() {
      var crv = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      var objwrap = new GH_ObjectWrapper(crv);
      var result = AdSecInput.TryCastToStressStrainPoint(component, 0, objwrap);

      Assert.NotNull(result);
      Assert.Equal(2, result.Strain.Value);
      Assert.Equal(1, result.Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsStressPointPointFromPointGoo() {
      var point = new Point3d() {
        X = 1,
        Y = 2,
        Z = 3,
      };
      var objwrap = new GH_ObjectWrapper(new AdSecStressStrainPointGoo(point));
      var result = AdSecInput.TryCastToStressStrainPoint(component, 0, objwrap);

      Assert.NotNull(result);
      Assert.Equal(1, result.Strain.Value);
      Assert.Equal(2000000, result.Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsStressPointPointFromPoint3d() {
      var point = new Point3d() {
        X = 3,
        Y = 1,
        Z = 2,
      };
      var objwrap = new GH_ObjectWrapper(point);
      var result = AdSecInput.TryCastToStressStrainPoint(component, 0, objwrap);

      Assert.NotNull(result);
      Assert.Equal(3, result.Strain.Value);
      Assert.Equal(1000000, result.Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsNull() {
      var objwrap = new GH_ObjectWrapper(null);
      var result = AdSecInput.TryCastToStressStrainPoint(component, 0, objwrap);

      Assert.Null(result);
    }

    [Fact]
    public void StressStrainPointsReturnsNull() {
      var result = AdSecInput.StressStrainPoints(null, dataAccess.Object, 0, true);
      Assert.Null(result);
    }

    [Fact]
    public void StressStrainPointsReturnsWarningWhenInputIsNonOptional() {
      var result = AdSecInput.StressStrainPoints(component, dataAccess.Object, 0);
      Assert.Null(result);

      var runtimeWarnings = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeWarnings);
      Assert.Contains("failed", runtimeWarnings.First());
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsWarningWhenEmptyPoints() {
      var objwrap = new List<GH_ObjectWrapper>();
      var result = AdSecInput.TryCastToStressStrainPoints(component, 0, objwrap);
      Assert.Null(result);

      var runtimeMessages = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeMessages);
      Assert.Contains("Input must contain at least 2 points", runtimeMessages.First());
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsWarningWhenLessThan2Points() {
      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(null),
      };
      var result = AdSecInput.TryCastToStressStrainPoints(component, 0, objwrap);
      Assert.Null(result);

      var runtimeMessages = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeMessages);
      Assert.Contains("Input must contain at least 2 points", runtimeMessages.First());
    }

    [Fact]
    public void TryCastToStressStrainsPointReturnsNull() {
      var objwrap = new GH_ObjectWrapper(null);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objwrap,
        objwrap,
      };
      var result = AdSecInput.TryCastToStressStrainPoints(component, 0, objectWrappers);

      Assert.Null(result);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsErrorWhenCantConvert() {
      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(null),
      };
      var result = AdSecInput.TryCastToStressStrainPoints(component, 0, objwrap);
      Assert.Null(result);

      var runtimeMessages = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeMessages);
      Assert.Contains("StressStrainPoint or a Polyline", runtimeMessages.First());
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsSamePoints() {
      var crv = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      var objwrap = new List<GH_ObjectWrapper> {
        new GH_ObjectWrapper(crv),
        new GH_ObjectWrapper(crv),
      };
      var result = AdSecInput.TryCastToStressStrainPoints(component, 0, objwrap);

      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal(2, result.First().Strain.Value);
      Assert.Equal(2, result.Last().Strain.Value);
      Assert.Equal(1, result.First().Stress.Value);
      Assert.Equal(1, result.Last().Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsStressPointPointsFromPointGoos() {
      var point = new Point3d() {
        X = 1,
        Y = 2,
        Z = 3,
      };
      var objwrap = new GH_ObjectWrapper(new AdSecStressStrainPointGoo(point));
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objwrap,
        objwrap,
        objwrap,
      };
      var result = AdSecInput.TryCastToStressStrainPoints(component, 0, objectWrappers);

      Assert.NotNull(result);
      Assert.Equal(3, result.Count);
      Assert.Equal(1, result.First().Strain.Value);
      Assert.Equal(1, result.Last().Strain.Value);
      Assert.Equal(2000000, result.First().Stress.Value);
      Assert.Equal(2000000, result.Last().Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsStressPointsFromPoints3d() {
      var point = new Point3d() {
        X = 3,
        Y = 1,
        Z = 2,
      };
      var objwrap = new GH_ObjectWrapper(point);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objwrap,
        objwrap,
      };
      var result = AdSecInput.TryCastToStressStrainPoints(component, 0, objectWrappers);

      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal(3, result.First().Strain.Value);
      Assert.Equal(3, result.Last().Strain.Value);
      Assert.Equal(1000000, result.First().Stress.Value);
      Assert.Equal(1000000, result.Last().Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsStressPointsFromCurves() {
      IStressStrainCurve crv = ILinearStressStrainCurve.Create(
        IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
      var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, false);

      var objwrap = new GH_ObjectWrapper(tuple.Item1);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objwrap,
      };
      var result = AdSecInput.TryCastToStressStrainPoints(component, 0, objectWrappers);

      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal(0, result.First().Strain.Value);
      Assert.Equal(-1, result.Last().Strain.Value);
      Assert.Equal(0, result.First().Stress.Value);
      Assert.Equal(0, result.Last().Stress.Value);
    }

  }
}
