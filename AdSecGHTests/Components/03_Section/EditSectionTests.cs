using System.Linq;

using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

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
    public void ShouldHaveNoWarnings() {
      var component = new EditSection();
      var section = new AdSecSectionGoo(new AdSecSection(SampleData.GetSectionDesign()));
      component.SetInputParamAt(0, section);
      ComponentTesting.ComputeOutputs(component);
      var runtimeMessages = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Empty(runtimeMessages);
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

    [Fact]
    public void ShouldHaveSixOutput() {
      var component = new EditSection();
      Assert.Equal(7, component.Params.Output.Count);
    }
  }
}
