using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec;

using OasysUnits;

namespace AdSecCoreTests {
  public class FindCrackLoadFunctionTests {
    private readonly FindCrackLoadFunction function;
    private static SectionSolution? Solution { get; set; } = null;

    public FindCrackLoadFunctionTests() {
      function = new FindCrackLoadFunction();
      function.LoadIncrement.Value = 1000;
      function.BaseLoad.Value = ILoad.Create(Force.FromNewtons(100), Moment.Zero, Moment.Zero);
      function.MaximumCrack.Value = Length.FromMillimeters(5e-5);
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      function.Solution.Value = Solution;
    }

    private void SetOptimisedLoadDirection(string direction) {
      function.OptimisedLoad.Value = direction;
      function.Compute();
    }

    [Theory]
    [InlineData("X")]
    [InlineData("XX")]
    [InlineData("Fx")]
    [InlineData("Fxx")]
    public void FxShouldHaveConvergence(string x) {
      SetOptimisedLoadDirection(x);
      Assert.NotNull(function.SectionLoad.Value);
      Assert.NotNull(function.MaximumCracking.Value);
    }

    [Theory]
    [InlineData("Y")]
    [InlineData("YY")]
    [InlineData("My")]
    [InlineData("Myy")]
    public void MyyShouldHaveConvergence(string y) {
      SetOptimisedLoadDirection(y);
      Assert.NotNull(function.SectionLoad.Value);
      Assert.NotNull(function.MaximumCracking.Value);
    }

    [Theory]
    [InlineData("Z")]
    [InlineData("ZZ")]
    [InlineData("Mz")]
    [InlineData("Mzz")]
    public void MzzShouldHaveConvergence(string z) {
      SetOptimisedLoadDirection(z);
      Assert.NotNull(function.SectionLoad.Value);
      Assert.NotNull(function.MaximumCracking.Value);
    }

    [Fact]
    public void ShouldThrowExceptionForWrongLoad() {
      Assert.Throws<ArgumentException>(() => SetOptimisedLoadDirection("p"));
    }

    [Fact]
    public void ShouldHaveFiveInput() {
      Assert.Equal(5, function.GetAllInputAttributes().Count());
    }
    [Fact]
    public void ShouldHaveTwoInput() {
      Assert.Equal(2, function.GetAllOutputAttributes().Count());
    }
  }
}
