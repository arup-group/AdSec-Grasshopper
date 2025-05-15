using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
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
      if (i == 0) {
        switch (_selectedItems[0]) {
          case CreatePreLoadFunction.ForceString:
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force);
            _selectedItems[1] = _dropDownItems[1][0];
            break;

          case CreatePreLoadFunction.StrainString:
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain);
            _selectedItems[1] = _dropDownItems[1][0];
            break;

          case CreatePreLoadFunction.StressString:
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress);
            _selectedItems[1] = _dropDownItems[1][0];
            break;
        }
      }
      UpdateUnits();
      base.UpdateUI();
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string> {
        "Force",
         "Measure"
      };

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(new List<string>() { CreatePreLoadFunction.ForceString, CreatePreLoadFunction.StrainString, CreatePreLoadFunction.StressString });

      _selectedItems.Add(_dropDownItems[0][0]);

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force));
      _selectedItems.Add(Force.GetAbbreviation(DefaultUnits.ForceUnit));

      _isInitialised = true;

    }

    public override void VariableParameterMaintenance() {
      UpdateUnits();
    }


    protected override void BeforeSolveInstance() {
      UpdateUnits();
    }

    private void UpdateLocalUnits() {
      var loadType = _selectedItems[0];
      var unitString = _selectedItems[1];
      BusinessComponent.PreLoadType = loadType;
      switch (loadType) {
        case CreatePreLoadFunction.ForceString:
          BusinessComponent.ForceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), unitString);
          break;

        case CreatePreLoadFunction.StrainString:
          BusinessComponent.MaterialStrainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), unitString);
          break;

        case CreatePreLoadFunction.StressString:
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
