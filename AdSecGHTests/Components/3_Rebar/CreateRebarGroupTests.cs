using AdSecCore.Builders;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.GUI.Base;
using Grasshopper.Kernel.Special;

using Oasys.GH.Helpers;

using OasysGH.Units;

using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components._3_Rebar {

  [Collection("GrasshopperFixture collection")]
  public class CreateRebarGroupTests {
    public CreateReinforcementGroup component;

    public CreateRebarGroupTests() {
      component = new CreateReinforcementGroup();
    }

    [Fact]
    public void ShouldComputeTemplate() {
      component.SetSelected(0, 0); // Template
      component.SetInputParamAt(0, new[] { new BuilderLayer().Build() });

      component.SetInputParamAt(1, new[] { 0.01 });
      Assert.NotNull(component.GetOutputParamAt(0));
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
    public void ShouldHaveTemplateOnTheSelectedUnit() {
      Assert.Equal("Template", component.SelectedItems[0]);
    }

    [Fact]
    public void ShouldHaveCorrectDefaultUnit() {
      var originalUnit = DefaultUnits.LengthUnitGeometry;
      DefaultUnits.LengthUnitGeometry = LengthUnit.Millimeter;
      component = new CreateReinforcementGroup();
      Assert.Equal("mm", component.SelectedItems[1]);
      //restore original unit
      DefaultUnits.LengthUnitGeometry = originalUnit;
    }

    [Fact]
    public void ShouldHaveMetersOnTheSelectedUnit() {
      Assert.Equal("m", component.SelectedItems[1]);
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

    [Fact]
    public void ShouldComputeWhenCoverInputIsString() {
      component.SetSelected(0, 0);
      ComponentTestHelper.SetInput(component, new AdSecRebarLayerGoo(new BuilderLayer().Build()), 0);
      ComponentTestHelper.SetInput(component, "4", 4);
      ComponentTestHelper.ComputeData(component);
      Assert.NotNull(ComponentTestHelper.GetOutput(component));
    }
  }
}
