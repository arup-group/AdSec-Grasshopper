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

    public class DummyFunction : Function {

      public override FuncAttribute Metadata { get; set; }
      public override Organisation Organisation { get; set; }
      public override void Compute() { }
    }

    [Fact]
    public void ShouldSkipRenameWhenParametersDoNotMatch() {
      // Works well for valid component
      string somethingNotRight = "SomethingNotRight";
      _component.Params.Input[0].Name = somethingNotRight;
      AdapterBase.RefreshParameter(_component.BusinessComponent, _component.Params);
      Assert.NotEqual(somethingNotRight, _component.Params.Input[0].Name);

      // But not for dummy function, as it doesn't have the right number of Attributes (empty)
      var dummyFunction = new DummyFunction();
      _component.Params.Input[0].Name = somethingNotRight;
      AdapterBase.RefreshParameter(dummyFunction, _component.Params);
      Assert.Equal(somethingNotRight, _component.Params.Input[0].Name);
    }

  }
}
