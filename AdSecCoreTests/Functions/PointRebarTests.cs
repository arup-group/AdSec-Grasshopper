using AdSecCore.Functions;

using AdSecGHCore.Constants;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class PointRebarTests {
    private readonly PointRebarFunction pointRebarFunction;

    public PointRebarTests() {
      pointRebarFunction = new PointRebarFunction();
      pointRebarFunction.Y.Value = 1;
      pointRebarFunction.Z.Value = 1;
      pointRebarFunction.Compute();
    }

    [Fact]
    public void ShouldReturnOneOutput() {
      Assert.Single(pointRebarFunction.GetAllOutputAttributes());
    }

    [Fact]
    public void ShouldComputeValidPoint() {
      var point = pointRebarFunction.Point.Value;
      Assert.Equal(1, point.Y.Value);
      Assert.Equal(1, point.Z.Value);
      Assert.Single(pointRebarFunction.GetAllOutputAttributes());
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
