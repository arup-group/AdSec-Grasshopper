using System;

using AdSecCore;
using AdSecCore.Builders;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;
using Oasys.GH.Helpers;

using OasysGH.Units;

using OasysUnits;

using Xunit;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreatePreLoadTests {
    private readonly CreatePreLoad _component;
    private readonly DoubleComparer _comparer = new DoubleComparer();
    private readonly AdSecRebarGroupGoo _rebarGroup = new AdSecRebarGroupGoo(new AdSecRebarGroup(new BuilderTemplateGroup().AtFace(ITemplateGroup.Face.Top).Build()));
    public CreatePreLoadTests() {
      _component = new CreatePreLoad();
      _rebarGroup.Cover = ICover.Create(Length.FromMillimeters(25));
    }

    private void SetDefaultInputs() {
      ComponentTestHelper.SetInput(_component, _rebarGroup, 0);
      ComponentTestHelper.SetInput(_component, 10, 1);
    }

    [Fact]
    public void ShouldNotModifyUpStreamRebarGroup() {
      SetDefaultInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = ((AdSecRebarGroupGoo)ComponentTestHelper.GetOutput(_component, 0));
      var inputPreLoad = (IPreForce)((ILongitudinalGroup)_rebarGroup.Value.Group).Preload;
      var outPreLoad = (IPreForce)((ILongitudinalGroup)output.Value.Group).Preload;
      Assert.NotEqual(inputPreLoad.Force.As(DefaultUnits.ForceUnit), outPreLoad.Force.As(DefaultUnits.ForceUnit), _comparer);
    }

    [Fact]
    public void ShouldCreatePreForceCorrectly() {
      SetDefaultInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = ((AdSecRebarGroupGoo)ComponentTestHelper.GetOutput(_component, 0));
      var outPreLoad = (IPreForce)((ILongitudinalGroup)output.Value.Group).Preload;
      Assert.NotNull(output);
      Assert.Contains("prestress", output.ToString());
      Assert.NotNull(output.Value.Group as ILongitudinalGroup);
      Assert.Equal(10, outPreLoad.Force.As(DefaultUnits.ForceUnit), _comparer);
    }

    [Fact]
    public void ShouldCreatePreStrainCorrectly() {
      _component.SetSelected(0, 1);
      SetDefaultInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = ((AdSecRebarGroupGoo)ComponentTestHelper.GetOutput(_component, 0));
      var outPreLoad = (IPreStrain)((ILongitudinalGroup)output.Value.Group).Preload;
      Assert.NotNull(output);
      Assert.Contains("prestress", output.ToString());
      Assert.NotNull(output.Value.Group as ILongitudinalGroup);
      Assert.Equal(10, outPreLoad.Strain.As(DefaultUnits.StrainUnitResult), _comparer);
    }

    [Fact]
    public void ShouldCreatePreStressCorrectly() {
      _component.SetSelected(0, 2);
      SetDefaultInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = ((AdSecRebarGroupGoo)ComponentTestHelper.GetOutput(_component, 0));
      var outPreLoad = (IPreStress)((ILongitudinalGroup)output.Value.Group).Preload;
      Assert.NotNull(output);
      Assert.Contains("prestress", output.ToString());
      Assert.NotNull(output.Value.Group as ILongitudinalGroup);
      Assert.Equal(10, outPreLoad.Stress.As(DefaultUnits.StressUnitResult), _comparer);
    }

    [Fact]
    public void ShouldThrowExceptionWhenRebarGroupIsNull() {
      ComponentTestHelper.SetInput(_component, new AdSecRebarGroupGoo(), 0);
      ComponentTestHelper.SetInput(_component, 10, 1);
      Assert.Throws<NullReferenceException>(() => ComponentTestHelper.ComputeData(_component));
    }

    [Fact]
    public void ShouldLogErrorMessageWhenRebarGroupInputIsNull() {
      AdSecRebarGroupGoo group = null;
      ComponentTestHelper.SetInput(_component, group, 0);
      ComponentTestHelper.SetInput(_component, 10, 1);
      ComponentTestHelper.ComputeData(_component);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeMessages);
      Assert.Contains("Invalid RebarGroup input", runtimeMessages[0]);
    }

    [Fact]
    public void ShouldThrowExceptionWhenPreloadInputIsInvalid() {
      ComponentTestHelper.SetInput(_component, _rebarGroup, 0);
      ComponentTestHelper.SetInput(_component, "InvalidType", 1);
      ComponentTestHelper.ComputeData(_component);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeMessages);
      Assert.Contains("Could not parse the input InvalidType to the desired quantity", runtimeMessages[0]);
    }

    [Fact]
    public void ShouldLogErrorMessageWhenRebarGroupIsNotLogitudinal() {
      var linkGroup = ILinkGroup.Create(new BuilderLayer().Build().BarBundle);
      var rebarGroup = new AdSecRebarGroupGoo(new AdSecRebarGroup(linkGroup));
      ComponentTestHelper.SetInput(_component, rebarGroup, 0);
      ComponentTestHelper.SetInput(_component, 10, 1);
      ComponentTestHelper.ComputeData(_component);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeMessages);
      Assert.Contains("RebarGroup must be a longitudinal group", runtimeMessages[0]);
    }

    [Fact]
    public void ShouldUpdateForceUnitsCorrectly() {
      _component.SetSelected(0, 0);
      _component.SetSelected(1, 3);
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("Force [lbf]", _component.Params.Input[1].Name);
    }

    [Fact]
    public void ShouldUpdateStrainUnitsCorrectly() {
      _component.SetSelected(0, 1);
      _component.SetSelected(1, 3);
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("Strain [%]", _component.Params.Input[1].Name);
    }

    [Fact]
    public void ShouldUpdateStressUnitsCorrectly() {
      _component.SetSelected(0, 2);
      _component.SetSelected(1, 3);
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("Stress [GPa]", _component.Params.Input[1].Name);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.Prestress));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldBeHidden() {
      Assert.False(_component.Hidden);
    }
  }
}
