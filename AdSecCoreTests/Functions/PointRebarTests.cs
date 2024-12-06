using AdSecCore.Functions;

using AdSecGHCore.Constants;

namespace AdSecCoreTests.Functions {
  public class PointRebarTests {
    private readonly PointRebarFunction pointRebarFunction;

    public PointRebarTests() {
      pointRebarFunction = new PointRebarFunction();
    }

    [Fact]
    public void ShouldStartHidden() {
      Assert.False(pointRebarFunction.Organisation.Hidden);
    }

    [Fact]
    public void ShouldBeOnSubCat3() {
      Assert.Equal(SubCategoryName.Cat3(), pointRebarFunction.Organisation.SubCategory);
    }

    [Fact]
    public void ShouldBeOnCatName() {
      Assert.Equal(CategoryName.Name(), pointRebarFunction.Organisation.Category);
    }
  }
}
