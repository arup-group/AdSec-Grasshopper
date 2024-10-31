using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {

  public abstract class DataConvertorTest {
    private readonly DummyBusiness dummyBusiness;
    private readonly DummyOasysDropdown oasysDropdown;

    public DataConvertorTest() {
      dummyBusiness = new DummyBusiness();
      oasysDropdown = new DummyOasysDropdown();
    }

    [Fact]
    public void ShouldHaveTheSameNumberOfInputs() {
      Assert.Single(oasysDropdown.Params.Input);
    }

    [Fact]
    public void ShouldPassTheName() {
      Assert.Equal("Alpha", oasysDropdown.Params.Input[0].Name);
    }

    [Fact]
    public void ShouldComputeAndAssignOutputs() {
      oasysDropdown.SetDefaultValues();
      ComputeOutputs(oasysDropdown);
      dynamic output = GetFirstOutput(oasysDropdown);
      Assert.NotNull(output.Value);
    }

    private static object GetFirstOutput(GH_Component component) {
      return GetOutputOfParam(component, 0, 0, 0);
    }

    private static object GetOutputOfParam(GH_Component component, int paramIndex, int branch, int index) {
      return component.Params.Output[paramIndex].VolatileData.get_Branch(branch)[index];
    }

    private static void ComputeOutputs(GH_Component component) {
      foreach (var param in component.Params.Output) {
        param.ExpireSolution(true);
        param.CollectData();
      }
    }

    [Fact]
    public void ShouldHaveDefaultValues() {
      oasysDropdown.SetDefaultValues();
      oasysDropdown.CollectData();
      // oasysDropdown.ExpireSolution(true);
      dynamic actual = oasysDropdown.Params.Input[0].VolatileData.get_Branch(0)[0]; // as GH_Number;
      Assert.Equal((float)dummyBusiness.Alpha.Default, actual.Value, 0.01f);
    }

    [Fact]
    public void ShouldPassTheNickname() {
      Assert.Equal("A", oasysDropdown.Params.Input[0].NickName);
    }
  }

}
