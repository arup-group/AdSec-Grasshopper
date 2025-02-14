using System.Collections.Generic;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests_StressStrainPointsTests {
    private Oasys.Collections.IList<IStressStrainPoint> _stressStrainPoints;

    public AdSecInputTests_StressStrainPointsTests() {
      _stressStrainPoints = Oasys.Collections.IList<IStressStrainPoint>.Create();
    }

    [Fact]
    public void TryCastToStressStrainPointsReturnsFalseWhenEmptyPoints() {
      var objectWrappers = new List<GH_ObjectWrapper>();
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
      Assert.Equal(1, _stressStrainPoints.First().Strain.As(DefaultUnits.StrainUnitResult), 5);
      Assert.Equal(1, _stressStrainPoints.Last().Strain.As(DefaultUnits.StrainUnitResult), 5);
      Assert.Equal(2, _stressStrainPoints.First().Stress.As(DefaultUnits.StressUnitResult), 5);
      Assert.Equal(2, _stressStrainPoints.Last().Stress.As(DefaultUnits.StressUnitResult), 5);
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
      Assert.Equal(3, _stressStrainPoints.First().Strain.As(DefaultUnits.StrainUnitResult), 5);
      Assert.Equal(3, _stressStrainPoints.Last().Strain.As(DefaultUnits.StrainUnitResult), 5);
      Assert.Equal(1, _stressStrainPoints.First().Stress.As(DefaultUnits.StressUnitResult), 5);
      Assert.Equal(1, _stressStrainPoints.Last().Stress.As(DefaultUnits.StressUnitResult), 5);
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
