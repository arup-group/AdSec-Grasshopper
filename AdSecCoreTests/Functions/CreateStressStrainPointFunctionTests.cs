
using AdSecCore.Functions;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecCore.Tests.Functions {
  public class StressStrainPointFunctionTests {
    private readonly StressStrainPointFunction _function;

    public StressStrainPointFunctionTests() {
      _function = new StressStrainPointFunction();
    }

    [Fact]
    public void DefaultPropertiesShouldBeCorrect() {
      Assert.Equal("Create Stress-Strain Point", _function.Metadata.Name);
      Assert.Equal("SSPoint", _function.Metadata.NickName);
      Assert.Equal(StrainUnit.MilliStrain, _function.LocalStrainUnit);
      Assert.Equal(PressureUnit.Megapascal, _function.LocalStressUnit);
      Assert.False(_function.StrainInput.Optional);
      Assert.False(_function.StressInput.Optional);
    }

    [Theory]
    [InlineData(0.002, 30, StrainUnit.Ratio, PressureUnit.Megapascal)]
    [InlineData(2, 30000, StrainUnit.MilliStrain, PressureUnit.Kilopascal)]
    [InlineData(0.2, 30, StrainUnit.Percent, PressureUnit.Megapascal)]
    public void ComputeWithValidInputsShouldCreatePoint(
        double strainValue,
        double stressValue,
        StrainUnit strainUnit,
        PressureUnit stressUnit) {

      _function.LocalStrainUnit = strainUnit;
      _function.LocalStressUnit = stressUnit;
      _function.StrainInput.Value = strainValue;
      _function.StressInput.Value = stressValue;
      _function.Compute();

      Assert.NotNull(_function.StressAndStrainOutput.Value);
      var point = _function.StressAndStrainOutput.Value;
      Assert.Equal(new Strain(strainValue, strainUnit).As(StrainUnit.Ratio),
          point.Strain.As(StrainUnit.Ratio), 6);
      Assert.Equal(new Pressure(stressValue, stressUnit).As(PressureUnit.Megapascal),
          point.Stress.As(PressureUnit.Megapascal), 3);
    }

    [Fact]
    public void UpdateUnits_ShouldUpdateUnitResults() {
      _function.LocalStrainUnit = StrainUnit.Percent;
      _function.LocalStressUnit = PressureUnit.Kilopascal;
      _function.UpdateUnits();
      Assert.Equal(StrainUnit.Percent, _function.StrainUnitResult);
      Assert.Equal(PressureUnit.Kilopascal, _function.StressUnitResult);
    }

    [Fact]
    public void UpdateParameterShouldUpdateInputNames() {
      _function.LocalStrainUnit = StrainUnit.Percent;
      _function.LocalStressUnit = PressureUnit.Kilopascal;
      _function.UpdateUnits();
      Assert.Contains("%", _function.StrainInput.Name);
      Assert.Contains("kPa", _function.StressInput.Name);
    }

    [Fact]
    public void OptionsShouldReturnCorrectUnitOptions() {
      var options = _function.Options().OfType<UnitOptions>().ToList();
      Assert.Equal(2, options.Count);
      Assert.Equal("Strain Unit", options[0].Description);
      Assert.Equal("Stress Unit", options[1].Description);
      Assert.Equal(typeof(StrainUnit), options[0].UnitType);
      Assert.Equal(typeof(PressureUnit), options[1].UnitType);
    }
  }
}
