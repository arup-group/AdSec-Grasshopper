using System;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class SlsResultTests {
    private readonly SlsResult _component;
    private static SectionSolution Solution { get; set; } = null;

    public SlsResultTests() {
      _component = new SlsResult();
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
    public void ComponentGuid_IsCorrect() {
      Assert.Equal(new Guid("27ba3ec5-b94c-43ad-8623-087540413628"), _component.ComponentGuid);
    }

    [Fact]
    public void Exposure_IsPrimary() {
      Assert.Equal(GH_Exposure.primary, _component.Exposure);
    }

    [Fact]
    public void PluginInfo_IsCorrect() {
      Assert.Equal(AdSecGH.PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void Icon_IsCorrect() {
      Assert.Equal(Resources.SLS.ToString(), _component.Icon_24x24.ToString());
    }

    [Fact]
    public void Category_IsCorrect() {
      Assert.Equal(CategoryName.Name(), _component.Category);
    }

    [Fact]
    public void SubCategory_IsCorrect() {
      Assert.Equal(SubCategoryName.Cat7(), _component.SubCategory);
    }

    [Fact]
    public void Hidden_IsTrue() {
      Assert.True(_component.Hidden);
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
  }
}
