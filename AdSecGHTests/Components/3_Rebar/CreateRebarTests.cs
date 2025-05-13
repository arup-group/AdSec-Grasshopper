using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._3_Rebar {
  [Collection("GrasshopperFixture collection")]
  public class CreateRebarTests {

    public CreateRebar component;

    public CreateRebarTests() {
      component = new CreateRebar();
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(component.Params.Output);
    }

    [Fact]
    public void ShouldHaveThreeInputsForOtherMode() {
      component.SetSelected(0, 1);
      Assert.Equal(3, component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveTwoDropDowns() {
      Assert.Equal(2, component.DropDownItems.Count);
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, component.PluginInfo);
    }
    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(component.MatchesExpectedIcon(Resources.Rebar));
    }
  }
}
