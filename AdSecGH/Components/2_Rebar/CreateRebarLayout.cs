using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units.Helpers;

using OasysUnits.Units;

namespace AdSecGH.Components {

  public class CreateRebarLayout : DropdownAdapter<RebarLayoutFunction> {

    public override Guid ComponentGuid => new Guid("1250f456-de99-4834-8d7f-4019cc0c70ba");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarLayout;

    protected override void UpdateUIFromSelectedItems() {
      base.UpdateUIFromSelectedItems();
      BusinessComponent.RebarLayoutOption = (RebarLayoutOption)Enum.Parse(typeof(RebarLayoutOption), _selectedItems[0], true);
      UpdateUnits();
    }

    public override void SetSelected(int i, int j) {
      var selectedItem = _dropDownItems[i][j];
      _selectedItems[i] = selectedItem;
      BusinessComponent.RebarLayoutOption = (RebarLayoutOption)Enum.Parse(typeof(RebarLayoutOption), _selectedItems[0], true);
      if (i == 0) {
        ProcessDropdownItems();
        //update with last selection
        _selectedItems[i] = selectedItem;
      }
      UpdateUnits();
      base.UpdateUI();
    }

    private void UpdateUnits() {
      BusinessComponent.RebarLayoutOption = (RebarLayoutOption)Enum.Parse(typeof(RebarLayoutOption), _selectedItems[0]);
      if (_selectedItems.Count > 1) {
        BusinessComponent.LocalLengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      }
      if (_selectedItems.Count > 2) {
        BusinessComponent.LocalAngleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[2]);
      }
    }
  }
}
