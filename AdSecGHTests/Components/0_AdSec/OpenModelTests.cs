using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

[Collection("GrasshopperFixture collection")]
public class OpenModelTests {
  private OpenModel _component;

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

  [Fact]
  public void ShouldInitializeAttributes() {
    _component.Attributes = null;
    _component.CreateAttributes();
    Assert.NotNull(_component.Attributes);
  }
}
