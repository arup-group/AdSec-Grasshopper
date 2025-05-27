using AdSecCore.Functions;

using AdSecGH.Components;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests {
  [Collection("GrasshopperFixture collection")]
  public class AdapterBaseTests {

    private CreatePoint _component;
    private Function _function;

    public AdapterBaseTests() {
      _component = new CreatePoint();
      _function = _component.BusinessComponent;
    }

    [Theory]
    [InlineData("This is a warning message.", GH_RuntimeMessageLevel.Warning)]
    [InlineData("This is a remark message.", GH_RuntimeMessageLevel.Remark)]
    [InlineData("This is an error message.", GH_RuntimeMessageLevel.Error)]
    public void ShouldAddWarningsOnComponent(string message, GH_RuntimeMessageLevel level) {
      _function.WarningMessages.Add(message);
      AdapterBase.UpdateMessages(_function, _component);

      Assert.Contains(message, _component.RuntimeMessages(level));
    }
  }
}
