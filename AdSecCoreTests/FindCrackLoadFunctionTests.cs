using AdSecCore.Functions;

namespace AdSecCoreTests {
  public class FindCrackLoadFunctionTests {
    private readonly FindCrackLoadFunction function;
    public FindCrackLoadFunctionTests() {
      function = new FindCrackLoadFunction();
    }

    [Theory]
    [InlineData("X")]
    [InlineData("XX")]
    [InlineData("Fx")]
    [InlineData("Fxx")]
    public void FxShouldReturnTrueFor(string x) {
      Assert.True(FindCrackLoadFunction.IsFx(x));
    }

    [Fact]
    public void FxShouldReturnFalseFor() {
      Assert.False(FindCrackLoadFunction.IsFx("zz"));
    }

    [Theory]
    [InlineData("Y")]
    [InlineData("YY")]
    [InlineData("My")]
    [InlineData("Myy")]
    public void MyyShouldReturnTrueFor(string x) {
      Assert.True(FindCrackLoadFunction.IsMyy(x));
    }

    [Fact]
    public void MyyShouldReturnFalseFor() {
      Assert.False(FindCrackLoadFunction.IsMyy("xx"));
    }

    [Theory]
    [InlineData("Z")]
    [InlineData("ZZ")]
    [InlineData("Mz")]
    [InlineData("Mzz")]
    public void MzzShouldReturnTrueFor(string x) {
      Assert.True(FindCrackLoadFunction.IsMzz(x));
    }

    [Fact]
    public void MzzShouldReturnFalseFor() {
      Assert.False(FindCrackLoadFunction.IsMzz("xx"));
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
