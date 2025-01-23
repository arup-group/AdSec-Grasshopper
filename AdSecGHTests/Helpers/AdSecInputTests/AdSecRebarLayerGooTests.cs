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
  public class AdSecRebarLayerGooTests {
    private AdSecRebarLayerGoo _layerGoo;

    public AdSecRebarLayerGooTests() {
      _layerGoo = null;
    }

    [Fact]
    public void TryCastToAdSecRebarBundleGooReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarLayerGoo(objwrap, ref _layerGoo);

      Assert.False(castSuccessful);
      Assert.Null(_layerGoo);
    }

    [Fact]
    public void TryCastToAdSecRebarBundleGooReturnsRebarBundleGooFromAdSecRebarLayerGoo() {
      var topReinforcementLayer = ILayerByBarCount.Create(2,
        IBarBundle.Create(Reinforcement.Steel.IS456.Edition_2000.S415, Length.FromMillimeters(20)));
      var adSecRebarBundleGoo = new AdSecRebarLayerGoo(topReinforcementLayer);

      var objwrap = new GH_ObjectWrapper(adSecRebarBundleGoo);
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarLayerGoo(objwrap, ref _layerGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_layerGoo);
      Assert.True(_layerGoo.IsValid);
    }
  }
}
