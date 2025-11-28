using AdSecCore;
using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.GH.Helpers;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class UlsResultTests {
    private readonly UltimateLimitStateResult _component;
    private static SectionSolution Solution { get; set; } = null;
    private readonly DoubleComparer comparer = new DoubleComparer();
    public UlsResultTests() {
      _component = new UltimateLimitStateResult();
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      ComponentTestHelper.SetInput(_component, Solution, 0);
    }

    private void SetLoad(bool correctLoad = true) {
      if (correctLoad) {
        ComponentTestHelper.SetInput(_component,
          ILoad.Create(
            Force.FromKilonewtons(100),
            Moment.FromKilonewtonMeters(100),
            Moment.Zero),
          1);
      } else {
        ComponentTestHelper.SetInput(_component, string.Empty, 1);
      }
      ComponentTestHelper.ComputeData(_component);
    }

    private void SetDeformation() {
      ComponentTestHelper.SetInput(_component,
        IDeformation.Create(
          Strain.FromRatio(0.00001),
          Curvature.Zero,
          Curvature.Zero),
        1);
    }

    private void SetLargeLoad() {
      ComponentTestHelper.SetInput(_component,
        ILoad.Create(
          Force.FromKilonewtons(-100),
          Moment.FromKilonewtonMeters(900),
          Moment.Zero),
        1);
      ComponentTestHelper.GetOutput(_component);
    }

    [Fact]
    public void ShouldHaveErrorForWrongLoad() {
      SetLoad(false);
      Assert.Single(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void MessagesShouldBeClearedOnEachRun() {
      SetLargeLoad();
      Assert.Equal(2, _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning).Count);
      var newLoadWithConvergence = ILoad.Create(
            Force.FromKilonewtons(-10),
            Moment.Zero,
            Moment.Zero);
      _component.Params.Input[1].AddVolatileData(new GH_Path(0), 0, newLoadWithConvergence);
      ComponentTestHelper.ComputeData(_component);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
    }

    [Fact]
    public void ShouldCalculateDeformationForGivenLoad() {
      SetLoad();
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 2));
    }

    [Fact]
    public void ShouldCalculateFailureForGivenDeformation() {
      SetDeformation();
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 8));
    }

    [Fact]
    public void ShouldCalculateNeutralAxisForGivenLoad() {
      SetLoad();
      var axis = (AdSecNeutralAxisGoo)ComponentTestHelper.GetOutput(_component, 5);
      var length = new Length(axis.AxisLine.Length, DefaultUnits.LengthUnitGeometry);
      Assert.Equal(0.7043, length.As(LengthUnit.Meter), comparer);
    }

    [Fact]
    public void ShouldCalculateOffsetForGivenLoad() {
      SetLoad();
      var wrapper = (GH_ObjectWrapper)ComponentTestHelper.GetOutput(_component, 6);
      var offset = (Length)wrapper.Value;
      Assert.Equal(-0.707, offset.As(LengthUnit.Meter), comparer);
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.UltimateLimitStateResult));
    }

  }
}
