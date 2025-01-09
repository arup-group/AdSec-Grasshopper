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
    private CreateStressStrainPoint _component;
    private Mock<IGH_DataAccess> _dataAccess;
    private AdSecStressStrainCurveGoo _curveGoo;
    private IStressStrainPoint _stressStrainPoint;
    private Oasys.Collections.IList<IStressStrainPoint> _stressStrainPoints;

    public AdSecInputTests() {
      _component = new CreateStressStrainPoint();
      _dataAccess = new Mock<IGH_DataAccess>();
      _curveGoo = null;
      _stressStrainPoint = null;
      _stressStrainPoints = Oasys.Collections.IList<IStressStrainPoint>.Create();
    }

    [Fact]
    public void StressStrainCurveGooReturnsNull() {
      var result = AdSecInput.StressStrainCurveGoo(null, _dataAccess.Object, 0, false, true);
      Assert.Null(result);
    }

    [Fact]
    public void StressStrainCurveGooReturnsWarningWhenInputIsNonOptional() {
      var result = AdSecInput.StressStrainCurveGoo(_component, _dataAccess.Object, 0, false);
      Assert.Null(result);

      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeWarnings);
      Assert.Contains("failed", runtimeWarnings.First());
    }

    [Fact]
    public void TryCastToStressStrainCurveReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(false, objwrap, ref _curveGoo);

      Assert.False(castSuccessful);
      Assert.Null(_curveGoo);
    }

    [Fact]
    public void TryCastToStressStrainCurveReturnsGooFromPolyline() {
      IStressStrainCurve crv = ILinearStressStrainCurve.Create(
        IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
      var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, false);

      var objwrap = new GH_ObjectWrapper(tuple.Item1);
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(false, objwrap, ref _curveGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_curveGoo);
      Assert.True(_curveGoo.Value.IsPolyline());
      Assert.Equal(2, _curveGoo.ControlPoints.Count);
    }

    [Fact]
    public void TryCastToStressStrainCurveReturnsGooFromAnotherGoo() {
      var exCrv = IExplicitStressStrainCurve.Create();
      var tuple = AdSecStressStrainCurveGoo.Create(exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit,
        false);
      var goo = new AdSecStressStrainCurveGoo(tuple.Item1, exCrv,
        AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);

      var objwrap = new GH_ObjectWrapper(goo);
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(false, objwrap, ref _curveGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_curveGoo);
      Assert.Null(_curveGoo.Value);
      Assert.Empty(_curveGoo.ControlPoints);
    }

    [Fact]
    public void StressStrainPointReturnsNull() {
      var result = AdSecInput.StressStrainPoint(null, _dataAccess.Object, 0, true);
      Assert.Null(result);
    }

    [Fact]
    public void StressStrainPointReturnsWarningWhenInputIsNonOptional() {
      var result = AdSecInput.StressStrainPoint(_component, _dataAccess.Object, 0);
      Assert.Null(result);

      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeWarnings);
      Assert.Contains("failed", runtimeWarnings.First());
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objwrap, ref _stressStrainPoint);
      Assert.False(castSuccessful);
      Assert.Null(_stressStrainPoint);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsSamePoint() {
      var crv = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      var objwrap = new GH_ObjectWrapper(crv);
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objwrap, ref _stressStrainPoint);

      Assert.True(castSuccessful);
      Assert.NotNull(_stressStrainPoint);
      Assert.Equal(2, _stressStrainPoint.Strain.Value);
      Assert.Equal(1, _stressStrainPoint.Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsStressPointPointFromPointGoo() {
      var point = new Point3d() {
        X = 1,
        Y = 2,
        Z = 3,
      };
      var objwrap = new GH_ObjectWrapper(new AdSecStressStrainPointGoo(point));
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objwrap, ref _stressStrainPoint);

      Assert.True(castSuccessful);
      Assert.NotNull(_stressStrainPoint);
      Assert.Equal(1, _stressStrainPoint.Strain.Value);
      Assert.Equal(2000000, _stressStrainPoint.Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsStressPointPointFromPoint3d() {
      var point = new Point3d() {
        X = 3,
        Y = 1,
        Z = 2,
      };
      var objwrap = new GH_ObjectWrapper(point);
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objwrap, ref _stressStrainPoint);

      Assert.True(castSuccessful);
      Assert.NotNull(_stressStrainPoint);
      Assert.Equal(3, _stressStrainPoint.Strain.Value);
      Assert.Equal(1000000, _stressStrainPoint.Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsNull() {
      var objwrap = new GH_ObjectWrapper(null);
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objwrap, ref _stressStrainPoint);

      Assert.False(castSuccessful);
      Assert.Null(_stressStrainPoint);
    }

    [Fact]
    public void StressStrainPointsReturnsNull() {
      var result = AdSecInput.StressStrainPoints(_component, _dataAccess.Object, 0, true);
      Assert.Null(result);
    }

    [Fact]
    public void StressStrainPointsReturnsWarningWhenInputIsNonOptional() {
      var result = AdSecInput.StressStrainPoints(_component, _dataAccess.Object, 0);
      Assert.Null(result);

      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Equal(2, runtimeWarnings.Count);
      Assert.Contains("must contain at least 2 points", runtimeWarnings.First());
      Assert.Contains("failed", runtimeWarnings.Last());
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsFalseWhenEmptyPoints() {
      var objwrap = new List<GH_ObjectWrapper>();
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objwrap, ref _stressStrainPoints);
      Assert.False(castSuccessful);
      Assert.Empty(_stressStrainPoints);
    }

    [Fact]
    public void TryCastToStressStrainsPointReturnsNull() {
      var objwrap = new GH_ObjectWrapper(null);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objwrap,
        objwrap,
      };
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);

      Assert.False(castSuccessful);
      Assert.Empty(_stressStrainPoints);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsFalseWhenCantConvert() {
      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(null),
      };
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objwrap, ref _stressStrainPoints);
      Assert.False(castSuccessful);
      Assert.Empty(_stressStrainPoints);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsFalseWhenNullPoints() {
      var objwrap = new List<GH_ObjectWrapper>();
      _stressStrainPoints = null;
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objwrap, ref _stressStrainPoints);
      Assert.False(castSuccessful);
      Assert.Null(_stressStrainPoints);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsSamePoints() {
      var crv = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      var objwrap = new List<GH_ObjectWrapper> {
        new GH_ObjectWrapper(crv),
        new GH_ObjectWrapper(crv),
      };
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objwrap, ref _stressStrainPoints);

      Assert.True(castSuccessful);
      Assert.NotEmpty(_stressStrainPoints);
      Assert.Equal(2, _stressStrainPoints.Count);
      Assert.Equal(2, _stressStrainPoints.First().Strain.Value);
      Assert.Equal(2, _stressStrainPoints.Last().Strain.Value);
      Assert.Equal(1, _stressStrainPoints.First().Stress.Value);
      Assert.Equal(1, _stressStrainPoints.Last().Stress.Value);
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
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);

      Assert.True(castSuccessful);
      Assert.NotEmpty(_stressStrainPoints);
      Assert.Equal(3, _stressStrainPoints.Count);
      Assert.Equal(1, _stressStrainPoints.First().Strain.Value);
      Assert.Equal(1, _stressStrainPoints.Last().Strain.Value);
      Assert.Equal(2000000, _stressStrainPoints.First().Stress.Value);
      Assert.Equal(2000000, _stressStrainPoints.Last().Stress.Value);
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
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);

      Assert.True(castSuccessful);
      Assert.NotEmpty(_stressStrainPoints);
      Assert.Equal(2, _stressStrainPoints.Count);
      Assert.Equal(3, _stressStrainPoints.First().Strain.Value);
      Assert.Equal(3, _stressStrainPoints.Last().Strain.Value);
      Assert.Equal(1000000, _stressStrainPoints.First().Stress.Value);
      Assert.Equal(1000000, _stressStrainPoints.Last().Stress.Value);
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
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);

      Assert.True(castSuccessful);
      Assert.NotEmpty(_stressStrainPoints);
      Assert.Equal(2, _stressStrainPoints.Count);
      Assert.Equal(0, _stressStrainPoints.First().Strain.Value);
      Assert.Equal(-1, _stressStrainPoints.Last().Strain.Value);
      Assert.Equal(0, _stressStrainPoints.First().Stress.Value);
      Assert.Equal(0, _stressStrainPoints.Last().Stress.Value);
    }

  }
}
