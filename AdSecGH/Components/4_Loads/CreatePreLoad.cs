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
        ProcessDropdownItems(out var spacerDescriptions, out var dropDownItems, out var selectedItems);
        _dropDownItems[1] = dropDownItems[1];
        _selectedItems[1] = dropDownItems[1][0];
      }
      base.UpdateUI();
    }

    protected override void BeforeSolveInstance() {
      UpdateUnits();
    }

    private void UpdateLocalUnits() {
      var unitString = _selectedItems[1];
      switch (BusinessComponent.PreLoadType) {
        case PreLoadType.Force:
          BusinessComponent.ForceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), unitString);
          break;

        case PreLoadType.Strain:
          BusinessComponent.MaterialStrainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), unitString);
          break;

        case PreLoadType.Stress:
          BusinessComponent.StressUnitResult = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), unitString);
          break;
      }
    }

    private void UpdateUnits() {
      UpdateDefaultUnits();
      UpdateLocalUnits();
      RefreshParameter();
    }
  }
}
