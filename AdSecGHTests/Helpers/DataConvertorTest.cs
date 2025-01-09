using System;
using System.Drawing;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {

  [Collection("GrasshopperFixture collection")]
  public class DataConvertorTest {
    private readonly FakeComponent component;
    private readonly FakeBusiness fakeBusiness;

    public DataConvertorTest() {
      fakeBusiness = new FakeBusiness();
      component = new FakeComponent();
      component.SetDefaultValues();
      ComponentTesting.ComputeOutputs(component);
    }

    [Fact]
    public void ShouldNotReturnResultForInvalidInput() {
      component.ClearInputs();
      var invalidInput = new Point();
      bool result1 = component.SetInputParamAt(0, invalidInput);
      bool result2 = component.SetInputParamAt(1, invalidInput);
      Assert.False(result1 && result2);
      ComponentTesting.ComputeOutputs(component);
      Assert.Throws<ArgumentOutOfRangeException>(() => GetFirstOutput(component));
    }

    [Fact]
    public void ShouldHaveTheSameNumberOfInputs() {
      Assert.Equal(fakeBusiness.GetAllInputAttributes().Length, component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveNoWarning() {
      var runtimeMessages = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Empty(runtimeMessages);
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      var runtimeMessages = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      foreach (string message in runtimeMessages) {
        Assert.Equal(string.Empty, message);
      }

      Assert.Empty(runtimeMessages);
    }

    [Fact]
    public void ShouldPassTheName() {
      Assert.Equal("Alpha", GetFirstInput().Name);
    }

    private IGH_Param GetFirstInput() {
      return component.GetInputParamAt(0);
    }

    private IGH_Param GetSecondInput() {
      return component.GetInputParamAt(1);
    }

    [Fact]
    private void ShouldSetAccessToItem() {
      Assert.Equal(GH_ParamAccess.item, GetFirstInput().Access);
    }

    [Fact]
    private void ShouldSetAccessToList() {
      Assert.Equal(GH_ParamAccess.list, GetSecondInput().Access);
    }

    [Fact]
    public void ShouldComputeAndAssignOutputs() {
      dynamic output = GetFirstOutput(component);
      Assert.NotNull(output.Value);
    }

    [Fact]
    public void ShouldComputeTheRightResult() {
      dynamic output = GetFirstOutput(component);
      Assert.Equal(12, output.Value);
    }

    private static object GetFirstOutput(GH_Component component) {
      return GetOutputOfParam(component, 0, 0, 0);
    }

    private static object GetOutputOfParam(GH_Component component, int param, int branch, int index) {
      return component.GetOutputParamAt(param).GetValue(branch, index);
    }

    [Fact]
    public void ShouldHaveDefaultValues() {
      component.SetDefaultValues();
      component.CollectData();
      dynamic actual = GetFirstInput().VolatileData.get_Branch(0)[0];
      Assert.Equal((float)fakeBusiness.Alpha.Default, actual.Value, 0.01f);
    }

    [Fact]
    public void ShouldPassTheNickname() {
      Assert.Equal("A", GetFirstInput().NickName);
    }

    [Fact]
    public void ShouldStoreGuid() {
      Assert.Equal("caa08c9e-417c-42ae-b704-91f214c8c871", component.ComponentGuid.ToString());
    }
  }

  public class ComponentTesting {

    public static void ComputeOutputs(GH_Component component) {
      component.ExpireSolution(true);
      component.CollectData();
      foreach (var param in component.Params.Output) {
        //param.ExpireSolution(true);
        param.CollectData();
      }
    }
  }
}
