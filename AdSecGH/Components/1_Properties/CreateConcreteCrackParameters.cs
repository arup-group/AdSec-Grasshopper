﻿using System;
using System.Collections.Generic;
using AdSecGH.Helpers;
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

namespace AdSecGH.Components
{
  public class CreateConcreteCrackParameters : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("bc810b4b-11f1-496f-b949-a0be77e9bdc8");
    public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCrackCalcParams;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;
    private PressureUnit _strengthUnit = DefaultUnits.MaterialStrengthUnit;

    public CreateConcreteCrackParameters() : base(
      "Create CrackCalcParams",
      "CrackCalcParams",
      "Create Concrete Crack Calculation Parameters for AdSec Material",
      CategoryName.Name(),
      SubCategoryName.Cat1())
    {
      this.Hidden = true; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string unitEAbbreviation = Pressure.GetAbbreviation(this._stressUnit);
      string unitSAbbreviation = Pressure.GetAbbreviation(this._strengthUnit);
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
      Pressure modulus = (Pressure)Input.UnitNumber(this, DA, 0, this._stressUnit);
      if (modulus.Value < 0)
      {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Elastic Modulus value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
        modulus = new Pressure(Math.Abs(modulus.Value), modulus.Unit);
      }
      Pressure compression = (Pressure)Input.UnitNumber(this, DA, 1, this._strengthUnit);
      if (compression.Value > 0)
      {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Compression value must be negative. Input value has been inverted. This service has been provided free of charge, enjoy!");
        compression = new Pressure(compression.Value * -1, compression.Unit);
      }
      Pressure tension = (Pressure)Input.UnitNumber(this, DA, 2, this._strengthUnit);
      if (tension.Value < 0)
      {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Tension value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
        tension = new Pressure(Math.Abs(tension.Value), tension.Unit);
      }

      // create new ccp
      IConcreteCrackCalculationParameters ccp = IConcreteCrackCalculationParameters.Create(modulus, compression, tension);
      AdSecConcreteCrackCalculationParametersGoo ccpGoo = new AdSecConcreteCrackCalculationParametersGoo(ccp);

      DA.SetData(0, ccpGoo);
    }

    #region Custom UI
    protected override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Elasticity Unit",
        "Strength Unit"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // pressure E
      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
      this.SelectedItems.Add(Pressure.GetAbbreviation(this._strengthUnit));

      // pressure stress
      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
      this.SelectedItems.Add(Pressure.GetAbbreviation(this._strengthUnit));

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      this.SelectedItems[i] = this.DropDownItems[i][j];

      switch (i)
      {
        case 0:
          this._stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[i]);
          break;
        case 1:
          this._strengthUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[i]);
          break;
      }

      base.UpdateUI();
    }

    protected override void UpdateUIFromSelectedItems()
    {
      this._stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[0]);
      this._strengthUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      string unitEAbbreviation = Pressure.GetAbbreviation(this._stressUnit);
      string unitSAbbreviation = Pressure.GetAbbreviation(this._strengthUnit);
      this.Params.Input[0].Name = "Elastic Modulus [" + unitEAbbreviation + "]";
      this.Params.Input[1].Name = "Compression [" + unitSAbbreviation + "]";
      this.Params.Input[2].Name = "Tension [" + unitSAbbreviation + "]";
    }
  }
}