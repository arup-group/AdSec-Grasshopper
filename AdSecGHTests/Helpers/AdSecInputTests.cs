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
    public void TryCastToStressStrainCurveReturnsFalseWhenCantConvert() {
      var objectWrapper = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(false, objectWrapper, ref _curveGoo);

      Assert.False(castSuccessful);
      Assert.Null(_curveGoo);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TryCastToStressStrainCurveReturnsGooFromPolylineCompressionIs(bool compression) {
      IStressStrainCurve crv = ILinearStressStrainCurve.Create(
        IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
      var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, false);

      var objectWrapper = new GH_ObjectWrapper(tuple.Item1);
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(compression, objectWrapper, ref _curveGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_curveGoo);
      Assert.True(_curveGoo.Value.IsPolyline());
      Assert.Equal(2, _curveGoo.ControlPoints.Count);
    }

    [Fact]
    public void TryCastToStressStrainCurveReturnsGooFromAnotherGoo() {
      var curve = IExplicitStressStrainCurve.Create();
      var tuple = AdSecStressStrainCurveGoo.Create(curve, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit,
        false);
      var goo = new AdSecStressStrainCurveGoo(tuple.Item1, curve,
        AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);

      var objectWrapper = new GH_ObjectWrapper(goo);
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(false, objectWrapper, ref _curveGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_curveGoo);
      Assert.Null(_curveGoo.Value);
      Assert.Empty(_curveGoo.ControlPoints);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsFalseWhenCantConvert() {
      var objectWrapper = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objectWrapper, ref _stressStrainPoint);
      Assert.False(castSuccessful);
      Assert.Null(_stressStrainPoint);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsSamePoint() {
      var point = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      var objectWrapper = new GH_ObjectWrapper(point);
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objectWrapper, ref _stressStrainPoint);

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
      var objectWrapper = new GH_ObjectWrapper(new AdSecStressStrainPointGoo(point));
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objectWrapper, ref _stressStrainPoint);

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
      var objectWrapper = new GH_ObjectWrapper(point);
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objectWrapper, ref _stressStrainPoint);

      Assert.True(castSuccessful);
      Assert.NotNull(_stressStrainPoint);
      Assert.Equal(3, _stressStrainPoint.Strain.Value);
      Assert.Equal(1000000, _stressStrainPoint.Stress.Value);
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsNull() {
      var objectWrapper = new GH_ObjectWrapper(null);
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objectWrapper, ref _stressStrainPoint);

      Assert.False(castSuccessful);
      Assert.Null(_stressStrainPoint);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsFalseWhenEmptyPoints() {
      var objectWrappers = new List<GH_ObjectWrapper>();
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);
      Assert.False(castSuccessful);
      Assert.Empty(_stressStrainPoints);
    }

    [Fact]
    public void TryCastToStressStrainsPointReturnsNull() {
      var objectWrapper = new GH_ObjectWrapper(null);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objectWrapper,
        objectWrapper,
      };
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);

      Assert.False(castSuccessful);
      Assert.Empty(_stressStrainPoints);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsFalseWhenCantConvert() {
      var objectWrappers = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(null),
      };
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);
      Assert.False(castSuccessful);
      Assert.Empty(_stressStrainPoints);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsFalseWhenNullPoints() {
      var objectWrappers = new List<GH_ObjectWrapper>();
      _stressStrainPoints = null;
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);
      Assert.False(castSuccessful);
      Assert.Null(_stressStrainPoints);
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsSamePoints() {
      var point = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      var objectWrappers = new List<GH_ObjectWrapper> {
        new GH_ObjectWrapper(point),
        new GH_ObjectWrapper(point),
      };
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoints(objectWrappers, ref _stressStrainPoints);

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
      var objectWrapper = new GH_ObjectWrapper(new AdSecStressStrainPointGoo(point));
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objectWrapper,
        objectWrapper,
        objectWrapper,
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
      var objectWrapper = new GH_ObjectWrapper(point);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objectWrapper,
        objectWrapper,
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
      IStressStrainCurve curve = ILinearStressStrainCurve.Create(
        IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
      var tuple = AdSecStressStrainCurveGoo.Create(curve, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear,
        false);

      var objectWrapper = new GH_ObjectWrapper(tuple.Item1);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objectWrapper,
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
