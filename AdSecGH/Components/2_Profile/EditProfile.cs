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
  public class EditProfile : DropdownAdapter<EditProfileFunction> {
    public override Guid ComponentGuid => new Guid("78f26bee-c72c-4d88-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.EditProfile;

    protected override void UpdateUIFromSelectedItems() {
      base.UpdateUIFromSelectedItems();
      UpdateUnits();
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      UpdateUnits();
      base.UpdateUI();
    }

    private void UpdateUnits() {
      UpdateDefaultUnits();
      BusinessComponent.LocalAngleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[0]);
      RefreshParameter();
    }
  }
}
