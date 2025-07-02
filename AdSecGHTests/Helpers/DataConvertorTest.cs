using System;
using System.Drawing;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {

  [Collection("GrasshopperFixture collection")]
  public class DataConvertorTest {
    private readonly FakeDropdownComponent _dropdownComponent;
    private readonly FakeBusiness fakeBusiness;

    public DataConvertorTest() {
      fakeBusiness = new FakeBusiness();
      _dropdownComponent = new FakeDropdownComponent();
      _dropdownComponent.SetDefaultValues();
      ComponentTesting.ComputeOutputs(_dropdownComponent);
    }

    [Fact]
    public void ShouldNotReturnResultForInvalidInput() {
      _dropdownComponent.ClearInputs();
      var invalidInput = new Point();
      bool result1 = _dropdownComponent.SetInputParamAt(0, invalidInput);
      bool result2 = _dropdownComponent.SetInputParamAt(1, invalidInput);
      Assert.False(result1 && result2);
      ComponentTesting.ComputeOutputs(_dropdownComponent);
      Assert.Throws<ArgumentOutOfRangeException>(() => GetFirstOutput(_dropdownComponent));
    }

    [Fact]
    public void ShouldHaveTheSameNumberOfInputs() {
      Assert.Equal(fakeBusiness.GetAllInputAttributes().Length, _dropdownComponent.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveNoWarning() {
      var runtimeMessages = _dropdownComponent.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Empty(runtimeMessages);
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      var runtimeMessages = _dropdownComponent.RuntimeMessages(GH_RuntimeMessageLevel.Error);
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
      return _dropdownComponent.GetInputParamAt(0);
    }

    private IGH_Param GetSecondInput() {
      return _dropdownComponent.GetInputParamAt(1);
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
      dynamic output = GetFirstOutput(_dropdownComponent);
      Assert.NotNull(output.Value);
    }

    [Fact]
    public void ShouldComputeTheRightResult() {
      dynamic output = GetFirstOutput(_dropdownComponent);
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
      _dropdownComponent.SetDefaultValues();
      _dropdownComponent.CollectData();
      dynamic actual = GetFirstInput().VolatileData.get_Branch(0)[0];
      Assert.Equal((float)fakeBusiness.Alpha.Default, actual.Value, 0.01f);
    }

    [Fact]
    public void ShouldPassTheNickname() {
      Assert.Equal("A", GetFirstInput().NickName);
    }

    [Fact]
    public void ShouldStoreGuid() {
      Assert.Equal("caa08c9e-417c-42ae-b704-91f214c8c871", _dropdownComponent.ComponentGuid.ToString());
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
