using AdSecCore.Functions;

using AdSecGHCore.Constants;
using AdSecGHCore.Functions;

namespace AdSecCoreTests.Functions {
  public class OpenModelFunctionTests {
    private OpenModelFunction _function;

    public OpenModelFunctionTests() {
      _function = new OpenModelFunction();
    }

    [Fact]
    public void ShouldHaveAName() {
      Assert.Equal("Open Model", _function.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveANickName() {
      Assert.Equal("Open", _function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveADescription() {
      Assert.Equal("Open an existing AdSec .ads file", _function.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveACategory() {
      Assert.Equal(CategoryName.Name(), _function.Organisation.Category);
    }

    [Fact]
    public void ShouldHaveNoSubCategory() {
      Assert.Equal(SubCategoryName.Cat0(), _function.Organisation.SubCategory);
    }

    [Fact]
    public void ShouldHavePathWithName() {
      Assert.Equal("Filename and path", _function.Path.Name);
    }

    [Fact]
    public void ShouldHaveLocalPlaneWithName() {
      Assert.Equal("LocalPlane", _function.Plane.Name);
    }

    [Fact]
    public void ShouldHaveLocalPlaneOptional() {
      Assert.True(_function.Plane.Optional);
    }

    [Fact]
    public void ShouldHaveLocalPlaneWithDefaultYZ() {
      Assert.Equal(OasysPlane.PlaneYZ, _function.Plane.Default[0]);
    }

    [Fact]
    public void ShouldHaveAnArrayOfLocalPlane() {
      Assert.Equal(Access.List, _function.Plane.Access);
    }
    [Fact]
    public void ShouldHaveTwoInputs() {
      Assert.Equal(2, _function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldIncludeInputsInThatOrder() {
      var attributes = _function.GetAllInputAttributes();
      Assert.IsType<PathParameter>(attributes[0]);
      Assert.IsType<PlaneParameter>(attributes[1]);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(_function.GetAllOutputAttributes());
    }

    [Fact]
    public void ShouldIncludeSectionWithName() {
      Assert.Equal("Section", _function.Sections.Name);
    }

    [Fact]
    public void ShouldIncludeOutputOfType() {
      Assert.IsType<SectionArrayParameter>(_function.GetAllOutputAttributes()[0]);
    }

  }
}
