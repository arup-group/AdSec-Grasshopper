using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests_AdSecMaterial {
    private AdSecMaterial _adSecMaterial;

    public AdSecInputTests_AdSecMaterial() {
      _adSecMaterial = null;
    }

    [Fact]
    public void TryCastToAdSecMaterialReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecMaterial(objwrap, ref _adSecMaterial);

      Assert.False(castSuccessful);
      Assert.Null(_adSecMaterial);
    }

    [Fact]
    public void TryCastToAdSecMaterialReturnsDesignCode() {
      var materialDesign = new MaterialDesign() {
        Material = Concrete.IS456.Edition_2000.M10,
        DesignCode = new DesignCode() {
          IDesignCode = IS456.Edition_2000,
          DesignCodeName = "IS456 Edition 2000",
        },
        GradeName = "test"
      };
      var objwrap = new GH_ObjectWrapper(new AdSecMaterialGoo(materialDesign));
      bool castSuccessful = AdSecInput.TryCastToAdSecMaterial(objwrap, ref _adSecMaterial);

      Assert.True(castSuccessful);
      Assert.NotNull(_adSecMaterial);
      Assert.True(_adSecMaterial.IsValid);
      Assert.Equal("test", _adSecMaterial.GradeName);
    }
  }
}
