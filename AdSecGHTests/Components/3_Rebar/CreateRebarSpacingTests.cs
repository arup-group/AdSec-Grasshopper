using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._3_Rebar {
  [Collection("GrasshopperFixture collection")]
  public class CreateRebarSpacingTests {
    private readonly CreateRebarSpacing _component;

    public CreateRebarSpacingTests() {
      _component = new CreateRebarSpacing();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.RebarSpacing));
    }

    [Fact]
    public void ShouldUpdateNameWithUnits() {
      _component.SetSelected(0, 0); // Distance
      _component.SetSelected(1, 0); // m
      Assert.Contains("[m]", _component.BusinessComponent.Spacing.Name);
    }

    [Fact(Skip = "Not implemented yet")]
    public void ShouldUpdateNameWithUnitsWithNonDefaultUnits() {
      _component.SetSelected(0, 0); // Distance
      _component.SetSelected(1, 1); // mm
      Assert.Contains("[mm]", _component.BusinessComponent.Spacing.Name);
    }

    [Fact]
    public void ShouldHaveDynamicDropdownItems() {
      _component.SetSelected(0, 1); // Count
      Assert.Single(_component.DropDownItems);
    }
  }
}
