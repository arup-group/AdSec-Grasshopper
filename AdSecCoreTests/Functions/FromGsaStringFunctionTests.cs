using AdSecCore.Constants;
using AdSecCore.Functions;

using AdSecGHCore.Constants;

namespace AdSecCoreTests.Functions {
  public class FromGsaStringFunctionTests {
    private readonly FromGsaStringFunction _function;

    public FromGsaStringFunctionTests() {
      _function = new FromGsaStringFunction();
    }

    [Fact]
    public void ShouldHaveName() {
      Assert.Equal("Material from GSA", _function.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveNickName() {
      Assert.Equal("MatGSA", _function.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveDescription() {
      Assert.Equal(
        "Create an AdSec catalogue concrete material from a GSA material string",
        _function.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveCategory() {
      Assert.Equal(CategoryName.Name(), _function.Organisation.Category);
    }

    [Fact]
    public void ShouldHaveSubCategory() {
      Assert.Equal(SubCategoryName.Cat1(), _function.Organisation.SubCategory);
    }

    [Fact]
    public void ShouldHaveOneInput() {
      Assert.Single(_function.GetAllInputAttributes());
    }

    [Fact]
    public void ShouldHaveStringInputNamed_GSAMaterial() {
      Assert.Equal("GSA Material", _function.Input.Name);
      Assert.Equal("GSA", _function.Input.NickName);
    }

    [Fact]
    public void ShouldHaveInputDescription() {
      Assert.Equal(
        "GSA Material string to convert to an AdSec catalogue material",
        _function.Input.Description);
    }

    [Fact]
    public void ShouldHaveInputAtFirstPosition() {
      var inputs = _function.GetAllInputAttributes();
      Assert.Equal(_function.Input, inputs[0]);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(_function.GetAllOutputAttributes());
    }

    [Fact]
    public void ShouldHaveMaterialOutput() {
      Assert.Equal("Material", _function.Material.Name);
    }

    [Fact]
    public void ShouldHaveMaterialOutputDescription() {
      Assert.Equal(
        "AdSec catalogue concrete material derived from the GSA material string",
        _function.Material.Description);
    }

    [Fact]
    public void ShouldHaveOutputAtFirstPosition() {
      var outputs = _function.GetAllOutputAttributes();
      Assert.Equal(_function.Material, outputs[0]);
    }

    [Fact]
    public void ShouldAddErrorWhenInputIsNull() {
      _function.Input.Value = null;
      _function.Compute();

      Assert.Single(_function.ErrorMessages);
    }

    [Fact]
    public void ShouldAddWarningForUnsupportedCode() {
      _function.Input.Value = "GSA Material (UNKNOWN CODE Concrete C30/37)";
      _function.Compute();

      Assert.Empty(_function.ErrorMessages);
      Assert.Single(_function.WarningMessages);
    }

    [Fact]
    public void ShouldStillReturnMaterialForUnsupportedCode() {
      _function.Input.Value = "GSA Material (UNKNOWN CODE Concrete C30/37)";
      _function.Compute();

      Assert.NotNull(_function.Material.Value);
      Assert.Equal("EN1992+Part1_1+Edition_2004+NationalAnnex+NoNationalAnnex",
        _function.Material.Value.DesignCode.DesignCodeName);
    }

    [Fact]
    public void ShouldAddNoWarningForExactMatch() {
      _function.Input.Value = "GSA Material (EC2-1-1 Concrete C12/15)";
      _function.Compute();

      Assert.Empty(_function.WarningMessages);
    }

    [Fact]
    public void ShouldComputeSuccessfullyForEc2Material() {
      _function.Input.Value = "GSA Material (EC2-1-1 Concrete C12/15)";
      _function.Compute();

      Assert.Empty(_function.ErrorMessages);
      Assert.NotNull(_function.Material.Value);
    }

    [Fact]
    public void ShouldSetGradeNameForEc2Material() {
      _function.Input.Value = "GSA Material (EC2-1-1 Concrete C12/15)";
      _function.Compute();

      Assert.Equal("C12_15", _function.Material.Value.GradeName);
    }

    [Fact]
    public void ShouldSetDesignCodeForEc2Material() {
      _function.Input.Value = "GSA Material (EC2-1-1 Concrete C12/15)";
      _function.Compute();

      Assert.NotNull(_function.Material.Value.DesignCode.IDesignCode);
    }

    [Fact]
    public void ShouldComputeSuccessfullyForCsaMaterial() {
      _function.Input.Value = "GSA Material (CSA A23.3-14 Concrete 20 MPa)";
      _function.Compute();

      Assert.Empty(_function.ErrorMessages);
      Assert.NotNull(_function.Material.Value);
    }
  }
}
