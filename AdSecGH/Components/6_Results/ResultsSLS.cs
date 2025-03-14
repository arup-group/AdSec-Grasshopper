﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;

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
      BusinessComponent.DeformationDescription(DefaultUnits.StrainUnitResult, DefaultUnits.CurvatureUnit);
      BusinessComponent.SecantStiffnessDescription(DefaultUnits.AxialStiffnessUnit, DefaultUnits.BendingStiffnessUnit);
      BusinessComponent.UncrackedMomentRangesDescription(DefaultUnits.MomentUnit);
      RefreshOutputParameter(BusinessComponent.GetAllOutputAttributes());
    }

    public SlsResult() { Hidden = true; Category = CategoryName.Name(); SubCategory = SubCategoryName.Cat7(); }
    public override Guid ComponentGuid => new Guid("27ba3ec5-b94c-43ad-8623-087540413628");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.SLS;
  }

}
