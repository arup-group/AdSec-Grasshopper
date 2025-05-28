using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecMaterialGooTests {
    AdSecMaterialGoo _materialGoo;
    private readonly IDesignCode DesignCode = IS456.Edition_2000;
    private readonly IConcrete material = Concrete.IS456.Edition_2000.M10;

    public AdSecMaterialGooTests() {
      var materialDesign = new MaterialDesign {
        Material = material,
        DesignCode = new DesignCode() {
          IDesignCode = DesignCode,
          DesignCodeName = "IS456 Edition 2000",
        },
      };
      _materialGoo = new AdSecMaterialGoo(materialDesign);
    }

    [Fact]
    public void ShouldIncludeTheNameOfTheType() {
      Assert.Contains("AdSec Material", _materialGoo.ToString());
    }

    [Fact]
    public void ShouldIncludeTheDesignCode() {
      Assert.Contains(_materialGoo.Material.DesignCodeName, _materialGoo.ToString());
    }

    [Fact]
    public void ShouldBeSurroundedByParenthesis() {
      string actualString = _materialGoo.ToString();
      actualString = actualString.Replace("AdSec Material ", string.Empty).Trim();
      Assert.StartsWith("(", actualString);
      Assert.EndsWith(")", actualString);
    }
  }
}
