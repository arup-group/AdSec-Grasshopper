using System;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;

namespace AdSecGH.Components {
  public class SlsResultGh : SlsResultFunction {
    public SlsResultGh() {

    }
  }

  public class SlsResult : ComponentAdapter<SlsResultGh> {
    protected override void BeforeSolveInstance() {
      BusinessComponent.RefreshDeformation(DefaultUnits.StrainUnitResult, DefaultUnits.CurvatureUnit);
      BusinessComponent.RefreshSecantStiffness(DefaultUnits.AxialStiffnessUnit, DefaultUnits.BendingStiffnessUnit);
      BusinessComponent.RefreshUncrackedMomentRanges(DefaultUnits.MomentUnit);
      RefreshParameters();
    }

    public SlsResult() { Hidden = true; Category = CategoryName.Name(); SubCategory = SubCategoryName.Cat7(); }
    public override Guid ComponentGuid => new Guid("27ba3ec5-b94c-43ad-8623-087540413628");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.SLS;
  }

}
