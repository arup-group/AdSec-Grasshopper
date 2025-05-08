using AdSecCore;
using AdSecCore.Functions;

using OasysUnits.Units;

namespace AdSecCoreTests.Extensions {
  public class UnitExtensionsTests {

    [Fact]
    public void NameWithUnitsShouldFormatAttribute() {
      var StrainInput = new StrainParameter {
        Name = "test",
      };
      string result = UnitExtensions.NameWithUnits(StrainInput, LengthUnit.Meter);
      Assert.Equal("test [m]", result);
    }

    [Fact]
    public void NameWithUnitsShouldFormatLengthQunatity() {
      string result = UnitExtensions.NameWithUnits("test", LengthUnit.Meter);
      Assert.Equal("test [m]", result);
    }

    [Fact]
    public void NameWithUnitsShouldFormatStrainQuantity() {
      string result = UnitExtensions.NameWithUnits("test", StrainUnit.Percent);
      Assert.Equal("test [%]", result);
    }

    [Fact]
    public void NameWithUnitsShouldFormatCurvatureQuantity() {
      string result = UnitExtensions.NameWithUnits("test", CurvatureUnit.PerMeter);
      Assert.Equal("test [m⁻¹]", result);
    }

    [Fact]
    public void NameWithUnitsShouldFormatPressureQuantity() {
      string result = UnitExtensions.NameWithUnits("test", PressureUnit.Pascal);
      Assert.Equal("test [Pa]", result);
    }

  }
}
