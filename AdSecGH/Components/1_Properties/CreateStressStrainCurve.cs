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
using Grasshopper.Kernel.Parameters;

using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units.Helpers;

using OasysUnits.Units;

using static AdSecGH.Parameters.AdSecStressStrainCurveGoo;

namespace AdSecGH.Components {
  public class CreateStressStrainCurveGh : StressStrainCurveFunction {
    public CreateStressStrainCurveGh() { }
  }

  public class CreateStressStrainCurve : DropdownAdapter<CreateStressStrainCurveGh> {
    public override Guid ComponentGuid => new Guid("b2ddf545-2a4c-45ac-ba1c-cb0f3da5b37f"); // Update with your GUID
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StressStrainCrv;

    protected override void UpdateUIFromSelectedItems() {
      base.UpdateUIFromSelectedItems();
      SetSelectedCurveType();
      UpdateUnits();
    }

    public override void SetSelected(int i, int j) {
      var selectedItem = _dropDownItems[i][j];
      SetSelectedCurveType();
      _selectedItems[i] = selectedItem;
      UpdateUnits();
      base.UpdateUI();
    }

    private void SetSelectedCurveType() {
      BusinessComponent.SelectedCurveType = (StressStrainCurveFunction.CurveType)Enum.Parse(typeof(StressStrainCurveType), _selectedItems[0], true);
    }

    private void UpdateUnits() {
      if (_selectedItems.Count > 1) {
        BusinessComponent.LocalStrainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[1]);
      }
      if (_selectedItems.Count > 2) {
        BusinessComponent.LocalStressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[2]);
      }
    }
  }
}
