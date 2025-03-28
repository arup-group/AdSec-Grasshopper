using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

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
      var material = new AdSecMaterial() {
        GradeName = "test",
      };
      var materialDesign = new MaterialDesign() {
        Material = material.Material,
        DesignCode = new DesignCode() {
          IDesignCode = material.DesignCode?.DesignCode,
          DesignCodeName = material.DesignCodeName,
        },
        GradeName = material.GradeName,
      };
      var objwrap = new GH_ObjectWrapper(new AdSecMaterialGoo(materialDesign));
      bool castSuccessful = AdSecInput.TryCastToAdSecMaterial(objwrap, ref _adSecMaterial);

      Assert.True(castSuccessful);
      Assert.NotNull(_adSecMaterial);
      Assert.False(_adSecMaterial.IsValid);
      Assert.Equal("test", _adSecMaterial.GradeName);
    }
  }
}
