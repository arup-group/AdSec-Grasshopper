using AdSecCore;

namespace AdSecCoreTests {
  public class DoubleComparerTests {
    [Fact]
    public void ValuesDifferByVeryLittleAndEpsilonIsZero() {
      Assert.NotEqual(10.0, 10.000001, new DoubleComparer(0, true));
    }

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

    [Fact]
    public void DoubleComparer_GetHashCode_IsUsedInHashSet() {
      var comparer = new DoubleComparer(0.1, true);
      var hashSet = new HashSet<double>(comparer);
      hashSet.Add(1.01);
      hashSet.Add(1);
      Assert.Single(hashSet);
    }
  }
}
