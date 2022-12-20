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
    public override IGH_Goo Duplicate() => new AdSecConcreteCrackCalculationParametersGoo(this.Value);
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;

    public AdSecConcreteCrackCalculationParametersGoo(IConcreteCrackCalculationParameters item) : base(item) { }

    public override string ToString()
    {
      return "AdSec " + TypeName +
        " {E:" + this.Value.ElasticModulus.ToString() +
        ", fc:" + this.Value.CharacteristicCompressiveStrength.ToString() +
        ", ft: " + this.Value.CharacteristicTensileStrength.ToString() + "}";
    }
  }
}
