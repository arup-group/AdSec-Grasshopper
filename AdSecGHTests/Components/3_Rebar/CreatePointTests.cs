using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components {

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
      var geo = componentBusinessComponent.LengthUnitGeometry;
      Assert.Equal("Y [mm]", componentBusinessComponent.Y.Name);
      Assert.Equal("Z [mm]", componentBusinessComponent.Z.Name);
    }

    [Fact]
    public void ShouldBeConsistentIfMultipleTimesDropdownIsChanged() {
      component.SetSelected(0, 0);
      component.SetSelected(0, 1);
      component.SetSelected(0, 0);
      var componentBusinessComponent = component.BusinessComponent;
      Assert.Equal("Y [mm]", componentBusinessComponent.Y.Name);
      Assert.Equal("Z [mm]", componentBusinessComponent.Z.Name);
    }

    [Fact]
    public void ShouldWorkWithNonDefaultUnits() {
      component.SetSelected(0, 0); // mm
      ComponentTesting.ComputeOutputs(component);
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(component.MatchesExpectedIcon(Resources.CreateVertexPoint));
    }
  }
}
