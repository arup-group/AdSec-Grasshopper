using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.GH.Helpers;

using OasysGH.Components;

using Xunit;

namespace AdSecGHTests.Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateConcreteCrackCalculationParametersTests {

    public static GH_OasysDropDownComponent ComponentMother() {
      var comp = new CreateConcreteCrackParameters();
      comp.CreateAttributes();

      comp.SetSelected(0, 0); // change dropdown to ?

      ComponentTestHelper.SetInput(comp, 130);
      ComponentTestHelper.SetInput(comp, 1700, 1);
      ComponentTestHelper.SetInput(comp, 1200, 2);

      return comp;
    }

    [Fact]
    public void ChangeDropDownTest() {
      var comp = ComponentMother();
      Assert.True(OasysDropDownComponentTestHelper.ChangeDropDownTest(comp));
    }

    [Fact]
    public void CreateComponent() {
      var comp = ComponentMother();
      comp.SetSelected(0, 0); // change dropdown to ?
      var output = (AdSecConcreteCrackCalculationParametersGoo)ComponentTestHelper.GetOutput(comp);
      Assert.NotNull(output);
    }

    [Fact]
    public void DeserializeTest() {
      var comp = ComponentMother();
      Assert.Empty(OasysDropDownComponentTestHelper.DeserializeTest(comp));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      var _component = ComponentMother();
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      var _component = ComponentMother();
      Assert.True(_component.MatchesExpectedIcon(Resources.CreateCrackCalcParams));
    }
  }
}
