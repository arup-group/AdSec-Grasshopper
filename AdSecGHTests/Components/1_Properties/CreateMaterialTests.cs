using AdSecGH.Components;

using Xunit;

namespace AdSecGHTests.Components._1_Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateMaterialTests {
    private readonly CreateCustomMaterial _component;

    public CreateMaterialTests() {
      _component = new CreateCustomMaterial();
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(_component.Params.Output);
    }
    
    [Fact]
    public void ShouldHaveSixInputs() {
      Assert.Equal(6, _component.Params.Input.Count);
    }
  }
}
