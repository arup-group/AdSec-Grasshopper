using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec.StandardMaterials;

namespace AdSecCoreTests.Functions {
  public class CreateSectionFunctionTests {

    readonly CreateSectionFunction function;

    public CreateSectionFunctionTests() {
      function = new CreateSectionFunction();

      function.Profile.Value = new ProfileDesign() {
        Profile = new ProfileBuilder().WidthDepth(1).WithWidth(2).Build()
      };
      function.Material.Value = Steel.EN1993.Edition_2005.S275;
    }

    [Fact]
    public void ShouldHaveFourInputs() {
      Assert.Equal(4, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(function.GetAllOutputAttributes());
    }

    [Fact]
    public void ShouldProduceAValidSection() {
      function.Compute();
      Assert.NotNull(function.Section.Value);
    }

    [Fact]
    public void ShouldUseTheMaterial() {
      function.Compute();
      Assert.Equal(function.Section.Value.Section.Material.GetType(), Steel.EN1993.Edition_2005.S275.GetType());
    }
  }
}
