using System;
using System.Collections.Generic;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec.Materials;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateConcreteCrackParameters : GH_OasysDropDownComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("bc810b4b-11f1-496f-b949-a0be77e9bdc8");
    public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCrackCalcParams;
    private PressureUnit _strengthUnit = DefaultUnits.MaterialStrengthUnit;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;

    public CreateConcreteCrackParameters() : base("Create CrackCalcParams", "CrackCalcParams",
      "Create Concrete Crack Calculation Parameters for AdSec Material", CategoryName.Name(),
      SubCategoryName.Cat1()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      switch (i) {
        case 0:
          _stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[i]);
          break;

        case 1:
          _strengthUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[i]);
          break;
      }

      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string unitEAbbreviation = Pressure.GetAbbreviation(_stressUnit);
      string unitSAbbreviation = Pressure.GetAbbreviation(_strengthUnit);
      Params.Input[0].Name = "Elastic Modulus [" + unitEAbbreviation + "]";
      Params.Input[1].Name = "Compression [" + unitSAbbreviation + "]";
      Params.Input[2].Name = "Tension [" + unitSAbbreviation + "]";
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new string[] {
        "Elasticity Unit",
        "Strength Unit"
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      // pressure E
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
      _selectedItems.Add(Pressure.GetAbbreviation(_strengthUnit));

      // pressure stress
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
      _selectedItems.Add(Pressure.GetAbbreviation(_strengthUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string unitEAbbreviation = Pressure.GetAbbreviation(_stressUnit);
      string unitSAbbreviation = Pressure.GetAbbreviation(_strengthUnit);
      pManager.AddGenericParameter("Elastic Modulus [" + unitEAbbreviation + "]", "E", "Value for Elastic Modulus", GH_ParamAccess.item);
      pManager.AddGenericParameter("Compression [" + unitSAbbreviation + "]", "fc", "Value for Characteristic Compressive Strength", GH_ParamAccess.item);
      pManager.AddGenericParameter("Tension [" + unitSAbbreviation + "]", "ft", "Value for Characteristic Tension Strength", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("CrackCalcParams", "CCP", "AdSec ConcreteCrackCalculationParameters", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      var modulus = (Pressure)Input.UnitNumber(this, DA, 0, _stressUnit);
      if (modulus.Value < 0) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Elastic Modulus value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
        modulus = new Pressure(Math.Abs(modulus.Value), modulus.Unit);
      }
      var compression = (Pressure)Input.UnitNumber(this, DA, 1, _strengthUnit);
      if (compression.Value > 0) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Compression value must be negative. Input value has been inverted. This service has been provided free of charge, enjoy!");
        compression = new Pressure(compression.Value * -1, compression.Unit);
      }
      var tension = (Pressure)Input.UnitNumber(this, DA, 2, _strengthUnit);
      if (tension.Value < 0) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Tension value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
        tension = new Pressure(Math.Abs(tension.Value), tension.Unit);
      }

      // create new ccp
      var ccp = IConcreteCrackCalculationParameters.Create(modulus, compression, tension);
      var ccpGoo = new AdSecConcreteCrackCalculationParametersGoo(ccp);

      DA.SetData(0, ccpGoo);
    }

    protected override void UpdateUIFromSelectedItems() {
      _stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[0]);
      _strengthUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), _selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
