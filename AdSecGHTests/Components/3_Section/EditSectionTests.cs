using System;

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

    private AdSecDesignCodeGoo GetDesignCode() {
      AdSecUtility.LoadAdSecAPI();
      var component = new CreateDesignCode();
      component.SetSelected(0, 1);
      return (AdSecDesignCodeGoo)ComponentTestHelper.GetOutput(component);
    }

    [Fact]
    public void CanSetAdSecDesignCodeGooObject() {
      ComponentTestHelper.SetInput(_component, GetDesignCode(), 3);
      ComponentTestHelper.ComputeData(_component);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void CanSetDesignCodeObject() {
      var designCodeGoo = GetDesignCode();
      var designCode = new DesignCode {
        IDesignCode = designCodeGoo.Value.DesignCode,
        DesignCodeName = designCodeGoo.Value.DesignCodeName
      };
      ComponentTestHelper.SetInput(_component, designCode, 3);
      ComponentTestHelper.ComputeData(_component);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
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
    public void ShouldUseCustomPlane() {
      // Given a new profile with a specific plane
      var profileDesign = ProfileDesign.From(SampleData.GetSectionDesign());
      var plane = OasysPlane.PlaneXZ;
      profileDesign.LocalPlane = plane;
      var profile = new AdSecProfileGoo(profileDesign);
      _component.SetInputParamAt(1, profile);
      ComponentTesting.ComputeOutputs(_component);
      // Process Component
      var profileOut = _component.GetOutputParamAt(1).GetValue<AdSecProfileGoo>(0, 0);
      // Confirm the new Section has that plane
      var inputPlane = profileDesign.LocalPlane;
      var localPlane = profileOut.Value.LocalPlane;
      Assert.Equal(inputPlane.XAxis.X, localPlane.XAxis.X);
      Assert.Equal(inputPlane.XAxis.Y, localPlane.XAxis.Y);
      Assert.Equal(inputPlane.XAxis.Z, localPlane.XAxis.Z);
    }

    [Fact]
    public void ShouldStoreNormalisedPlane() {
      // Given a new profile with a non-normalised plane
      var profileDesign = ProfileDesign.From(SampleData.GetSectionDesign());
      var plane = OasysPlane.PlaneXZ;
      plane.XAxis = new OasysPoint(1, 2, 3); // Non-Normalised plane
      profileDesign.LocalPlane = plane;
      // Process Component
      var profile = new AdSecProfileGoo(profileDesign);
      _component.SetInputParamAt(1, profile);
      ComponentTesting.ComputeOutputs(_component);
      var profileOut = _component.GetOutputParamAt(1).GetValue<AdSecProfileGoo>(0, 0);
      // Confirm the new Section has a normalised plane
      var localPlane = profileOut.Value.LocalPlane;
      Assert.True(localPlane.XAxis.X <= 1);
      Assert.True(localPlane.XAxis.Y <= 1);
      Assert.True(localPlane.XAxis.Z <= 1);
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
      string message = runtimeMessages[0];
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
