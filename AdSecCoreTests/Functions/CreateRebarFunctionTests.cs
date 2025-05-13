using AdSecCore.Functions;

using AdSecGHCore.Constants;

using Oasys.AdSec.StandardMaterials;

using OasysUnits.Units;

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

    [Fact]
    public void ShouldHaveDefaultRebarModeSingle() {
      Assert.Equal(RebarMode.Single, function.Mode);
    }

    [Fact]
    public void ShouldHaveTwoDropdowns() {
      Assert.Equal(2, function.Options.Length);
    }

    [Fact]
    public void ShouldHaveMaterialParameter() {
      Assert.Equal("Material", function.MaterialParameter.Name);
    }

    [Fact]
    public void ShouldHaveRebarBundleParameter() {
      Assert.Equal("Rebar", function.RebarBundleParameter.Name);
    }

    [Fact]
    public void ShouldHaveCountParameter() {
      Assert.Equal("Count", function.CountParameter.Name);
    }

    [Fact]
    public void ShouldHaveARebarTypeDropdown() {
      Assert.Equal("Rebar Type", function.Options[0].Description);
    }

    [Fact]
    public void ShouldHaveAMeasureDropdown() {
      Assert.Equal("Measure", function.Options[1].Description);
    }

    [Fact]
    public void ShouldHaveDropdownForLengthUnit() {
      Assert.Equal(typeof(LengthUnit), (function.Options[1] as UnitOptions)?.UnitType);
    }

    [Fact]
    public void ShouldUpdateDiameterWhenUnitsChange() {
      var name = function.DiameterParameter.Name;
      function.LengthUnitGeometry = LengthUnit.Millimeter;
      Assert.NotEqual(name, function.DiameterParameter.Name);
    }

    [Fact]
    public void ShouldHaveInitialValueWithUnits() {
      Assert.Equal("Diameter [m]", function.DiameterParameter.Name);
    }

    [Fact]
    public void ShouldHaveThreeInputsOnBundle() {
      function.SetMode(RebarMode.Bundle);
      Assert.Equal(3, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldTriggerEventWhenChangingModes() {
      bool hasRun = false;
      function.OnVariableInputChanged += () => { hasRun = true; };
      function.SetMode(RebarMode.Bundle);
      Assert.True(hasRun);
    }

    [Fact]
    public void ShouldNotTriggerWhenYouAreOnCurrentMode() {
      bool hasRun = false;
      function.OnVariableInputChanged += () => { hasRun = true; };
      function.SetMode(RebarMode.Single);
      Assert.False(hasRun);
    }

    [Fact]
    public void ShouldHaveTwoInputsOnSingle() {
      Assert.Equal(2, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveSingleOutput() {
      Assert.Single(function.GetAllOutputAttributes());
    }

    [Fact]
    public void ShouldComputeSingleBar() {
      function.DiameterParameter.Value = 0.01;
      function.MaterialParameter.Value = new MaterialDesign() {
        Material = Reinforcement.Steel.IS456.Edition_2000.S250
      };
      function.Compute();
      Assert.NotNull(function.RebarBundleParameter.Value);
    }

    [Fact]
    public void ShouldComputeBundleBars() {
      function.DiameterParameter.Value = 0.01;
      function.MaterialParameter.Value = new MaterialDesign() {
        Material = Reinforcement.Steel.IS456.Edition_2000.S250
      };
      function.CountParameter.Value = 2;
      function.SetMode(RebarMode.Bundle);
      function.Compute();
      Assert.NotNull(function.RebarBundleParameter.Value);
    }
  }
}
