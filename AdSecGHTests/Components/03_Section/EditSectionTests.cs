using AdSecGH.Components;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class EditSectionTests {

    [Fact]
    public void ShouldHaveNoErrors() {
      var component = new EditSection();
      ComponentTesting.ComputeOutputs(component);
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveMetadataWithRightName() {
      var component = new EditSection();
      Assert.Equal("EditSection", component.BusinessComponent.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveSixInputs() {
      var component = new EditSection();
      Assert.Equal(6, component.Params.Input.Count);
    }
  }
}
