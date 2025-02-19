using AdSecCore;

namespace AdSecCoreTests {
  public class DoubleComparerTests {
    [Fact]
    public void ValuesDifferLessThanEpsilonShouldBeConsideredEqualNoMargin() {
      Assert.Equal(10.0, 10.01, new DoubleComparer());
    }

    [Fact]
    public void ValuesDifferLessThanEpsilonShouldBeConsideredEqualWithMargin() {
      Assert.Equal(10.0, 10.001, new DoubleComparer(0.1, true));
    }
    [Fact]
    public void ValuesLargerThanEpsilonShouldBeConsideredEqual() {
      double epsilon = 1f;
      Assert.NotEqual(10.0, 10.0 + (epsilon * 2), new DoubleComparer(epsilon, true));
    }
  }
}
