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
  public class AdSecInputTests_StressStrainPointTests {
    private IStressStrainPoint _stressStrainPoint;

    public AdSecInputTests_StressStrainPointTests() {
      _stressStrainPoint = null;
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
      Assert.Equal(1, _stressStrainPoint.Strain.As(DefaultUnits.StrainUnitResult));
      Assert.Equal(2, _stressStrainPoint.Stress.As(DefaultUnits.StressUnitResult));
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
      Assert.Equal(3, _stressStrainPoint.Strain.As(DefaultUnits.StrainUnitResult));
      Assert.Equal(1, _stressStrainPoint.Stress.As(DefaultUnits.StressUnitResult));
    }

    [Fact]
    public void TryCastToStressStrainPointReturnsNull() {
      var objectWrapper = new GH_ObjectWrapper(null);
      bool castSuccessful = AdSecInput.TryCastToStressStrainPoint(objectWrapper, ref _stressStrainPoint);

      Assert.False(castSuccessful);
      Assert.Null(_stressStrainPoint);
    }

  }
}
