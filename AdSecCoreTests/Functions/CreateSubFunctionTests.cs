using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class CreateSubFunctionTests {

    private readonly CreateSubComponentFunction function;

    public CreateSubFunctionTests() {
      function = new CreateSubComponentFunction();
    }

    [Fact]
    public void ShouldHaveTwoInputs() {
      Assert.Equal(2, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldProduceAValidSubComponent() {
      function.Section.Value = new SectionDesign {
        Section = SectionBuilder.Get100Section(),
      };
      function.Compute();
      Assert.NotNull(function.SubComponent.Value);
    }

    [Fact]
    public void ShouldProduceAValidSubComponentWithOffset() {
      function.Section.Value = new SectionDesign {
        Section = SectionBuilder.Get100Section(),
      };
      function.Offset.Value = IPoint.Create(Length.FromMillimeters(100), Length.FromMillimeters(100));
      function.Compute();
      Assert.NotNull(function.SubComponent.Value);
      var valueSubComponent = function.SubComponent.Value.ISubComponent;
      Assert.Equal(100, valueSubComponent.Offset.Y.As(LengthUnit.Millimeter));
      Assert.Equal(100, valueSubComponent.Offset.Z.As(LengthUnit.Millimeter));
    }

    [Fact]
    public void ShouldHaveOffsetAsOptional() {
      // Assert.True(createSubComponentFunction.GetAllInputAttributes()[1].Optional);
    }
  }
}
