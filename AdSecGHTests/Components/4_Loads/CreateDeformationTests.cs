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
  public class CreateDeformationTests {
    private readonly CreateDeformation _component;
    private readonly DoubleComparer _comparer = new DoubleComparer();
    public CreateDeformationTests() {
      _component = new CreateDeformation();
    }

    private void SetDefaultInputs() {
      ComponentTestHelper.SetInput(_component, 0.001, 0);
      ComponentTestHelper.SetInput(_component, 0.002, 1);
      ComponentTestHelper.SetInput(_component, 0.003, 2);
    }

    [Fact]
    public void ShouldCreateDeformationCorrectly() {
      SetDefaultInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = (AdSecDeformationGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      Assert.Equal(0.001, output.Value.X.As(DefaultUnits.StrainUnitResult), _comparer);
      Assert.Equal(0.002, output.Value.YY.As(DefaultUnits.CurvatureUnit), _comparer);
      Assert.Equal(0.003, output.Value.ZZ.As(DefaultUnits.CurvatureUnit), _comparer);
    }

    [Fact]
    public void ShouldHandleZeroValues() {
      ComponentTestHelper.SetInput(_component, Strain.Zero, 0);
      ComponentTestHelper.SetInput(_component, Curvature.Zero, 1);
      ComponentTestHelper.SetInput(_component, Curvature.Zero, 2);
      var output = (AdSecDeformationGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      Assert.Equal(0, output.Value.X.As(StrainUnit.Ratio), _comparer);
      Assert.Equal(0, output.Value.YY.As(CurvatureUnit.PerMeter), _comparer);
      Assert.Equal(0, output.Value.ZZ.As(CurvatureUnit.PerMeter), _comparer);
    }

    [Fact]
    public void ShouldThrowExceptionWhenInputIsNotCorrectValues() {
      ComponentTestHelper.SetInput(_component, Moment.Zero, 0);
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
      Assert.Contains("εx [%]", _component.Params.Input[0].Name);
      Assert.Contains("κyy [cm⁻¹]", _component.Params.Input[1].Name);
      Assert.Contains("κzz [cm⁻¹]", _component.Params.Input[2].Name);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.DeformationLoad));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }
  }
}
