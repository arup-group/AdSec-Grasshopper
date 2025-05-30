using AdSecCore.Functions;

using AdSecGHCore.Constants;

namespace AdSecCoreTests.Functions {
  public class EditProfileFunctionTests {

    private EditProfileFunction _function;

    public EditProfileFunctionTests() {
      _function = new EditProfileFunction();
    }

    [Fact]
    public void ShouldHaveAName() {
      Assert.Equal("Edit Profile", _function.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveANickName() {
      Assert.Equal("ProfileEdit", _function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveADescription() {
      Assert.Equal("Modify an AdSec Profile", _function.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveACategory() {
      Assert.Equal(CategoryName.Name(), _function.Organisation.Category);
    }

    [Fact]
    public void ShouldHaveNoSubCategory() {
      Assert.Equal(SubCategoryName.Cat2(), _function.Organisation.SubCategory);
    }
  }
}
