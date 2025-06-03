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

    [Fact]
    public void ShouldHaveFourInputs() {
      Assert.Equal(2, _function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveFourOutputs() {
      Assert.Single(_function.GetAllOutputAttributes());
    }

    [Fact]
    public void ShouldHaveARebarInput() {
      Assert.Equal("Rebar", _function.Rebar.Name);
    }

    [Fact]
    public void ShouldHaveASpacingInput() {
      Assert.Equal("S", _function.Spacing.NickName);
    }

    [Fact]
    public void ShouldHaveACountInput() {
      Assert.Equal("Count", _function.Count.Name);
    }

    [Fact]
    public void ShouldHaveDefaultInputOrder() {
      var inputs = _function.GetAllInputAttributes();
      Assert.Equal(_function.Rebar, inputs[0]);
      Assert.Equal(_function.Spacing, inputs[1]);
    }

    [Fact]
    public void ShouldHaveALayer() {
      Assert.Equal("Spaced Rebars", _function.SpacedRebars.Name);
    }

    [Fact]
    public void ShouldHaveOutputParam() {
      var outputs = _function.GetAllOutputAttributes();
      Assert.Equal(_function.SpacedRebars, outputs[0]);
    }

    [Fact]
    public void ShouldIncludeTheUnitInTheSpacingName() {
      Assert.Contains("[m]", _function.Spacing.Name);
    }

    [Fact]
    public void ShouldHaveDropdownForMeasure() {
      Assert.Contains(_function.Options(), x => x.Description == "Measure");
    }

    [Fact]
    public void ShouldHaveDropdownForSpacingMethod() {
      Assert.Contains(_function.Options(), x => x.Description == "Spacing method");
    }

    [Fact]
    public void ShouldChangeInputFromDistanceToCount() {
      _function.SetMode(CreateRebarSpacingFunction.FoldMode.Count);
      Assert.Equal(_function.Count, _function.GetAllInputAttributes()[1]);
    }

    [Fact]
    public void ShouldHaveOneDropdownForCount() {
      _function.SetMode(CreateRebarSpacingFunction.FoldMode.Count);
      Assert.Single(_function.Options());
    }
  }
}
