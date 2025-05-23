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

  public class CreatePreLoadGh : CreatePreLoadFunction {
    public CreatePreLoadGh() {
    }
  }

  public class CreatePreLoad : DropdownAdapter<CreatePreLoadGh> {

    public override Guid ComponentGuid => new Guid("cbab2b12-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Prestress;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      BusinessComponent.PreLoadType = (PreLoadType)Enum.Parse(typeof(PreLoadType), _selectedItems[0], true);
      if (i == 0) {
        ProcessDropdownItems();
      }
      base.UpdateUI();
    }

    internal override void SetLocalUnits() {
      var unitString = _selectedItems[1];
      switch (BusinessComponent.PreLoadType) {
        case PreLoadType.Force:
          BusinessComponent.LocalUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), unitString);
          break;

        case PreLoadType.Strain:
          BusinessComponent.LocalUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), unitString);
          break;

        case PreLoadType.Stress:
          BusinessComponent.LocalUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), unitString);
          break;
      }
    }
  }
}
