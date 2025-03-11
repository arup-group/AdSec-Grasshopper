using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Components;
using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

namespace AdSecGH.Components {
  public class ResultsSlsGh : SlsResultFunction {
    public ResultsSlsGh() {
      RefreshDeformation(DefaultUnits.StrainUnitResult, DefaultUnits.CurvatureUnit);
      RefreshSecantStiffness(DefaultUnits.AxialStiffnessUnit, DefaultUnits.BendingStiffnessUnit);
    }
  }

  public class ResultsSls : ComponentAdapter<ResultsSlsGh> {
    public ResultsSls() { Hidden = true; Category = CategoryName.Name(); SubCategory = SubCategoryName.Cat7(); }
    public override Guid ComponentGuid => new Guid("27ba3ec5-b94c-43ad-8623-087540413628");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.SLS;
  }

}
