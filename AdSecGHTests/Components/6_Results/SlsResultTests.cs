using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.GH.Helpers;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class SlsResultTests {
    private readonly ServiceabilityLimitStateResult _component;
    private static SectionSolution Solution { get; set; } = null;

    public SlsResultTests() {
      _component = new ServiceabilityLimitStateResult();
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      ComponentTestHelper.SetInput(_component, Solution, 0);
    }

    private void SetLoad(bool correctLoad = true) {
      if (correctLoad) {
        ComponentTestHelper.SetInput(_component, ILoad.Create(Force.FromKilonewtons(100), Moment.FromKilonewtonMeters(100), Moment.Zero), 1);

      } else {
        ComponentTestHelper.SetInput(_component, string.Empty, 1);
      }
      ComponentTestHelper.ComputeData(_component);
    }

    private void SetDeformation() {
      ComponentTestHelper.SetInput(_component, IDeformation.Create(Strain.FromRatio(0.00001), Curvature.Zero, Curvature.Zero), 1);
    }

    private void SetLargeLoad() {
      ComponentTestHelper.SetInput(_component, ILoad.Create(Force.FromKilonewtons(-100), Moment.FromKilonewtonMeters(900), Moment.Zero), 1);
      ComponentTestHelper.GetOutput(_component);
    }

    [Fact]
    public void ShouldHaveRemarkWhenUtiliztionIsGreaterThanOne() {
      SetLoad();
      Assert.Single(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ShouldHaveErrorForWrongLoad() {
      SetLoad(false);
      Assert.Single(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveWarningForHighLoad() {
      SetLargeLoad();
      Assert.Single(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
    }

    [Fact]
    public void ShouldCalculateCrackForGivenDeformation() {
      SetDeformation();
      ComponentTestHelper.GetOutput(_component, 3);
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 3));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.SLS));
    }
  }
}
