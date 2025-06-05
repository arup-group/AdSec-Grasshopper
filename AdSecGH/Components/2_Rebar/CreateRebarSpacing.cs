using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using GH_IO.Serialization;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units.Helpers;

using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateRebarSpacing : DropdownAdapter<CreateRebarSpacingFunction> {
    public override Guid ComponentGuid => new Guid("846d546a-4284-4d69-906b-0e6985d7ddd3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarSpacing;

    public override void SetSelected(int i, int j) {
      string selectedItem = _dropDownItems[i][j];
      _selectedItems[i] = selectedItem;
      if (i == 0) {
        BusinessComponent.SetMode((SpacingMode)Enum.Parse(typeof(SpacingMode), _selectedItems[i]));
        _selectedItems[i] = selectedItem;
      } else {
        UpdateUnits();
      }

      base.UpdateUI();
    }

    protected override void UpdateUIFromSelectedItems() {
      base.UpdateUIFromSelectedItems();
      UpdateUnits();
    }

    protected override void BeforeSolveInstance() { UpdateUnits(); }

    private void UpdateUnits() {
      UpdateDefaultUnits();
      if (_dropDownItems.Count > 1) {
        BusinessComponent.LocalLengthUnitGeometry
          = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
        BusinessComponent.UpdateUnits();
      }

      RefreshParameter();
    }
  }
}
