using AdSecCore.Functions;

using AdSecGH.Components;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests {
  [Collection("GrasshopperFixture collection")]
  public class AdapterBaseTests {

    private readonly CreatePoint _component;
    private readonly Function _function;

    public AdapterBaseTests() {
      _component = new CreatePoint();
      _function = _component.BusinessComponent;
    }

    [Fact]
    public void ShouldAddWarningsOnComponent() {
      var message = "This is a warning message.";
      _function.WarningMessages.Add(message);
      AdapterBase.UpdateMessages(_function, _component);
      Assert.Contains(message, _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
    }

    [Fact]
    public void ShouldAddErrorsOnComponent() {
      var message = "This is a error message.";
      _function.ErrorMessages.Add(message);
      AdapterBase.UpdateMessages(_function, _component);
      Assert.Contains(message, _component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldAddRemarksOnComponent() {
      var message = "This is a remark message.";
      _function.RemarkMessages.Add(message);
      AdapterBase.UpdateMessages(_function, _component);
      Assert.Contains(message, _component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

  }
}
