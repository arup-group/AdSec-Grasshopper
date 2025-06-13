using AdSecGHCore.Constants;

public class CreateCustomMaterialFunctionTests {
  CreateCustomMaterialFunction _function;

  public CreateCustomMaterialFunctionTests() {
    _function = new CreateCustomMaterialFunction();
  }

  [Fact]
  public void ShouldHaveAName() {
    Assert.Equal("Custom Material", _function.Metadata.Name);
  }

  [Fact]
  public void ShouldHaveANickName() {
    Assert.Equal("CustomMaterial", _function.Metadata.NickName);
  }

  [Fact]
  public void ShouldHaveADescription() {
    Assert.Equal("Create a custom AdSec Material", _function.Metadata.Description);
  }

  [Fact]
  public void ShouldHaveACategory() {
    Assert.Equal(CategoryName.Name(), _function.Organisation.Category);
  }

  [Fact]
  public void ShouldHaveNoSubCategory() {
    Assert.Equal(SubCategoryName.Cat1(), _function.Organisation.SubCategory);
  }

}
