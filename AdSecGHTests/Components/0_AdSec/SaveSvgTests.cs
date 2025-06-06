using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._0_AdSec {
  [Collection("GrasshopperFixture collection")]
  public class SaveSvgTests {
    private readonly SaveSvg _component;

    public SaveSvgTests() {
      _component = new SaveSvg();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.SVG));
    }
  }
}
