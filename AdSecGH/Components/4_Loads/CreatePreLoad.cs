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
    private ForceUnit _forceUnit = DefaultUnits.ForceUnit;
    private StrainUnit _strainUnit = DefaultUnits.MaterialStrainUnit;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;
    private const string _forceString = "Force";
    private const string _strainString = "Strain";
    private const string _stressString = "Stress";

    public override Guid ComponentGuid => new Guid("cbab2b12-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Prestress;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      switch (i) {
        case 0:
          switch (_selectedItems[0]) {
            case _forceString:
              _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force);
              _selectedItems[1] = _forceUnit.ToString();
              break;

            case _strainString:
              _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain);
              _selectedItems[1] = _strainUnit.ToString();
              break;

            case _stressString:
              _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress);
              _selectedItems[1] = _stressUnit.ToString();
              break;
          }
          break;
        case 1:
          switch (_selectedItems[0]) {
            case _forceString:
              _forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), _selectedItems[i]);
              break;

            case _strainString:
              _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[i]);
              break;

            case _stressString:
              _stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[i]);
              break;
          }
          break;
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

      _dropDownItems.Add(new List<string>() { _forceString, _strainString, _stressString });
      _selectedItems.Add(_dropDownItems[0][0]);

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force));
      _selectedItems.Add(Force.GetAbbreviation(_forceUnit));

      _isInitialised = true;

    }

    public override void VariableParameterMaintenance() {
      UpdateUnits();
    }


    protected override void BeforeSolveInstance() {
      UpdateUnits();
    }

    private void UpdateUnits() {
      UpdateDefaultUnits();
      BusinessComponent.PreLoadType = _selectedItems[0];
      BusinessComponent.ForceUnit = _forceUnit;
      BusinessComponent.MaterialStrainUnit = _strainUnit;
      BusinessComponent.StressUnitResult = _stressUnit;
      RefreshParameter();
    }
  }
}
