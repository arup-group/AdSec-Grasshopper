using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec.DesignCode;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecDesignCodeGooTests {
    private readonly AdSecDesignCodeGoo _designCodeGoo;
    private readonly IDesignCode _designCode = IS456.Edition_2000;

    public AdSecDesignCodeGooTests() {
      var designCode = new DesignCode() {
        IDesignCode = _designCode,
        DesignCodeName = "IS456 Edition 2000",
      };
      _designCodeGoo = new AdSecDesignCodeGoo(designCode);
    }

    [Fact]
    public void ShouldHaveCorrectDesignCode() {
      Assert.Contains("IS456 Edition 2000", _designCodeGoo.ToString());
    }

    [Fact]
    public void ShouldIncludeTheNameOfTheType() {
      Assert.Contains("AdSec DesignCode", _designCodeGoo.ToString());
    }

    [Fact]
    public void ShouldBeSurroundedByParenthesis() {
      string actualString = _designCodeGoo.ToString();
      actualString = actualString.Replace("AdSec DesignCode ", string.Empty).Trim();
      Assert.StartsWith("(", actualString);
      Assert.EndsWith(")", actualString);
    }
  }
}
