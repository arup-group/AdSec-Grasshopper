using AdSecGH.Components;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateSectionTests {
    readonly CreateSection component;

    public CreateSectionTests() {
      component = new CreateSection();
    }

    [Fact]
    public void shouldHaveFourInputs() {
      Assert.Equal(4, component.Params.Input.Count);
    }

    [Fact]
    public void shouldHaveOneOutput() {
      Assert.Single(component.Params.Output);
    }
  }
}
