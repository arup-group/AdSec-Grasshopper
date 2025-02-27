using AdSecCore.Functions;

namespace AdSecCoreTests.Functions {
  public class AnalyseFunctionTests {

    private readonly AnalyseFunction analyseFunction;

    public AnalyseFunctionTests() {
      analyseFunction = new AnalyseFunction();
    }

    [Fact]
    public void ShouldHaveOneInput() {
      Assert.Single(analyseFunction.GetAllInputAttributes());
    }

    [Fact]
    public void ShouldHaveTwoOutputs() {
      Assert.Equal(2, analyseFunction.GetAllOutputAttributes().Length);
    }

    [Fact(Skip = "Not implemented yet")]
    public void ShouldComputOutputs() { }
  }
}
