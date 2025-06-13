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

  [Fact]
  public void ShouldHaveSixInputs() {
    Assert.Equal(6, _function.GetAllInputAttributes().Length);
  }

  [Fact]
  public void ShouldHaveDesignCode() {
    Assert.Equal("DesignCode", _function.DesignCode.Name);
  }

  [Fact]
  public void ShouldExplainDesignCodeIsOptional() {
    Assert.Equal("[Optional] Set the Material's DesignCode", _function.DesignCode.Description);
  }

  [Fact]
  public void ShouldHaveDesignCodeOptional() {
    Assert.True(_function.DesignCode.Optional);
  }

  [Fact]
  public void ShouldHaveUlsCrv() {
    Assert.Equal("ULS Comp. Crv", _function.UlsCurve.Name);
  }

}
