using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

namespace AdSecGH.Components {
  public class UltimateLimitStateResultGh : UltimateLimitStateResultFunction {
    public UltimateLimitStateResultGh() {
    }
  }

  public class UltimateLimitStateResult : ComponentAdapter<UltimateLimitStateResultGh> {
    public UltimateLimitStateResult() {
      Hidden = true;
      Category = CategoryName.Name();
      SubCategory = SubCategoryName.Cat7();
    }
    public override Guid ComponentGuid => new Guid("146bd264-66ac-4484-856f-8557be762a33");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.UltimateLimitStateResult;
  }
}
