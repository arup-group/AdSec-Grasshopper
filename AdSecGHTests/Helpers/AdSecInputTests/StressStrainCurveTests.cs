using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests_StressStrainCurveTests {
    private AdSecStressStrainCurveGoo _curveGoo;

    public AdSecInputTests_StressStrainCurveTests() {
      _curveGoo = null;
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
      var tuple = AdSecStressStrainCurveGoo.Create(crv, false);

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
      var tuple = AdSecStressStrainCurveGoo.Create(curve, false);
      var goo = new AdSecStressStrainCurveGoo(tuple.Item1, curve, tuple.Item2);

      var objectWrapper = new GH_ObjectWrapper(goo);
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(false, objectWrapper, ref _curveGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_curveGoo);
      Assert.Null(_curveGoo.Value);
      Assert.Empty(_curveGoo.ControlPoints);
    }
  }
}
