using System;
using System.DirectoryServices.AccountManagement;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units.Helpers;

using OasysUnits.Units;

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
      //update unit first
      UpdateUnits();
      //then update curve type
      SetSelectedCurveType();
    }

    public override void SetSelected(int i, int j) {
      var selectedItem = _dropDownItems[i][j];
      _selectedItems[i] = selectedItem;
      SetSelectedCurveType();
      UpdateUnits();
      base.UpdateUI();
    }

    private void SetSelectedCurveType() {
      BusinessComponent.SelectedCurveType = (StressStrainCurveType)Enum.Parse(typeof(StressStrainCurveType), _selectedItems[0], true);
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
