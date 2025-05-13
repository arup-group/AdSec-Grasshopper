using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

using CreateRebarFunction = AdSecCore.Functions.CreateRebarFunction;

namespace AdSecGH.Components {
  public class CreateRebar : DropdownAdapter<CreateRebarFunction> {

    public override Guid ComponentGuid => new Guid("024d241a-b6cc-4134-9f5c-ac9a6dcb2c4b");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Rebar;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        BusinessComponent.SetMode(UpdateMode());
      } else {
        BusinessComponent.LengthUnitGeometry = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }
    }

    private RebarMode UpdateMode() {
      return (RebarMode)Enum.Parse(typeof(RebarMode), _selectedItems[0]);
    }

    protected override void UpdateUIFromSelectedItems() {
      BusinessComponent.SetMode((UpdateMode()));
      BusinessComponent.LengthUnitGeometry = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }

    protected override void BeforeSolveInstance() {
      UpdateDefaultUnits();
      BusinessComponent.LengthUnitGeometry = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      RefreshParameter();
    }
  }
}
