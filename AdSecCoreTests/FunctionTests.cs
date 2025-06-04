using AdSecCore.Functions;

namespace AdSecCoreTests {
  public class FunctionTests {
    private readonly TestFunction function;

    public class TestFunction : Function {
      public override FuncAttribute Metadata { get; set; } = new();
      public override Organisation Organisation { get; set; } = new();
      public override void Compute() { }
    }

    public FunctionTests() {
      function = new TestFunction();
    }

    [Fact]
    public void ShouldHaveInputAttributeInitialized() {
      Assert.Empty(function.GetAllInputAttributes());
    }

    [Fact]
    public void ShouldHaveOutputsAttributeInitialized() {
      Assert.Empty(function.GetAllOutputAttributes());
    }
  }
}
