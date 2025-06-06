﻿using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components {

  [Collection("GrasshopperFixture collection")]
  public class CreateSubComponentTests {
    private readonly CreateSubComponent component;

    public CreateSubComponentTests() {
      component = new CreateSubComponent();
      component.SetInputParamAt(0, SectionExtension.AdSecSectionGooSample());
      component.SetInputParamAt(1, IPointBuilder.InMillimeters());
    }

    [Fact]
    public void ShouldHaveTwoInputs() {
      Assert.Equal(2, component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(component.Params.Output);
    }

    [Fact]
    public void ShouldHaveNoWarnings() {
      ComponentTestHelper.ComputeData(component);
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
    }

    [Fact]
    public void ShouldTakePlaneFromSectionInput() {
      ComponentTestHelper.ComputeData(component);
      var data = component.GetValue<AdSecSubComponentGoo>();
      Assert.Equal(OasysPlane.PlaneXY, data.section.LocalPlane.ToOasys());
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      ComponentTestHelper.ComputeData(component);

      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldProduceAnOutput() {
      ComponentTestHelper.ComputeData(component);
      var data = component.GetOutputParamAt(0);
      Assert.NotNull(ComponentTestHelper.GetOutput(component));
    }
  }
}
