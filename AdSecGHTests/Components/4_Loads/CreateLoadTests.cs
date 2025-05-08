using AdSecCore;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateLoadTests {
    private readonly CreateLoad _component;
    private readonly DoubleComparer comparer = new DoubleComparer();
    public CreateLoadTests() {
      _component = new CreateLoad();
    }

    private void SetDefaultInputs() {
      ComponentTestHelper.SetInput(_component, 1, 0);
      ComponentTestHelper.SetInput(_component, 2, 1);
      ComponentTestHelper.SetInput(_component, 3, 2);
    }

    [Fact]
    public void ShouldCreateLoadCorrectly() {
      SetDefaultInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = (AdSecLoadGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      Assert.Equal(1, output.Value.X.As(DefaultUnits.ForceUnit), comparer);
      Assert.Equal(2, output.Value.YY.As(DefaultUnits.MomentUnit), comparer);
      Assert.Equal(3, output.Value.ZZ.As(DefaultUnits.MomentUnit), comparer);
    }

    [Fact]
    public void ShouldHandleZeroValues() {
      ComponentTestHelper.SetInput(_component, Force.Zero, 0);
      ComponentTestHelper.SetInput(_component, Moment.Zero, 1);
      ComponentTestHelper.SetInput(_component, Moment.Zero, 2);
      var output = (AdSecLoadGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      Assert.Equal(0, output.Value.X.As(DefaultUnits.ForceUnit), comparer);
      Assert.Equal(0, output.Value.YY.As(DefaultUnits.MomentUnit), comparer);
      Assert.Equal(0, output.Value.ZZ.As(DefaultUnits.MomentUnit), comparer);
    }

    [Fact]
    public void ShouldThrowExceptionWhenForceInputIsNotCorrectValues() {
      ComponentTestHelper.SetInput(_component, Moment.Zero, 0);
      ComponentTestHelper.ComputeData(_component);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeMessages);
      Assert.Contains("Could not parse the input", runtimeMessages[0]);
    }

    [Fact]
    public void ShouldThrowExceptionWhenMomentInputIsNotCorrectValues() {
      ComponentTestHelper.SetInput(_component, Force.Zero, 1);
      ComponentTestHelper.ComputeData(_component);
      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(runtimeMessages);
      Assert.Contains("Could not parse the input", runtimeMessages[0]);
    }

    [Fact]
    public void ShouldUpdateUnitsCorrectly() {
      // Arrange
      _component.SetSelected(0, 3);
      _component.SetSelected(1, 3);
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("Fx [lbf]", _component.Params.Input[0].Name);
      Assert.Contains("Myy [kipf·in]", _component.Params.Input[1].Name);
      Assert.Contains("Mzz [kipf·in]", _component.Params.Input[2].Name);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.CreateLoad));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }
  }
}
