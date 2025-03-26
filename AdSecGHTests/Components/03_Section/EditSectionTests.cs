using System.Linq;

using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

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
      var section = new AdSecSectionGoo(new AdSecSection(SampleData.GetSectionDesign()));
      _component.SetInputParamAt(0, section);
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      ComponentTesting.ComputeOutputs(_component);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveNoWarnings() {
      ComponentTesting.ComputeOutputs(_component);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Empty(runtimeMessages);
    }

    [Fact]
    public void ShouldHaveAProfile() {
      ComponentTesting.ComputeOutputs(_component);
      object profileOut = _component.GetOutputParamAt(1).GetValue(0, 0);
      Assert.NotNull(profileOut);
    }

    [Fact]
    public void ShouldOverrideTheProfile() {
      var profileDesign = ProfileDesign.From(SampleData.GetSectionDesign());
      var plane = OasysPlane.PlaneYZ;
      plane.XAxis = new OasysPoint { X = 1, Z = 2, Y = 3, };
      profileDesign.LocalPlane = plane;
      var profile = new AdSecProfileGoo(profileDesign);
      _component.SetInputParamAt(1, profile);
      ComponentTesting.ComputeOutputs(_component);
      var profileOut = _component.GetOutputParamAt(1).GetValue<AdSecProfileGoo>(0, 0);
      Assert.NotNull(profileOut);
      Assert.Equal(profileDesign.LocalPlane.XAxis.X, profileOut.Value.LocalPlane.XAxis.X);
      Assert.Equal(profileDesign.LocalPlane.XAxis.Y, profileOut.Value.LocalPlane.XAxis.Y);
      Assert.Equal(profileDesign.LocalPlane.XAxis.Z, profileOut.Value.LocalPlane.XAxis.Z);
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
      _component.ClearInputs();
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

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.EditSection));
    }
  }
}
