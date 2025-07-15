using Grasshopper.Kernel.Types;

using Oasys.AdSec.Materials;

using OasysGH;
using OasysGH.Parameters;

namespace AdSecGH.Parameters {
  /// <summary>
  /// Goo wrapper class, makes sure <see cref="IConcreteCrackCalculationParameters"/> can be used in Grasshopper.
  /// </summary>
  public class AdSecConcreteCrackCalculationParametersGoo : GH_OasysGoo<IConcreteCrackCalculationParameters> {
    public static string Description => "AdSec Concrete Crack Calculation Parameters";
    public static string Name => "Crack Calc Params";
    public static string NickName => "CrackCalcParams";
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;

    public AdSecConcreteCrackCalculationParametersGoo(IConcreteCrackCalculationParameters item) : base(item) {
    }

    public override IGH_Goo Duplicate() {
      return new AdSecConcreteCrackCalculationParametersGoo(Value);
    }

    public override string ToString() {
      if (Value == null) {
        return string.Empty;
      }
      return
        $"AdSec {TypeName} {{E: {Value.ElasticModulus}, fc: {Value.CharacteristicCompressiveStrength}, ft: {Value.CharacteristicTensileStrength}}}";
    }
  }
}
