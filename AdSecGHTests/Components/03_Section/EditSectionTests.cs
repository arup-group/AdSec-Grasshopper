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

    private readonly EditSection _component;

    public EditSectionTests() {
      _component = new EditSection();
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      ComponentTesting.ComputeOutputs(_component);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveNoWarnings() {
      var section = new AdSecSectionGoo(new AdSecSection(SampleData.GetSectionDesign()));
      _component.SetInputParamAt(0, section);
      ComponentTesting.ComputeOutputs(_component);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Empty(runtimeMessages);
    }

    [Fact]
    public void ShouldHaveAValidSection() {
      var section = new AdSecSectionGoo(new AdSecSection(SampleData.GetSectionDesign()));
      _component.SetInputParamAt(0, section);
      ComponentTesting.ComputeOutputs(_component);
      var sectionOut = _component.GetOutputParamAt(0).GetValue<AdSecSectionGoo>(0, 0);
      Assert.NotNull(sectionOut);
    }

    [Fact]
    public void ShouldNeedSectionToWork() {
      ComponentTesting.ComputeOutputs(_component);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(runtimeMessages);
      string message = runtimeMessages.First();
      Assert.Contains("Sec", message);
      Assert.Contains("failed", message);
    }

    [Fact]
    public void ShouldHaveMetadataWithRightName() {
      Assert.Equal("EditSection", _component.BusinessComponent.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveSixInputs() {
      Assert.Equal(6, _component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveSevenOutput() {
      Assert.Equal(7, _component.Params.Output.Count);
    }
  }
}
