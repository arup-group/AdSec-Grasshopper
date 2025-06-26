using System;
using System.Collections.Generic;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

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
      var curveGoo = AdSecStressStrainCurveGoo.Create(crv, false);

      var objectWrapper = new GH_ObjectWrapper(curveGoo.Curve);
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(compression, objectWrapper, ref _curveGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_curveGoo);
      Assert.True(_curveGoo.Curve.IsPolyline());
      Assert.Equal(2, _curveGoo.ControlPoints.Count);
    }

    [Fact]
    public void TryCastToStressStrainCurveReturnsGooFromAnotherGoo() {
      var curve = IExplicitStressStrainCurve.Create();
      var curveGoo = AdSecStressStrainCurveGoo.Create(curve, false);
      var goo = new AdSecStressStrainCurveGoo(curveGoo.Curve, curveGoo.Value, curveGoo.ControlPoints);
      var objectWrapper = new GH_ObjectWrapper(goo);
      bool castSuccessful = AdSecInput.TryCastToStressStrainCurve(false, objectWrapper, ref _curveGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_curveGoo);
      Assert.Empty(_curveGoo.ControlPoints);
    }
  }
}
