using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class ResilianceTest {
    [Fact]
    public void ShouldBeAbleToIgnoreNonExistantParameters() {
      var component = new MalformedComponent();
      component.SetDefaultValues();
      ComponentTesting.ComputeOutputs(component);
      Assert.Empty(component.Params.Input);
    }
  }
}
