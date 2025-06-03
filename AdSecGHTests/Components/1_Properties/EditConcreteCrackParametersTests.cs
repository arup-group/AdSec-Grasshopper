using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._02_Properties {
  [Collection("GrasshopperFixture collection")]
  public class EditConcreteCrackParametersTests {
    private readonly EditConcreteCrackCalculationParameters _component;

    public EditConcreteCrackParametersTests() {
      _component = new EditConcreteCrackCalculationParameters();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.EditCrackCalcParams));
    }
  }
}
