using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.GUI.Base;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

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
    public void ShouldHaveTwoDropdowns() {
      Assert.Equal(2, component.DropDownItems.Count);
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

    [Fact]
    public void ShouldNotDisconnectSourceWhenChanging() {
      var slider = GetSlider();
      component.Params.Input[4].AddSource(slider);
      component.SetSelected(0, 1); // Perimeter
      ComponentTesting.ComputeOutputs(component);
      Assert.Single(component.Params.Input[1].Sources);
    }

    private static GH_NumberSlider GetSlider() {
      var slider = new GH_NumberSlider();
      slider.CreateAttributes();
      slider.Slider.Type = GH_SliderAccuracy.Float;
      slider.Slider.Minimum = 0;
      slider.Slider.Maximum = 10;
      slider.Slider.Value = 1;
      return slider;
    }
  }
}
