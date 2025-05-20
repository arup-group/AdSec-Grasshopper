using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGHCore.Constants;

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
        LocalPlane = OasysPlane.PlaneXY,
      };
    }

    [Fact]
    public void ShouldHaveTwoInputs() {
      Assert.Equal(2, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldMaintainThePlane() {
      function.Compute();
      Assert.Equal(OasysPlane.PlaneXY, function.SubComponent.Value.SectionDesign.LocalPlane);
    }

    [Fact]
    public void ShouldHaveASectionParameterNamedSection() {
      Assert.Equal("Section", function.Section.Name);
    }

    [Fact]
    public void ShouldHaveAnSubCompAsSubComponent() {
      Assert.Equal("SubComponent", function.SubComponent.Name);
    }

    [Fact]
    public void ShouldHaveAnOffsetParameter() {
      Assert.Equal("Offset", function.Offset.Name);
    }

    [Fact]
    public void ShouldBeInTheCategoryAdSec() {
      Assert.Equal(CategoryName.Name(), function.Organisation.Category);
    }

    [Fact]
    public void ShouldBeInTheSubCategoryAdSec() {
      Assert.Equal(SubCategoryName.Cat4(), function.Organisation.SubCategory);
    }

    [Fact]
    public void ShouldBeNamedSubComponent() {
      Assert.Equal("SubComponent", function.Metadata.Name);
    }

    [Fact]
    public void ShouldBeNickNamedSubComponent() {
      Assert.Equal("SubComponent", function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveDescription() {
      Assert.Equal("Create an AdSec Subcomponent from a Section", function.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(function.GetAllOutputAttributes());
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

    [Fact]
    public void ShouldHaveOffsetAsOptional() {
      function.Offset.Value = null;
      function.Compute();
      Assert.NotNull(function.SubComponent.Value);
    }
  }
}
