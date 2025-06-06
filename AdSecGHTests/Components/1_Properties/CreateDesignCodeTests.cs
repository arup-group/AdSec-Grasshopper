using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components.Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateDesignCodeTests {
    private readonly CreateDesignCode _component;
    public CreateDesignCodeTests() {
      AdSecUtility.LoadAdSecAPI();
      _component = new CreateDesignCode();
      SetACI318Edition2002Metrric();
    }

    private void SetACI318Edition2002Metrric() {
      _component.SetSelected(0, 1);
    }

    private AdSecMaterialGoo GetDesignCodeFromMaterial() {
      var component = new CreateStandardMaterial();
      component.SetSelected(0, 0);
      component.SetSelected(1, 0);
      return (AdSecMaterialGoo)ComponentTestHelper.GetOutput(component);
    }

    [Fact]
    public void ExpectedDesignCodeIsACI318Edition2002Metric() {
      var output = (AdSecDesignCodeGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal("ACI318+Edition_2002+Metric", output.Value.DesignCodeName);
    }

    [Fact]
    public void DesignCodeCreatedFromDesignCodeOrThroughConcreteMaterialShouldBeConsistent() {
      var designFromDesignCode = ((AdSecDesignCodeGoo)ComponentTestHelper.GetOutput(_component)).Value;
      var designCodeFromMaterial = GetDesignCodeFromMaterial().Value.DesignCode;
      Assert.Equal(designCodeFromMaterial.DesignCodeName, designFromDesignCode.DesignCodeName);
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.CreateDesignCode));
    }

  }
}
