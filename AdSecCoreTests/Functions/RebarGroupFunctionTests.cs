using AdSecCore.Functions;

using AdSecGHCore.Constants;

namespace AdSecCoreTests.Functions {
  public class RebarGroupFunctionTests {

    private readonly RebarGroupFunction function;

    public RebarGroupFunctionTests() {
      function = new RebarGroupFunction();
    }

    [Fact]
    public void ShouldHaveMetadata() {
      Assert.NotNull(function.Metadata.Name);
      Assert.Equal("Create Reinforcement Group", function.Metadata.Name);
      Assert.Equal("Reinforcement Group", function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveOrganisation() {
      Assert.NotNull(function.Organisation);
      Assert.Equal(CategoryName.Name(), function.Organisation.Category);
      Assert.Equal(SubCategoryName.Cat3(), function.Organisation.SubCategory);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(function.GetAllOutputAttributes());
    }
    [Fact]
    public void ShouldChangeModes() {
      Assert.Equal(FoldMode.Template, function.Mode);
      function.SetMode(FoldMode.Perimeter);
      Assert.Equal(FoldMode.Perimeter, function.Mode);
    }

    [Fact]
    public void ShouldHaveTemplateWithFourInputs() {
      function.SetMode(FoldMode.Template);
      Assert.Equal(4, function.GetAllInputAttributes().Length);
    }

  }
}
