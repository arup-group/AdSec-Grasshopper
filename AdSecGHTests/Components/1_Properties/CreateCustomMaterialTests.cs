using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._02_Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateCustomMaterialTests {
    private readonly CreateCustomMaterial _component;

    public CreateCustomMaterialTests() {
      _component = new CreateCustomMaterial();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.CreateCustomMaterial));
    }
  }
}
