using AdSecCore.Functions;

using AdSecGHCore.Constants;

namespace AdSecCoreTests.Functions {
  public class CreateRebarFunctionTests {
    private CreateRebarFunction function;

    public CreateRebarFunctionTests() {
      function = new CreateRebarFunction();
    }

    [Fact]
    public void ShouldHaveAName() {
      Assert.Equal("Create Rebar", function.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveANickName() {
      Assert.Equal("Rebar", function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveADescription() {
      Assert.Equal("Create Rebar (single or bundle) for an AdSec Section", function.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveCategoryCat3() {
      Assert.Equal(SubCategoryName.Cat3(), function.Organisation.SubCategory);
    }

    [Fact]
    public void ShouldHaveSubCategoryCat3() {
      Assert.Equal(CategoryName.Name(), function.Organisation.Category);
    }
  }
}
