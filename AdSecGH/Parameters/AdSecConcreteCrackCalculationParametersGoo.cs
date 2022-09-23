using System;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Materials;
using OasysGH;
using OasysGH.Parameters;

namespace AdSecGH.Parameters
{
  /// <summary>
  /// Goo wrapper class, makes sure <see cref="IConcreteCrackCalculationParameters"/> can be used in Grasshopper.
  /// </summary>
  public class AdSecConcreteCrackCalculationParametersGoo : GH_OasysGoo<IConcreteCrackCalculationParameters>
  {
    public static string Name => "Crack Calc Params";
    public static string NickName => "CrackCalcParams";
    public static string Description => "AdSec Concrete Crack Calculation Parameters";
    public AdSecConcreteCrackCalculationParametersGoo(IConcreteCrackCalculationParameters item) : base(item) { }
    public override IGH_Goo Duplicate() => new AdSecConcreteCrackCalculationParametersGoo(this.Value);
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;

    public AdSecConcreteCrackCalculationParametersGoo(UnitsNet.Pressure elasticModulus, UnitsNet.Pressure characteristicCompressiveStrength, UnitsNet.Pressure characteristicTensionStrength)
    {
      this.Value = IConcreteCrackCalculationParameters.Create(
          elasticModulus,
          characteristicCompressiveStrength,
          characteristicTensionStrength);
    }

    public AdSecConcreteCrackCalculationParametersGoo(double elasticModulus, double characteristicCompressiveStrength, double characteristicTensionStrength)
    {
      this.Value = IConcreteCrackCalculationParameters.Create(
          new UnitsNet.Pressure(elasticModulus, Units.StressUnit),
          new UnitsNet.Pressure(characteristicCompressiveStrength, Units.StressUnit),
          new UnitsNet.Pressure(characteristicTensionStrength, Units.StressUnit));
    }

    public override string ToString()
    {
      // recreate pressure values with document units
      UnitsNet.Pressure e = new UnitsNet.Pressure(this.Value.ElasticModulus.As(Units.StressUnit), Units.StressUnit);
      UnitsNet.Pressure fck = new UnitsNet.Pressure(this.Value.CharacteristicCompressiveStrength.As(Units.StressUnit), Units.StressUnit);
      UnitsNet.Pressure ftk = new UnitsNet.Pressure(this.Value.CharacteristicTensileStrength.As(Units.StressUnit), Units.StressUnit);
      return "AdSec " + TypeName +
        " {E:" + e.ToString() +
        ", fc:" + fck.ToString() +
        ", ft: " + ftk.ToString() + "}";
    }
  }
}
