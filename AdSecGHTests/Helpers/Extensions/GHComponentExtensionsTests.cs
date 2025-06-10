using AdSecGH.Components;
using AdSecGH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers.Extensions {
  [Collection("GrasshopperFixture collection")]
  public class GHComponentExtensionsTests {
    [Fact]
    public void ClearingSources() {
      var component = new SaveModel();
      component.Params.Input[0].Sources.Add(null);

      Assert.NotEmpty(component.Params.Input[0].Sources);

      component.RemoveSourcesFromInputAt(0);

      Assert.Empty(component.Params.Input[0].Sources);
    }

    [Fact]
    public void CanAddPanelIntoComponent() {
      var component = new SaveModel();
      Assert.Empty(component.Params.Input[0].Sources);

      component.AddPanelForInputAt(0, "test", new Dummies.DummyContext());
      Assert.Single(component.Params.Input[0].Sources);
    }
  }
}
