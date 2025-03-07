using AdSecGH.Components;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class FindCrackLoadTests {

    private readonly FindCrackLoad component;

    public FindCrackLoadTests() {
      component = new FindCrackLoad();
      ComponentTestHelper.SetInput(component, 10, 3);
      ComponentTestHelper.SetInput(component, ILoad.Create(Force.FromNewtons(100), Moment.Zero, Moment.Zero), 1);
    }

    private void SetInvalidSolution() {
      ComponentTestHelper.SetInput(component, string.Empty, 0);
    }

    private void SetMaximumCracking() {
      ComponentTestHelper.SetInput(component, 5e-8, 4);
    }

    private void SetInvalidCracking() {
      ComponentTestHelper.SetInput(component, string.Empty, 4);
    }

    [Fact]
    public void ShouldHaveErrors() {
      SetInvalidSolution();
      SetMaximumCracking();
      ComponentTestHelper.GetOutput(component);
      Assert.Single(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveErrorWhenCrackingIsInvalid() {
      SetInvalidSolution();
      SetInvalidCracking();
      ComponentTestHelper.GetOutput(component);
      Assert.Single(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }
  }
}
