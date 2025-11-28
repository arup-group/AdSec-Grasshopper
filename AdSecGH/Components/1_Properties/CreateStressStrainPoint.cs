using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateStressStrainPointGh : StressStrainPointFunction {
    public CreateStressStrainPointGh() { }
  }

  public class CreateStressStrainPoint : DropdownAdapter<CreateStressStrainPointGh> {
    public override Guid ComponentGuid => new Guid("69a789d4-c11b-4396-b237-a10efdd6d0c4");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateStressStrainPoint;

    protected override void UpdateUIFromSelectedItems() {
      base.UpdateUIFromSelectedItems();
      //update unit
      UpdateUnits();
    }

    public override void SetSelected(int i, int j) {
      var selectedItem = _dropDownItems[i][j];
      _selectedItems[i] = selectedItem;
      UpdateUnits();
      base.UpdateUI();
    }

    private void UpdateUnits() {
      if (_selectedItems.Count > 0) {
        BusinessComponent.LocalStrainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[0]);
      }
      if (_selectedItems.Count > 1) {
        BusinessComponent.LocalStressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[1]);
      }
    }
  }
}
