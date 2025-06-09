using AdSecCore.Constants;
using AdSecCore.Functions;

using AdSecGHCore.Constants;
using AdSecGHCore.Functions;

namespace AdSecCoreTests.Functions {
  public class OpenModelFunctionTests {
    private OpenModelFunction _function;

    public OpenModelFunctionTests() {
      AdSecFileHelper.LoadMode = AdSecDllLoader.LoadMode.LoadFrom;
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

    [Fact]
    public void ShouldLoadAnAdsJsonFile() {
      _function.Path.Value = "simple.ads";
      _function.Compute();
      Assert.NotNull(_function.Sections.Value);
    }

    [Fact]
    public void ShouldLoadWithDesignCode() {
      _function.Path.Value = "simple.ads";
      _function.Compute();
      Assert.NotNull(_function.Sections.Value[0].DesignCode);
      Assert.NotNull(_function.Sections.Value[0]);
      Assert.Equal("EN1992", _function.Sections.Value[0].DesignCode.DesignCodeName);
    }

    [Fact]
    public void ShouldLoadWithDefaultDesignCode() {
      _function.Path.Value = "simple.ads";
      AdSecFileHelper.CodesStrings.Remove("EC2_GB_04");
      _function.Compute();
      AdSecFileHelper.CodesStrings.Add("EC2_GB_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+GB+Edition_2014");
      Assert.Equal("EC2_04", _function.Sections.Value[0].DesignCode.DesignCodeName);
    }

    [Fact]
    public void ShouldAddWarningIfFileContainsNoSections() {
      _function.Path.Value = "missing_sections.ads";
      _function.Compute();
      Assert.Empty(_function.Sections.Value);
      Assert.Contains("File contains no valid sections", _function.WarningMessages);
    }

    [Fact]
    public void ShouldExposeTheWarningsFromJsonParser() {
      _function.Path.Value = "sections_with_warnings.ads";
      _function.Compute();
      Assert.Contains("At least one section has tasks which is not currently supported and have been ignored", _function.WarningMessages);

    }
  }
}
