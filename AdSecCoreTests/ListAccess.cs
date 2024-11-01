using Oasys.Business;

namespace AdSecCoreTests {
  public class ListAccess {
    [Fact]
    public void ShouldSetDefaultAccessToItem() {
      Assert.Equal(Access.Item, new ParameterAttribute<int>().Access);
    }

    [Fact]
    public void ArrayTypeParametersShouldSetAccessToArray() {
      Assert.Equal(Access.List, new BaseArrayParameter<int[]>().Access);
    }

    [Fact]
    public void DoubleArrayParameterShouldSetAccessToArray() {
      Assert.Equal(Access.List, new DoubleArrayParameter().Access);
    }
  }

  public class DefaultValues {
    [Fact]
    public void ShouldSetDefaultValues() {
      var parameter = new DoubleParameter {
        Default = 10,
      };
      parameter.SetDefault();
      Assert.Equal(10, parameter.Value);
    }
  }
}
