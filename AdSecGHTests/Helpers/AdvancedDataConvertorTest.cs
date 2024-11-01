using AdSecGH.Parameters;

using Grasshopper.Kernel.Parameters;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdvancedDataConvertorTest {
    private readonly AllParameters allParameters;
    private readonly AllParametersComponent component;

    public AdvancedDataConvertorTest() {
      allParameters = new AllParameters();
      component = new AllParametersComponent();
      component.SetDefaultValues();
      ComponentTesting.ComputeOutputs(component);
    }

    [Fact]
    public void ShouldBeAbleToCreateIt() {
      Assert.NotNull(component);
    }

    [Fact]
    public void ShouldHaveSectionParameter() {
      Assert.NotNull(component.Params.GetInputParam("Section") as AdSecSectionParameter);
    }

    [Fact]
    public void ShouldHavePointParameter() {
      Assert.NotNull(component.Params.GetInputParam("Points") as Param_GenericObject);
    }
  }
}
