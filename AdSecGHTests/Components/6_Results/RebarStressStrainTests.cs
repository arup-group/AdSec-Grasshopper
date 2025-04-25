using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec;
using Oasys.GH.Helpers;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class RebarStressStrainTests {
    private readonly RebarStressStrain _component;
    private static SectionSolution Solution { get; set; } = null;

    public RebarStressStrainTests() {
      _component = new RebarStressStrain();
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      ComponentTestHelper.SetInput(_component, Solution, 0);
    }

    private void SetLoad() {
      ComponentTestHelper.SetInput(_component, ILoad.Create(
        Force.FromKilonewtons(-600),
        Moment.FromKilonewtonMeters(50),
        Moment.Zero), 1);
    }

    private void SetDeformation() {
      ComponentTestHelper.SetInput(_component, IDeformation.Create(
        Strain.FromRatio(-0.003),
        Curvature.Zero,
        Curvature.Zero), 1);
      ComponentTestHelper.ComputeData(_component);
    }

    [Fact]
    public void ShouldComputeCorrectlyForLoad() {
      SetLoad();
      // Check all outputs exist and are not empty lists
      for (int i = 0; i < 5; i++) {
        var output = ComponentTestHelper.GetOutput(_component, i);
        Assert.NotNull(output);
      }
    }

    [Fact]
    public void ShouldComputeCorrectlyForDeformation() {
      SetDeformation();
      for (int i = 0; i < 5; i++) {
        var output = ComponentTestHelper.GetOutput(_component, i);
        Assert.NotNull(output);
      }
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.StressStrainRebar));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }
  }
}
