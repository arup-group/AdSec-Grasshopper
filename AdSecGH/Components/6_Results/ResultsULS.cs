using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;

namespace AdSecGH.Components {
  public class UlsResultGh : UlsResultFunction {
    public UlsResultGh() {
    }
  }

  public class UlsResult : ComponentAdapter<UlsResultGh> {
    protected override void BeforeSolveInstance() {
      BusinessComponent.DeformationDescription(DefaultUnits.StrainUnitResult, DefaultUnits.CurvatureUnit);
      RefreshOutputParameter(BusinessComponent.GetAllOutputAttributes());
    }

    public UlsResult() {
      Hidden = true;
      Category = CategoryName.Name();
      SubCategory = SubCategoryName.Cat7();
    }
    public override Guid ComponentGuid => new Guid("146bd264-66ac-4484-856f-8557be762a33");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.ULS;
  }
}
