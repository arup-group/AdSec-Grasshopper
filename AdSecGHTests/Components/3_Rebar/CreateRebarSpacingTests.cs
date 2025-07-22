using System.ComponentModel;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.GH.Helpers;

using OasysUnits;
using OasysUnits.Units;

using Rhino.NodeInCode;

using Xunit;

namespace AdSecGHTests.Components._3_Rebar {
  [Collection("GrasshopperFixture collection")]
  public class CreateRebarSpacingTests {
    private readonly CreateRebarSpacing _component;
    private readonly IBarBundle _singleBars = new BuilderSingleBar().AtPosition(Geometry.Zero()).Build().BarBundle;

    public CreateRebarSpacingTests() {
      _component = new CreateRebarSpacing();
      _component.SetInputParamAt(0, new AdSecRebarBundleGoo(_singleBars));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.RebarSpacing));
    }

    [Fact]
    public void ShouldUpdateNameWithUnits() {
      _component.SetSelected(0, 0); // Distance
      Assert.Contains("[m]", _component.BusinessComponent.Spacing.Name);
    }

    [Fact]
    public void ShouldUpdateNameWithUnitsWithNonDefaultUnits() {
      _component.SetSelected(0, 0); // Distance
      _component.SetSelected(1, 0); // mm
      Assert.Contains("[mm]", _component.BusinessComponent.Spacing.Name);
    }

    [Fact]
    public void ShouldHaveDynamicDropdownItems() {
      _component.SetSelected(0, 1); // Count
      Assert.Single(_component.DropDownItems);
    }

    [Fact]
    public void ShouldComputeOverDistance() {
      _component.SetSelected(0, 0); // Distance
      _component.SetInputParamAt(1, Length.From(1, LengthUnit.Meter).Value);
      ComponentTestHelper.ComputeData(_component);
      var adSecRebarLayerGoo = _component.GetValue<AdSecRebarLayerGoo>();
      var layerByBarPitch = adSecRebarLayerGoo.Value as ILayerByBarPitch;
      Assert.NotNull(layerByBarPitch);
      Assert.Equal(1, layerByBarPitch.Pitch.As(LengthUnit.Meter));
    }

    [Fact]
    public void ShouldComputeOverCount() {
      _component.SetSelected(0, 1); // Count
      _component.SetInputParamAt(1, 2);
      ComponentTestHelper.ComputeData(_component);
      var adSecRebarLayerGoo = _component.GetValue<AdSecRebarLayerGoo>();
      var layerByBarCount = adSecRebarLayerGoo.Value as ILayerByBarCount;
      Assert.NotNull(layerByBarCount);
      Assert.Equal(2, layerByBarCount.Count);
    }

    [Fact]
    public void ShouldUseSavedMode() {
      _component.SetSelected(0, 1); // Count
      _component.SetInputParamAt(1, 2);
      ComponentTestHelper.ComputeData(_component);
      Assert.Equal(SpacingMode.Count, _component.BusinessComponent.Mode);

      var doc = new GH_DocumentIO();
      doc.Document = new GH_Document();
      doc.Document.AddObject(_component, false);
      var randomPath = CreateRebarGroupSaveLoadTests.GetRandomName();
      doc.SaveQuiet(randomPath);
      doc.Open(randomPath);
      var loadedComponent = (CreateRebarSpacing)doc.Document.FindComponent(_component.InstanceGuid);
      ComponentTestHelper.SetInput(loadedComponent, new AdSecRebarBundleGoo(_singleBars));
      ComponentTestHelper.SetInput(loadedComponent, 2, 1);
      var result = (AdSecRebarLayerGoo)ComponentTestHelper.GetOutput(loadedComponent);
      var layer = result.Value as ILayerByBarCount;
      Assert.Equal(2, layer.Count);
      Assert.Equal(SpacingMode.Count, loadedComponent.BusinessComponent.Mode);
    }

    [Fact]
    public void ShouldUseSavedUnits() {
      _component.SetSelected(0, 0); // Distance
      _component.SetSelected(1, 1); // cm
      _component.SetInputParamAt(1, 2);
      ComponentTestHelper.ComputeData(_component);
      Assert.Equal(LengthUnit.Centimeter, _component.BusinessComponent.LocalLengthUnitGeometry);

      var doc = new GH_DocumentIO();
      doc.Document = new GH_Document();
      doc.Document.AddObject(_component, false);
      var randomPath = CreateRebarGroupSaveLoadTests.GetRandomName();
      doc.SaveQuiet(randomPath);
      var doc2 = new GH_DocumentIO();
      doc2.Document = new GH_Document();
      doc2.Open(randomPath);
      var loadedComponent = (CreateRebarSpacing)doc2.Document.FindComponent(_component.InstanceGuid);
      ComponentTestHelper.ComputeData(loadedComponent);

      Assert.Equal(LengthUnit.Centimeter, loadedComponent.BusinessComponent.LocalLengthUnitGeometry);
    }
  }
}
