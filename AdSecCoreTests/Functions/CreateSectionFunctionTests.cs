using AdSecCore.Functions;

namespace AdSecCoreTests.Functions {
  public class CreateSectionFunctionTests {

    readonly CreateSectionFunction function;

    public CreateSectionFunctionTests() {
      function = new CreateSectionFunction();
    }

    [Fact]
    public void ShouldHaveFourInputs() {
      Assert.Equal(4, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(function.GetAllOutputAttributes());
    }
  }
}
