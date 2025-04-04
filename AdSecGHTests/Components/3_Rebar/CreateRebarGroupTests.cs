using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec.StandardMaterials;
using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._3_Rebar {
  [Collection("GrasshopperFixture collection")]
  public class CreateRebarGroupTests {
    public CreateReinforcementGroup component;

    public CreateRebarGroupTests() {
      component = new CreateReinforcementGroup();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(component.MatchesExpectedIcon(Resources.RebarGroup));
    }

    [Fact]
    public void ShouldUpdateParametersWhenChangingMode() {
      component.SetSelected(0, 1); // Perimeter
      ComponentTesting.ComputeOutputs(component);
      Assert.Equal(2, component.Params.Input.Count);
    }
  }
}
