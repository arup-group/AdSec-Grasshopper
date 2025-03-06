using AdSecCore.Functions;

namespace AdSecCoreTests {
  public class FindCrackLoadFunctionTests {
    private readonly FindCrackLoadFunction function;
    public FindCrackLoadFunctionTests() {
      function = new FindCrackLoadFunction();
    }

    [Fact]
    public void IsFxWhenX() {
      Assert.True(FindCrackLoadFunction.IsFx("X"));
    }

    [Fact]
    public void IsFxWhenXX() {
      Assert.True(FindCrackLoadFunction.IsFx("XX"));
    }

    [Fact]
    public void IsFxWhenFx() {
      Assert.True(FindCrackLoadFunction.IsFx("Fx"));
    }

    [Fact]
    public void IsFxWhenFXX() {
      Assert.True(FindCrackLoadFunction.IsFx("FXX"));
    }

    [Fact]
    public void IsNotFxWhenZZ() {
      Assert.False(FindCrackLoadFunction.IsFx("ZZ"));
    }

    [Fact]
    public void IsMyyWhenY() {
      Assert.True(FindCrackLoadFunction.IsMyy("Y"));
    }

    [Fact]
    public void IsMyyWhenYY() {
      Assert.True(FindCrackLoadFunction.IsMyy("YY"));
    }

    [Fact]
    public void IsMyyWhenMy() {
      Assert.True(FindCrackLoadFunction.IsMyy("MY"));
    }

    [Fact]
    public void IsMyyWhenMyy() {
      Assert.True(FindCrackLoadFunction.IsMyy("MYY"));
    }

    [Fact]
    public void IsNotMyyWhenZZ() {
      Assert.False(FindCrackLoadFunction.IsMyy("ZZ"));
    }

    [Fact]
    public void IsMzzWhenZ() {
      Assert.True(FindCrackLoadFunction.IsMzz("Z"));
    }

    [Fact]
    public void IsMzzWhenZZ() {
      Assert.True(FindCrackLoadFunction.IsMzz("ZZ"));
    }

    [Fact]
    public void IsMzzWhenMz() {
      Assert.True(FindCrackLoadFunction.IsMzz("MZ"));
    }

    [Fact]
    public void IsMzzWhenMzz() {
      Assert.True(FindCrackLoadFunction.IsMzz("MZZ"));
    }

    [Fact]
    public void IsNotMzzWhenYY() {
      Assert.False(FindCrackLoadFunction.IsMzz("YY"));
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
