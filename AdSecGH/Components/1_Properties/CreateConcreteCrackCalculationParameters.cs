using System;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec.Materials;
using OasysGH;
using OasysGH.Components;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
  /// <summary>
  /// Component to create new Concrete Crack Calculation Parameters
  /// </summary>
  public class CreateConcreteCrackCalculationParameters : GH_OasysDropDownComponent, IGH_VariableParameterComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("bc810b4b-11f1-496f-b949-a0be77e9bdc8");
    public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCrackCalcParams;

    public CreateConcreteCrackCalculationParameters() : base("Create CrackCalcParams",
      "CrackCalcParams",
      "Create Concrete Crack Calculation Parameters for AdSec Material",
      Ribbon.CategoryName.Name(),
      Ribbon.SubCategoryName.Cat1())
    { this.Hidden = true; } // sets the initial state of the component to hidden
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string unitEAbbreviation = Pressure.GetAbbreviation(this.StressUnitE);
      string unitSAbbreviation = Pressure.GetAbbreviation(this.StrengthUnit);
      pManager.AddGenericParameter("Elastic Modulus [" + unitEAbbreviation + "]", "E", "Value for Elastic Modulus", GH_ParamAccess.item);
      pManager.AddGenericParameter("Compression [" + unitSAbbreviation + "]", "fc", "Value for Characteristic Compressive Strength", GH_ParamAccess.item);
      pManager.AddGenericParameter("Tension [" + unitSAbbreviation + "]", "ft", "Value for Characteristic Tension Strength", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("CrackCalcParams", "CCP", "AdSec ConcreteCrackCalculationParameters", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      Pressure modulus = GetInput.GetStress(this, DA, 0, StressUnitE);
      if (modulus.Value < 0)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Elastic Modulus value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
        modulus = new Pressure(Math.Abs(modulus.Value), modulus.Unit);
      }
      Pressure compression = GetInput.GetStress(this, DA, 1, StrengthUnit);
      if (compression.Value > 0)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Compression value must be negative. Input value has been inverted. This service has been provided free of charge, enjoy!");
        compression = new Pressure(compression.Value * -1, compression.Unit);
      }
      Pressure tension = GetInput.GetStress(this, DA, 2, StrengthUnit);
      if (tension.Value < 0)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Tension value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
        tension = new Pressure(Math.Abs(tension.Value), tension.Unit);
      }

      // create new ccp
      IConcreteCrackCalculationParameters ccp = IConcreteCrackCalculationParameters.Create(modulus, compression, tension);
      AdSecConcreteCrackCalculationParametersGoo ccpGoo = new AdSecConcreteCrackCalculationParametersGoo(ccp);

      DA.SetData(0, ccpGoo);
    }

    #region Custom UI
    private PressureUnit StressUnitE = Units.StressUnit;
    private PressureUnit StrengthUnit = Units.StressUnit;

    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Elasticity Unit",
        "Strength Unit"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // pressure E
      this.DropDownItems.Add(Units.FilteredStressUnits);
      this.SelectedItems.Add(StrengthUnit.ToString());

      // pressure stress
      this.DropDownItems.Add(Units.FilteredStressUnits);
      this.SelectedItems.Add(StrengthUnit.ToString());

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // change selected item
      this.SelectedItems[i] = this.DropDownItems[i][j];

      switch (i)
      {
        case 0:
          StressUnitE = (PressureUnit)Enum.Parse(typeof(PressureUnit), this.SelectedItems[i]);
          break;
        case 1:
          StrengthUnit = (PressureUnit)Enum.Parse(typeof(PressureUnit), this.SelectedItems[i]);
          break;
      }

      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      this.StressUnitE = (PressureUnit)Enum.Parse(typeof(PressureUnit), this.SelectedItems[0]);
      this.StrengthUnit = (PressureUnit)Enum.Parse(typeof(PressureUnit), this.SelectedItems[1]);

      base.UpdateUIFromSelectedItems();
    }
    #endregion

    #region IGH_VariableParameterComponent null implementation
    void IGH_VariableParameterComponent.VariableParameterMaintenance()
    {
      string unitEAbbreviation = Pressure.GetAbbreviation(this.StressUnitE);
      string unitSAbbreviation = Pressure.GetAbbreviation(this.StrengthUnit);
      Params.Input[0].Name = "Elastic Modulus [" + unitEAbbreviation + "]";
      Params.Input[1].Name = "Compression [" + unitSAbbreviation + "]";
      Params.Input[2].Name = "Tension [" + unitSAbbreviation + "]";
    }
    #endregion
  }
}