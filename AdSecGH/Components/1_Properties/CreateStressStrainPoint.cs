using System;
using System.Collections.Generic;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateStressStrainPoint : GH_OasysDropDownComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("69a789d4-c11b-4396-b237-a10efdd6d0c4");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.StressStrainPt;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;

    public CreateStressStrainPoint() : base(
      "Create StressStrainPt",
      "StressStrainPt",
      "Create a Stress Strain Point for AdSec Stress Strain Curve",
      CategoryName.Name(),
      SubCategoryName.Cat1()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      switch (i) {
        case 0:
          _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[i]);
          break;

        case 1:
          _stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[i]);
          break;
      }
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string strainUnitAbbreviation = Strain.GetAbbreviation(_strainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(_stressUnit);
      Params.Input[0].Name = "Strain [" + strainUnitAbbreviation + "]";
      Params.Input[1].Name = "Stress [" + stressUnitAbbreviation + "]";
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new string[] {
        "Strain Unit",
        "Stress Unit"
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
      _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
      _selectedItems.Add(Pressure.GetAbbreviation(_stressUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string strainUnitAbbreviation = Strain.GetAbbreviation(_strainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(_stressUnit);
      pManager.AddGenericParameter("Strain [" + strainUnitAbbreviation + "]", "ε", "Value for strain (X-axis)", GH_ParamAccess.item);
      pManager.AddGenericParameter("Stress [" + stressUnitAbbreviation + "]", "σ", "Value for stress (Y-axis)", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("StressStrainPt", "SPt", "AdSec Stress Strain Point", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // create new point
      var pt = new AdSecStressStrainPointGoo(
        (Pressure)Input.UnitNumber(this, DA, 1, _stressUnit),
        (Strain)Input.UnitNumber(this, DA, 0, _strainUnit));

      DA.SetData(0, pt);
    }

    protected override void UpdateUIFromSelectedItems() {
      _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[0]);
      _stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
