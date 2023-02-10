using System;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec.Materials;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Parameters;
using OasysGH.Units;
using OasysUnits;

namespace AdSecGH.Components
{
  public class EditConcreteCrackCalculationParameters : GH_OasysComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("bc900b4b-11f1-496f-b949-a0be77e9bdc8");
    public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.EditCrackCalcParams;

    public EditConcreteCrackCalculationParameters() : base(
      "Edit CrackCalcParams",
      "EditCalcParams",
      "Edit Concrete Crack Calculation Parameters for AdSec Material",
      CategoryName.Name(),
      SubCategoryName.Cat1())
    {
      this.Hidden = true; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string unitAbbreviation = Pressure.GetAbbreviation(DefaultUnits.StressUnitResult);
      pManager.AddGenericParameter("CrackCalcParams", "CCP", "AdSec ConcreteCrackCalculationParameters", GH_ParamAccess.item);
      pManager.AddGenericParameter("Elastic Modulus [" + unitAbbreviation + "]", "E", "[Optional] Overwrite Value for Elastic Modulus", GH_ParamAccess.item);
      pManager.AddGenericParameter("Compression [" + unitAbbreviation + "]", "fc", "[Optional] Overwrite Value for Characteristic Compressive Strength", GH_ParamAccess.item);
      pManager.AddGenericParameter("Tension [" + unitAbbreviation + "]", "ft", "[Optional] Overwrite Value for Characteristic Tension Strength", GH_ParamAccess.item);
      // make all but first input optional
      for (int i = 1; i < pManager.ParamCount; i++)
        pManager[i].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("CrackCalcParams", "CCP", "Modified AdSec ConcreteCrackCalculationParameters", GH_ParamAccess.item);
      pManager.AddGenericParameter("Elastic Modulus", "E", "Value of Elastic Modulus", GH_ParamAccess.item);
      pManager.AddGenericParameter("Compression", "fc", "Value of Characteristic Compressive Strength", GH_ParamAccess.item);
      pManager.AddGenericParameter("Tension", "ft", "Value of Characteristic Tension Strength", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // 0 Cracked params
      AdSecConcreteCrackCalculationParametersGoo concreteCrack = new AdSecConcreteCrackCalculationParametersGoo(AdSecInput.ConcreteCrackCalculationParameters(this, DA, 0));

      if (concreteCrack != null && concreteCrack.Value != null)
      {
        // #### get the remaining inputs ####
        Pressure e = concreteCrack.Value.ElasticModulus;
        Pressure fck = concreteCrack.Value.CharacteristicCompressiveStrength;
        Pressure ft = concreteCrack.Value.CharacteristicTensileStrength;
        bool reCreate = false;

        // 1 Elastic modulus
        if (Params.Input[1].SourceCount > 0)
        {
          e = (Pressure)Input.UnitNumber(this, DA, 1, DefaultUnits.StressUnitResult);
          if (e.Value < 0)
          {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Elastic Modulus value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
            e = new Pressure(Math.Abs(e.Value), e.Unit);
          }
          reCreate = true;
        }

        // 2 Compression
        if (Params.Input[2].SourceCount > 0)
        {
          fck = (Pressure)Input.UnitNumber(this, DA, 2, DefaultUnits.StressUnitResult);
          if (fck.Value > 0)
          {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Compression value must be negative. Input value has been inverted. This service has been provided free of charge, enjoy!");
            fck = new Pressure(fck.Value * -1, fck.Unit);
          }
          reCreate = true;
        }

        // 3 Tension
        if (Params.Input[3].SourceCount > 0)
        {
          ft = (Pressure)Input.UnitNumber(this, DA, 3, DefaultUnits.StressUnitResult);
          if (ft.Value < 0)
          {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Tension value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
            ft = new Pressure(Math.Abs(ft.Value), ft.Unit);
          }
          reCreate = true;
        }

        if (reCreate)
        {
          IConcreteCrackCalculationParameters ccp = IConcreteCrackCalculationParameters.Create(e, fck, ft);
          concreteCrack = new AdSecConcreteCrackCalculationParametersGoo(ccp);
        }

        // #### set outputs ####
        DA.SetData(0, concreteCrack);
        DA.SetData(1, new GH_UnitNumber(new Pressure(e.As(DefaultUnits.StressUnitResult), DefaultUnits.StressUnitResult)));
        DA.SetData(2, new GH_UnitNumber(new Pressure(fck.As(DefaultUnits.StressUnitResult), DefaultUnits.StressUnitResult)));
        DA.SetData(3, new GH_UnitNumber(new Pressure(ft.As(DefaultUnits.StressUnitResult), DefaultUnits.StressUnitResult)));

      }
      else
      {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ConcreteCrackCalculationParameters are null");
        return;
      }
    }
  }
}
