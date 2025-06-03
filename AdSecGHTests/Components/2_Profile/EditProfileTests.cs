using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._2_Profile {
  [Collection("GrasshopperFixture collection")]
  public class EditProfileTests {
    private readonly EditProfile _component;

    public EditProfileTests() {
      _component = new EditProfile();
    }

    [Fact]
    public void ShouldHaveBusinessComponent() {
      Assert.NotNull(_component.BusinessComponent);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.EditProfile));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveFourInputs() {
      Assert.Equal(4, _component.Params.Input.Count);
    }

    [Fact]
    public void ShouldUpdateNameWhenChangingDropdownToRad() {
      _component.SetSelected(0, 1);
      Assert.Contains("°", _component.Params.Input[1].Name);
    }
  }
}
