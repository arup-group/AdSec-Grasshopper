using AdSecGH.Components;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateSubComponentTests {
    private readonly CreateSubcomponent component;

    public CreateSubComponentTests() {
      component = new CreateSubcomponent();
    }

    [Fact]
    public void ShouldHaveTwoInputs() {
      Assert.Equal(2, component.Params.Input.Count);
    }
  }
}
