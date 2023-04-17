using Grasshopper.Kernel.Types;
using OasysGH;
using OasysGH.Parameters;

namespace AdSecGH.Parameters {
  /// <summary>
  /// Goo wrapper class, makes sure this can be used in Grasshopper.
  /// </summary>
  public class AdSecMaterialGoo : GH_OasysGoo<AdSecMaterial> {
    public static string Description => "AdSec Material Parameter";
    public static string Name => "Material";
    public static string NickName => "Mat";
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;

    public AdSecMaterialGoo(AdSecMaterial item) : base(item) {
    }

    public override IGH_Goo Duplicate() => new AdSecMaterialGoo(this.Value);
  }
}
