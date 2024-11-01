using Grasshopper.Kernel;

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
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldPassTheName() {
      Assert.Equal("Alpha", GetFirstInput().Name);
    }

    private IGH_Param GetFirstInput() {
      return GetParamAt(0);
    }

    private IGH_Param GetSecondInput() {
      return GetParamAt(1);
    }

    private IGH_Param GetParamAt(int index) {
      return component.Params.Input[index];
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

    private static object GetFirstOutput(GH_Component component) {
      return GetOutputOfParam(component, 0, 0, 0);
    }

    private static object GetOutputOfParam(GH_Component component, int param, int branch, int index) {
      return component.Params.Output[param].VolatileData.get_Branch(branch)[index];
    }

    [Fact]
    public void ShouldHaveDefaultValues() {
      component.SetDefaultValues();
      component.CollectData();
      dynamic actual = GetFirstInput().VolatileData.get_Branch(0)[0]; // as GH_Number;
      Assert.Equal((float)fakeBusiness.Alpha.Default, actual.Value, 0.01f);
    }

    [Fact]
    public void ShouldPassTheNickname() {
      Assert.Equal("A", GetFirstInput().NickName);
    }
  }

  public class ComponentTesting {

    public static void ComputeOutputs(GH_Component component) {
      component.ExpireSolution(true);
      component.CollectData();
      foreach (var param in component.Params.Output) {
        param.ExpireSolution(true);
        param.CollectData();
      }
    }
  }
}
