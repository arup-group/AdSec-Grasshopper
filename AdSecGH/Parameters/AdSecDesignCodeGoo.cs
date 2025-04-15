using AdSecCore.Functions;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.DesignCode;

using OasysGH;
using OasysGH.Parameters;

namespace AdSecGH.Parameters {
  /// <summary>
  /// Goo wrapper class, makes sure <see cref="AdSecDesignCode"/> can be used in Grasshopper.
  /// </summary>
  public class AdSecDesignCodeGoo : GH_OasysGoo<AdSecDesignCode> {
    public static string Description => "AdSec Design Code";
    public static string Name => "DesignCode";
    public static string NickName => "DC";
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;

    public AdSecDesignCodeGoo(AdSecDesignCode item) : base(item) {
    }

    public AdSecDesignCodeGoo(IDesignCode designCode) : base(new AdSecDesignCode(designCode)) {

    }

    public override IGH_Goo Duplicate() {
      return new AdSecDesignCodeGoo(Value);
    }
  }
}
