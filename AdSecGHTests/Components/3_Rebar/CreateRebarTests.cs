using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;
using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._3_Rebar {
  [Collection("GrasshopperFixture collection")]
  public class CreateRebarTests {

    public CreateRebar component;

    public CreateRebarTests() {
      component = new CreateRebar();
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(component.Params.Output);
    }

    [Fact]
    public void ShouldHaveThreeInputsForOtherMode() {
      component.SetSelected(0, 1);
      Assert.Equal(3, component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveDefaultMeters() {
      Assert.Contains("[m]", component.BusinessComponent.DiameterParameter.Name);
    }

    [Fact]
    public void ShouldHaveTheUnitsChanged() {
      component.SetSelected(1, 0);
      Assert.Contains("[mm]", component.BusinessComponent.DiameterParameter.Name);
    }

    [Fact]
    public void ShouldMaintainUnitSelectionAfterCompute() {
      component.SetSelected(0, 0);
      component.SetSelected(1, 0);
      var materialDesign = new MaterialDesign() {
        Material = Reinforcement.Steel.IS456.Edition_2000.S250,
        DesignCode = new DesignCode() {
          IDesignCode = IS456.Edition_2000,
        }
      };
      var adSecMaterial = new AdSecMaterialGoo(materialDesign);
      component.SetInputParamAt(0, adSecMaterial);
      component.SetInputParamAt(1, 10);
      ComponentTestHelper.ComputeData(component);
      var output = component.GetOutputParamAt(0);
      Assert.NotNull(output);
      Assert.Equal("Diameter [mm]", component.Params.Input[1].Name);
    }

    [Fact]
    public void ShouldHaveTwoDropDowns() {
      Assert.Equal(2, component.DropDownItems.Count);
    }


    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(component.MatchesExpectedIcon(Resources.Rebar));
    }
  }
}
