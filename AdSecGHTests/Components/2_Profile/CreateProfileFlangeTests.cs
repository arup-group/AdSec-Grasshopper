using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateProfileFlangeTests {

    private readonly CreateProfileFlange _component;

    public CreateProfileFlangeTests() {
      _component = new CreateProfileFlange();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.CreateFlange));
    }
  }
}
