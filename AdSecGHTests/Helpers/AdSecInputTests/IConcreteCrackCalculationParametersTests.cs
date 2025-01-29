using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class IConcreteCrackCalculationParametersTests {
    private IConcreteCrackCalculationParameters _parameters;

    public IConcreteCrackCalculationParametersTests() {
      _parameters = null;
    }

    [Fact]
    public void TryCastToIConcreteCrackCalculationParametersReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToConcreteCrackCalculationParameters(objwrap, ref _parameters);

      Assert.False(castSuccessful);
      Assert.Null(_parameters);
    }

    [Fact]
    public void
      TryCastToIConcreteCrackCalculationParametersReturnsCorrectDataFromIConcreteCrackCalculationParameters() {
      var pressure = new Pressure(-0.5, PressureUnit.Bar);
      var pressure2 = new Pressure(1, PressureUnit.Bar);
      var input = IConcreteCrackCalculationParameters.Create(pressure2, pressure, pressure2);

      var objwrap = new GH_ObjectWrapper(input);
      bool castSuccessful = AdSecInput.TryCastToConcreteCrackCalculationParameters(objwrap, ref _parameters);

      Assert.True(castSuccessful);
      Assert.NotNull(_parameters);
    }

    [Fact]
    public void
      TryCastToIConcreteCrackCalculationParametersReturnsCorrectDataFromAdSecConcreteCrackCalculationParametersGoo() {
      var pressure = new Pressure(-0.5, PressureUnit.Bar);
      var pressure2 = new Pressure(1, PressureUnit.Bar);
      var input = new AdSecConcreteCrackCalculationParametersGoo(
        IConcreteCrackCalculationParameters.Create(pressure2, pressure, pressure2));

      var objwrap = new GH_ObjectWrapper(input);
      bool castSuccessful = AdSecInput.TryCastToConcreteCrackCalculationParameters(objwrap, ref _parameters);

      Assert.True(castSuccessful);
      Assert.NotNull(_parameters);
    }
  }
}
