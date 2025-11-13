using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

namespace AdSecGH.Components {
  public class ServiceabilityLimitStateResultGh : ServiceabilityLimitStateResultFunction {
    public ServiceabilityLimitStateResultGh() {

    }
  }

  public class ServiceabilityLimitStateResult : ComponentAdapter<ServiceabilityLimitStateResultGh> {
    public ServiceabilityLimitStateResult() { Hidden = true; Category = CategoryName.Name(); SubCategory = SubCategoryName.Cat7(); }
    public override Guid ComponentGuid => new Guid("27ba3ec5-b94c-43ad-8623-087540413628");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.SLS;
  }

}
