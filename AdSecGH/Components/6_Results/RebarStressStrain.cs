using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;

namespace AdSecGH.Components {
  public class RebarStressStrainGh : RebarStressStrainFunction {
    public RebarStressStrainGh() {
    }
  }

  public class RebarStressStrain : ComponentAdapter<RebarStressStrainGh> {
    protected override void BeforeSolveInstance() {
      UpdateUnit();
      BusinessComponent.UpdateOutputParameter();
      RefreshOutputParameter(BusinessComponent.GetAllOutputAttributes());
    }

    public RebarStressStrain() {
      Hidden = true;
      Category = CategoryName.Name();
      SubCategory = SubCategoryName.Cat7();
    }

    public override Guid ComponentGuid => new Guid("bb9fe65b-76e1-466b-be50-3dd9c7a3283f");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StressStrainRebar;
  }
}
