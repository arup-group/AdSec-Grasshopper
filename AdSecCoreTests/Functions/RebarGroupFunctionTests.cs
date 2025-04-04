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
      Assert.Equal(5, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHavePerimeterWithFourInputs() {
      function.SetMode(FoldMode.Perimeter);
      Assert.Equal(2, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveLinkWithFourInputs() {
      function.SetMode(FoldMode.Link);
      Assert.Equal(2, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveAnILayerInput() {
      function.SetMode(FoldMode.Template);
      var parameter = function.GetAllInputAttributes()[0];
      Assert.Equal(typeof(RebarLayerParameter), parameter.GetType());
    }

    [Fact]
    public void ShouldGiveAnUpdateWhenChangingModes() {
      bool isUpdated = false;
      function.OnVariableInputChanged += () => { isUpdated = true; };
      function.SetMode(FoldMode.Link);
      Assert.True(isUpdated);
    }

    [Fact]
    public void ShouldNotTriggerIfTheSameModeIsSetStartWithTemplate() {
      int triggered = 0;
      function.OnVariableInputChanged += () => { triggered++; };
      function.SetMode(FoldMode.Template);
      Assert.Equal(0, triggered);
    }

    [Fact]
    public void ShouldNotTriggerIfTheSameModeThreeDifferentOnes() {
      int triggered = 0;
      function.OnVariableInputChanged += () => { triggered++; };
      function.SetMode(FoldMode.Link);
      function.SetMode(FoldMode.Perimeter);
      Assert.Equal(2, triggered);
    }
  }
}
