using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._3_Rebar {
  [Collection("GrasshopperFixture collection")]
  public class CreateRebarSpacingTests {

    public CreateRebarSpacing component;

    public CreateRebarSpacingTests() {
      component = new CreateRebarSpacing();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(component.MatchesExpectedIcon(Resources.RebarSpacing));
    }
  }
}
