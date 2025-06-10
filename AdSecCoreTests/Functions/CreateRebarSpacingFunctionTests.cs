using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGHCore.Constants;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Layers;

namespace AdSecCoreTests.Functions {
  public class CreateRebarSpacingFunctionTests {
    CreateRebarSpacingFunction _function;

    public CreateRebarSpacingFunctionTests() {
      _function = new CreateRebarSpacingFunction();
      var singleBars = new BuilderSingleBar().AtPosition(Geometry.Zero()).Build().BarBundle;
      _function.Rebar.Value = singleBars as IBarBundle;
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
      _function.SetMode(SpacingMode.Count);
      Assert.Equal(_function.Count, _function.GetAllInputAttributes()[1]);
    }

    [Fact]
    public void ShouldHaveOneDropdownForCount() {
      _function.SetMode(SpacingMode.Count);
      Assert.Single(_function.Options());
    }

    [Fact]
    public void ShouldNotifyWhenModeIsChangedToUpdateInputs() {
      bool wasCalled = false;
      _function.OnVariableInputChanged += () => wasCalled = true;
      _function.SetMode(SpacingMode.Count);
      Assert.True(wasCalled);
    }

    [Fact]
    public void ShouldNotifyWhenModeIsChangedToUpdateDropdowns() {
      bool wasCalled = false;
      _function.OnDropdownChanged += () => wasCalled = true;
      _function.SetMode(SpacingMode.Count);
      Assert.True(wasCalled);
    }

    [Fact]
    public void ShouldComputeByDistance() {
      _function.SetMode(SpacingMode.Distance);
      _function.Spacing.Value = 0.1;
      _function.Compute();
      var layer = _function.SpacedRebars.Value as ILayerByBarPitch;
      Assert.NotNull(layer);
      Assert.Equal(0.1, layer.Pitch.Value);
    }

    [Fact]
    public void ShouldComputeByCount() {
      _function.SetMode(SpacingMode.Count);
      _function.Count.Value = 1;
      _function.Compute();
      var layer = _function.SpacedRebars.Value as ILayerByBarCount;
      Assert.NotNull(layer);
      Assert.Equal(1, layer.Count);
    }

    [Fact]
    public void ShouldNotComputeIfCountIsZero() {
      _function.SetMode(SpacingMode.Count);
      _function.Count.Value = 0;
      _function.Compute();
      Assert.Single(_function.ErrorMessages);
    }

    [Fact]
    public void ShouldNotComputeIfDistanceIsZero() {
      _function.SetMode(SpacingMode.Distance);
      _function.Spacing.Value = 0;
      _function.Compute();
      Assert.Single(_function.ErrorMessages);
    }

    [Fact]
    public void ShouldNotUpdateDropdownOrVariableInputsIfIfSave() {
      _function.SetMode(SpacingMode.Count);
      bool variableChanged = false;
      _function.OnVariableInputChanged += () => variableChanged = true;
      bool dropdownChanged = false;
      _function.OnDropdownChanged += () => dropdownChanged = true;
      _function.SetMode(SpacingMode.Count);

      Assert.False(variableChanged);
      Assert.False(dropdownChanged);
    }
  }
}
