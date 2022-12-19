using System;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Materials;
using OasysGH;
using OasysGH.Parameters;
using OasysUnits;

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
    public override IGH_Goo Duplicate() => new AdSecConcreteCrackCalculationParametersGoo(this.Value);
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;

    public AdSecConcreteCrackCalculationParametersGoo(IConcreteCrackCalculationParameters item) : base(item) { }

    public override string ToString()
    {
      // recreate pressure values with document units
      Pressure e = new Pressure(this.Value.ElasticModulus.As(Units.StressUnit), Units.StressUnit);
      Pressure fck = new Pressure(this.Value.CharacteristicCompressiveStrength.As(Units.StressUnit), Units.StressUnit);
      Pressure ftk = new Pressure(this.Value.CharacteristicTensileStrength.As(Units.StressUnit), Units.StressUnit);
      return "AdSec " + TypeName +
        " {E:" + e.ToString() +
        ", fc:" + fck.ToString() +
        ", ft: " + ftk.ToString() + "}";
    }
  }
}
