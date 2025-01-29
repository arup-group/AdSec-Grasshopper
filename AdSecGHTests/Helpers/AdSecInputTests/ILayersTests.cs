using System.Collections.Generic;

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
  public class AdSecInputTests_ILayersTests {
    private Oasys.Collections.IList<ILayer> _layers;
    private List<int> invalidIds;

    public AdSecInputTests_ILayersTests() {
      _layers = Oasys.Collections.IList<ILayer>.Create();
      invalidIds = new List<int>();
    }

    [Fact]
    public void TryCastToILayersReturnsFalseWhenInvalidIdsListIsNullInitialised() {
      bool castSuccessful = AdSecInput.TryCastToILayers(null, _layers, null);

      Assert.False(castSuccessful);
      Assert.Empty(_layers);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToILayersReturnsFalseWhenNull() {
      bool castSuccessful = AdSecInput.TryCastToILayers(null, _layers, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_layers);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToILayersReturnsFalseWhenEmptySections() {
      var objectWrappers = new List<GH_ObjectWrapper>();
      bool castSuccessful = AdSecInput.TryCastToILayers(objectWrappers, _layers, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_layers);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToILayersReturnsFalseWhenValueIsNull() {
      ILayer layer = null;
      var objectWrappers = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(layer),
      };
      bool castSuccessful = AdSecInput.TryCastToILayers(objectWrappers, _layers, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_layers);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
    }

    [Fact]
    public void TryCastToILayersReturnsEmptyWhenNullItemSections() {
      var objectWrapper = new GH_ObjectWrapper(null);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objectWrapper,
      };
      bool castSuccessful = AdSecInput.TryCastToILayers(objectWrappers, _layers, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_layers);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
    }

    [Fact]
    public void TryCastToILayersReturnsFalseWhenSecondItemIncorrect() {
      var layer = GetLayer();
      var objectWrappers = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(layer),
        new GH_ObjectWrapper(null),
      };
      bool castSuccessful = AdSecInput.TryCastToILayers(objectWrappers, _layers, invalidIds);

      Assert.False(castSuccessful);
      Assert.Single(_layers);
      Assert.Single(invalidIds);
      Assert.Equal(1, invalidIds[0]);
    }

    [Fact]
    public void TryCastToILayersReturnsCorrectDataFromRebarLayerGoo() {
      var layer = GetLayer();
      var input = new AdSecRebarLayerGoo(layer);

      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(input),
      };
      bool castSuccessful = AdSecInput.TryCastToILayers(objwrap, _layers, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_layers);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToILayersReturnsCorrectDataFromILayer() {
      var input = GetLayer();

      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(input),
      };
      bool castSuccessful = AdSecInput.TryCastToILayers(objwrap, _layers, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_layers);
      Assert.Empty(invalidIds);
    }

    private static ILayerByBarCount GetLayer() {
      var layer = ILayerByBarCount.Create(2,
        IBarBundle.Create(Reinforcement.Steel.IS456.Edition_2000.S415, Length.FromMillimeters(1)));
      return layer;
    }
  }
}
