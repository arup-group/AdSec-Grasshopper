using AdSecCore.Functions;

using Grasshopper.Kernel.Types;

using OasysGH;
using OasysGH.Parameters;

namespace AdSecGH.Parameters {
  /// <summary>
  ///   Goo wrapper class, makes sure this can be used in Grasshopper.
  /// </summary>
  public class AdSecMaterialGoo : GH_OasysGoo<MaterialDesign> {
    public static string Description => "AdSec Material Parameter";
    public static string Name => "Material";
    public static string NickName => "Mat";
    public AdSecMaterial Material { get; private set; }
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;

    public AdSecMaterialGoo(MaterialDesign materialDesign) : base(materialDesign) {
      Material = new AdSecMaterial() {
        Material = Value?.Material,
        DesignCode = new AdSecDesignCode() {
          DesignCode = Value?.DesignCode?.IDesignCode,
        },
        GradeName = Value?.GradeName,
      };
    }

    public override IGH_Goo Duplicate() {
      return IsValid ? new AdSecMaterialGoo(Value) : null;
    }

    public override string ToString() {
      return Material?.Material == null ? "Empty Material" :
        new AdSecMaterial(Material.Material, Material.GradeName).ToString();
    }
  }
}
