using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._2_Profile {
  [Collection("GrasshopperFixture collection")]
  public class EditProfileTests {
    private readonly EditProfile _component;

    public EditProfileTests() {
      _component = new EditProfile();
      ProfileDesign profile = ProfileDesign.From(new SectionDesign() {
        Section = new SectionBuilder().WithHeight(1).WithWidth(1).Build()
      });
      _component.SetInputParamAt(0, new AdSecProfileGoo(profile));
      ComponentTestHelper.ComputeData(_component);
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

    [Fact]
    public void ShouldHaveNoWarnings() {
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldBeAbleToGetPlaneWithNoInputs() {
      Assert.NotNull(_component.GetValue<AdSecProfileGoo>());
    }
  }
}
