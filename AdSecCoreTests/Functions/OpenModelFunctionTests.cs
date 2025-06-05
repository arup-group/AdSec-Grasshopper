using AdSecGHCore.Constants;

namespace AdSecCoreTests.Functions {
  public class OpenModelFunctionTests {
    private OpenModelFunction _function;

    public OpenModelFunctionTests() {
      _function = new OpenModelFunction();
    }

    [Fact]
    public void ShouldHaveAName() {
      Assert.Equal("Open Model", _function.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveANickName() {
      Assert.Equal("Open", _function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveADescription() {
      Assert.Equal("Open an existing AdSec .ads file", _function.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveACategory() {
      Assert.Equal(CategoryName.Name(), _function.Organisation.Category);
    }

    [Fact]
    public void ShouldHaveNoSubCategory() {
      Assert.Equal(SubCategoryName.Cat0(), _function.Organisation.SubCategory);
    }

  }
}
