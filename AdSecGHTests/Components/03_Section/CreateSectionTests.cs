using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;

using AdSecGHTests.Helpers;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;
using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateSectionTests {
    readonly CreateSection component;

    public CreateSectionTests() {
      component = new CreateSection();
      component.SetInputParamAt(0, new ProfileDesign() {
        Profile = new ProfileBuilder().WidthDepth(1).WithWidth(2).Build(),
        LocalPlane = OasysPlane.PlaneYZ,
      });
      component.SetInputParamAt(1, new MaterialDesign() {
        Material = Concrete.IS456.Edition_2000.M10,
        DesignCode = IS456.Edition_2000
      });
    }

    [Fact]
    public void ShouldHaveAnInputWithoutOptional() {
      ComponentTesting.ComputeOutputs(component);
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void shouldHaveFourInputs() {
      Assert.Equal(4, component.Params.Input.Count);
    }

    [Fact]
    public void shouldHaveOneOutput() {
      Assert.Single(component.Params.Output);
    }
  }
}
