using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.StandardMaterials;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecRebarBundleGooTests {
    private AdSecRebarBundleGoo _bundleGoo;
    private bool showRemark = false;

    public AdSecRebarBundleGooTests() {
      _bundleGoo = null;
    }

    [Fact]
    public void TryCastToAdSecRebarBundleGooReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarBundleGoo(objwrap, ref _bundleGoo, ref showRemark);

      Assert.False(castSuccessful);
      Assert.Null(_bundleGoo);
    }

    [Fact]
    public void TryCastToAdSecRebarBundleGooReturnsRebarBundleGooFromAdSecRebarBundleGoo() {
      var stressStrainPoint
        = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio));
      IStressStrainCurve curve = ILinearStressStrainCurve.Create(stressStrainPoint);
      var tensionCompressionCurve = ITensionCompressionCurve.Create(curve, curve);
      var length = new Length(1, LengthUnit.Meter);
      var bundle = IBarBundle.Create(IReinforcement.Create(tensionCompressionCurve, tensionCompressionCurve), length,
        1);

      var adSecRebarBundleGoo = new AdSecRebarBundleGoo(bundle, string.Empty);

      var objwrap = new GH_ObjectWrapper(adSecRebarBundleGoo);
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarBundleGoo(objwrap, ref _bundleGoo, ref showRemark);

      Assert.True(castSuccessful);
      Assert.False(showRemark);
      Assert.NotNull(_bundleGoo);
      Assert.True(_bundleGoo.IsValid);
    }

    [Fact]
    public void TryCastToAdSecRebarBundleGooReturnsRebarBundleGooFromAdSecRebarLayerGoo() {
      var topReinforcementLayer = ILayerByBarCount.Create(2,
        IBarBundle.Create(Reinforcement.Steel.IS456.Edition_2000.S415, Length.FromMillimeters(20)));
      var adSecRebarLayerGoo = new AdSecRebarLayerGoo(topReinforcementLayer, string.Empty);

      var objwrap = new GH_ObjectWrapper(adSecRebarLayerGoo);
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarBundleGoo(objwrap, ref _bundleGoo, ref showRemark);

      Assert.True(castSuccessful);
      Assert.True(showRemark);
      Assert.NotNull(_bundleGoo);
      Assert.True(_bundleGoo.IsValid);
    }
  }
}
