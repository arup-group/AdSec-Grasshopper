using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.StandardMaterials;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecRebarLayerGooTests {
    private AdSecRebarLayerGoo _layerGoo;

    public AdSecRebarLayerGooTests() {
      _layerGoo = null;
    }

    [Fact]
    public void TryCastToAdSecRebarLayerGooReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarLayerGoo(objwrap, ref _layerGoo);

      Assert.False(castSuccessful);
      Assert.Null(_layerGoo);
    }

    [Fact]
    public void TryCastToAdSecRebarLayerGooReturnsRebarLayerGoo() {
      var topReinforcementLayer = ILayerByBarCount.Create(2,
        IBarBundle.Create(Reinforcement.Steel.IS456.Edition_2000.S415, Length.FromMillimeters(20)));
      var adSecRebarLayerGoo = new AdSecRebarLayerGoo(topReinforcementLayer);

      var objwrap = new GH_ObjectWrapper(adSecRebarLayerGoo);
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarLayerGoo(objwrap, ref _layerGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_layerGoo);
      Assert.True(_layerGoo.IsValid);
    }
  }
}
