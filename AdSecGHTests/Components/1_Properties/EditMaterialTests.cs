using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._02_Properties {
  [Collection("GrasshopperFixture collection")]
  public class EditMaterialTests {
    private readonly EditMaterial _component;

    public EditMaterialTests() {
      _component = new EditMaterial();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.EditMaterial));
    }
  }
}
