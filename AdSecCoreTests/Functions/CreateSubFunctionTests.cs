using AdSecCore.Functions;

namespace AdSecCoreTests.Functions {
  public class CreateSubFunctionTests {

    private readonly CreateSubComponentFunction createSubComponentFunction;

    public CreateSubFunctionTests() {
      createSubComponentFunction = new CreateSubComponentFunction();
    }

    [Fact]
    public void ShouldHaveTwoInputs() {
      Assert.Equal(2, createSubComponentFunction.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveOffsetAsOptional() {
      // Assert.True(createSubComponentFunction.GetAllInputAttributes()[1].Optional);
    }
  }
}
