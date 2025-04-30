using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec;
using Oasys.GH.Helpers;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateDeformationTests {
    private readonly CreateDeformation _component;

    public CreateDeformationTests() {
      _component = new CreateDeformation();
    }

    private void SetDefaultInputs() {
      ComponentTestHelper.SetInput(_component, Strain.FromRatio(0.001).Value, 0);
      ComponentTestHelper.SetInput(_component, Curvature.FromPerMeters(0.002).Value, 1);
      ComponentTestHelper.SetInput(_component, Curvature.FromPerMeters(0.003).Value, 2);
    }

    [Fact]
    public void ShouldCreateDeformationCorrectly() {
      SetDefaultInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = (AdSecDeformationGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      Assert.Equal(0.001, output.Value.X.As(StrainUnit.Ratio), 1);
      Assert.Equal(0.002, output.Value.YY.As(CurvatureUnit.PerMeter), 2);
      Assert.Equal(0.003, output.Value.ZZ.As(CurvatureUnit.PerMeter), 3);
    }

    [Fact]
    public void ShouldHandleZeroValues() {
      ComponentTestHelper.SetInput(_component, Strain.Zero, 0);
      ComponentTestHelper.SetInput(_component, Curvature.Zero, 1);
      ComponentTestHelper.SetInput(_component, Curvature.Zero, 2);
      var output = (AdSecDeformationGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      Assert.Equal(0, output.Value.X.As(StrainUnit.Ratio), 1);
      Assert.Equal(0, output.Value.YY.As(CurvatureUnit.PerMeter), 2);
      Assert.Equal(0, output.Value.ZZ.As(CurvatureUnit.PerMeter), 3);
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
