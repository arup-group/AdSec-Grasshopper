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
    private const string _selectedForceUnit = "Force";
    private const string _selectedStrainUnit = "Strain";
    private const string _selectedStressUnit = "Stress";

    public CreatePreLoad() : base() { Hidden = true; }
    public override Guid ComponentGuid => new Guid("cbab2b12-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Prestress;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      switch (i) {
        case 0:
          switch (_selectedItems[0]) {
            case _selectedForceUnit:
              _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force);
              _selectedItems[1] = _forceUnit.ToString();
              break;

            case _selectedStrainUnit:
              _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain);
              _selectedItems[1] = _strainUnit.ToString();
              break;

            case _selectedStressUnit:
              _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress);
              _selectedItems[1] = _stressUnit.ToString();
              break;
          }
          break;
        case 1:
          switch (_selectedItems[0]) {
            case _selectedForceUnit:
              _forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), _selectedItems[i]);
              break;

            case _selectedStrainUnit:
              _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[i]);
              break;

            case _selectedStressUnit:
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
        _selectedForceUnit,
        _selectedStrainUnit,
        _selectedStressUnit,
      };

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(_spacerDescriptions);
      _selectedItems.Add(_dropDownItems[0][0]);

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force));
      _selectedItems.Add(Force.GetAbbreviation(_forceUnit));

      _isInitialised = true;

    }

    public override void VariableParameterMaintenance() {
      string forceUnitAbbreviation = Force.GetAbbreviation(_forceUnit);
      string strainUnitAbbreviation = Strain.GetAbbreviation(_strainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(_stressUnit);

      switch (_selectedItems[0]) {
        case _selectedForceUnit:
          BusinessComponent.PreloadInput.Name = $"{_selectedForceUnit} [{forceUnitAbbreviation}]";
          BusinessComponent.PreloadInput.NickName = "P";
          break;

        case _selectedStrainUnit:
          BusinessComponent.PreloadInput.Name = $"{_selectedStrainUnit} [{strainUnitAbbreviation}]";
          BusinessComponent.PreloadInput.NickName = "ε";
          break;

        case _selectedStressUnit:
          BusinessComponent.PreloadInput.Name = $"{_selectedStressUnit} [{stressUnitAbbreviation}]";
          BusinessComponent.PreloadInput.NickName = "σ";
          break;
      }
    }


    protected override void BeforeSolveInstance() {
      UpdateUnits();
    }

    private void UpdateUnits() {
      UpdateDefaultUnits();
      BusinessComponent.ForceUnit = _forceUnit;
      BusinessComponent.MaterialStrainUnit = _strainUnit;
      BusinessComponent.StressUnitResult = _stressUnit;
      RefreshParameter();
    }
  }
}
