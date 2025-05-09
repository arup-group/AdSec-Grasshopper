using AdSecGH.Components;

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
  }
}
