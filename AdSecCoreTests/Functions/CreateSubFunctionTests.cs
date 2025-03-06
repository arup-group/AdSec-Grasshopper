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
      function.Section.Value = new SectionDesign {
        Section = SectionBuilder.Get100Section(),
      };
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
      function.Offset.Value = IPoint.Create(Length.FromMillimeters(100), Length.FromMillimeters(100));
      function.Compute();
      Assert.NotNull(function.SubComponent.Value);
      var valueSubComponent = function.SubComponent.Value.ISubComponent;
      Assert.Equal(100, valueSubComponent.Offset.Y.As(LengthUnit.Millimeter));
      Assert.Equal(100, valueSubComponent.Offset.Z.As(LengthUnit.Millimeter));
    }

    [Fact(Skip = "Need to bring the changes of optional parameters")]
    public void ShouldHaveOffsetAsOptional() {
      function.Offset.Value = null;
      function.Compute();
      Assert.NotNull(function.SubComponent.Value);
    }
  }
}
