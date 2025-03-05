using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

using Attribute = AdSecCore.Functions.Attribute;
namespace AdSecGH.Components {
  public class FindCrackLoadGh : FindCrackLoadFunction {
    public FindCrackLoadGh() {

    }
  }

  public class FindCrackLoad : ComponentAdapter<FindCrackLoadGh> {
    public FindCrackLoad() { Hidden = true; }
    public override Guid ComponentGuid => new Guid("f0b27be7-f367-4a2c-b90c-3ba0f66ae584");
    public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CrackLoad;

  }
}
