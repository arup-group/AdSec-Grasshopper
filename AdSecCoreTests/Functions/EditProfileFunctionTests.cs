using AdSecCore.Functions;

using AdSecGHCore.Constants;

using OasysUnits.Units;

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

    [Fact]
    public void ShouldIncludeDropdownWithAngleUnits() {
      Assert.Contains(_function.Options(), x => x.GetType() == typeof(UnitOptions));
    }

    [Fact]
    public void ShouldUpdateNameForRotationBasedOnUnit() {
      _function.LocalAngleUnit = AngleUnit.Degree;
      _function.UpdateUnits();
      Assert.Contains("°", _function.Rotation.Name);
    }

    [Fact]
    public void ShouldHaveDefaultRadians() {
      Assert.Contains("rad", _function.Rotation.Name);
    }

    [Fact]
    public void ShouldDropdownWithDescriptionMeasure() {
      Assert.Equal("Measure", _function.Options()[0].Description);
    }

    [Fact]
    public void ShouldDropdownWithUnitTypeAngleUnit() {
      Assert.Equal(typeof(AngleUnit), (_function.Options()[0] as UnitOptions).UnitType);
    }
  }
}
