using AdSecCore.Functions;

using AdSecGHCore.Constants;

namespace AdSecCoreTests.Functions {
  public class CreateRebarSpacingFunctionTests {
    CreateRebarSpacingFunction _function;

    public CreateRebarSpacingFunctionTests() {
      _function = new CreateRebarSpacingFunction();
    }

    [Fact]
    public void ShouldHaveAName() {
      Assert.Equal("Create Rebar Spacing", _function.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveANickName() {
      Assert.Equal("Spacing", _function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveADescription() {
      Assert.Equal("Create Rebar spacing (by Count or Pitch) for an AdSec Section", _function.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveACategory() {
      Assert.Equal(CategoryName.Name(), _function.Organisation.Category);
    }

    [Fact]
    public void ShouldHaveNoSubCategory() {
      Assert.Equal(SubCategoryName.Cat3(), _function.Organisation.SubCategory);
    }
  }
}
