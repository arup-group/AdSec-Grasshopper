using AdSecGHCore.Helpers;

namespace AdSecCoreTests {
  public class DoubleComparerTests {
    [Fact]
    public void ValuesDifferLessThanEpsilonShouldBeConsideredEqual() {
      Assert.Equal(10.0, 10.01, new DoubleComparer());
    }

    [Fact]
    public void ValuesLargerThanEpsilonShouldBeConsideredEqual() {
      int epsilon = 1;
      Assert.NotEqual(10.0, 10.0 + (epsilon * 2), new DoubleComparer(epsilon, true));
    }
  }
}
