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

    [Fact]
    public void ShouldHaveFourInputs() {
      Assert.Equal(4, _function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveProfileInput() {
      Assert.Equal("Profile", _function.Profile.Name);
    }

    [Fact]
    public void ShouldHaveRotationInput() {
      Assert.Equal("R", _function.Rotation.NickName);
    }

    [Fact]
    public void ShouldHaveIsReflectedY() {
      Assert.Equal("isReflectedY", _function.ReflectedY.Name);
    }

    [Fact]
    public void ShouldHaveIsReflectedZ() {
      Assert.Equal("isReflectedZ", _function.ReflectedZ.Name);
    }

    [Fact]
    public void ShouldIncludeItemsAtOrder() {
      var inputs = _function.GetAllInputAttributes();
      Assert.Equal(_function.Profile, inputs[0]);
      Assert.Equal(_function.Rotation, inputs[1]);
      Assert.Equal(_function.ReflectedY, inputs[2]);
      Assert.Equal(_function.ReflectedZ, inputs[3]);
    }

  }
}
