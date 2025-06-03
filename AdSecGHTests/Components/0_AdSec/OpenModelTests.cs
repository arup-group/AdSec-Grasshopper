using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._0_AdSec {
  [Collection("GrasshopperFixture collection")]
  public class OpenModelTests {
    private readonly OpenModel _component;

    public OpenModelTests() {
      _component = new OpenModel();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.OpenAdSec));
    }
  }
}
