using AdSecGH.Components;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH.Units.Helpers;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreatePointTests {

    private readonly CreatePointGh func;

    public CreatePointTests() {
      func = new CreatePointGh();
    }
  }

  [Collection("GrasshopperFixture collection")]
  public class CreatePointGhTests {
    private readonly CreatePoint component;

    public CreatePointGhTests() {
      component = new CreatePoint();
      component.SetInputParamAt(0, 0);
      component.SetInputParamAt(1, 0);
      ComponentTesting.ComputeOutputs(component);
    }

    [Fact]
    public void ShouldHaveCorrectName() {
      Assert.Equal("Y [m]", component.Params.Input[0].Name);
      Assert.Equal("Z [m]", component.Params.Input[1].Name);
    }

    [Fact]
    public void ShouldUpdateNameWhenUnitsChange() {
      component.SetSelected(0, 0); // mm
      ComponentTesting.ComputeOutputs(component);
      Assert.Equal("Y [mm]", component.Params.Input[0].Name);
      Assert.Equal("Z [mm]", component.Params.Input[1].Name);
    }

    [Fact]
    public void ShouldUpdateAttributeAsWell() {
      component.SetSelected(0, 0); // mm
      var componentBusinessComponent = component.BusinessComponent;
      Assert.Equal("Y [mm]", componentBusinessComponent.Y.Name);
      Assert.Equal("Z [mm]", componentBusinessComponent.Z.Name);
    }

    [Fact]
    public void ShouldWorkWithNonDefaultUnits() {
      string unit = FilteredUnits.FilteredLengthUnits[1]; // mm
      component.SetSelected(0, 0); // mm
      ComponentTesting.ComputeOutputs(component);
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }
  }
}
