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
    private AngleUnit _angleUnit = AngleUnit.Radian;

    public override Guid ComponentGuid => new Guid("78f26bee-c72c-4d88-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.EditProfile;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      _angleUnit = (AngleUnit)UnitsHelper.Parse(typeof(AngleUnit), _selectedItems[i]);
      UpdateLocalUnitsAndRefreshParams();
      base.UpdateUI();
    }

    private void UpdateLocalUnitsAndRefreshParams() {
      UpdateUnits();
      BusinessComponent.AngleUnit = _angleUnit;
      RefreshParameter(this);
    }
  }
}
