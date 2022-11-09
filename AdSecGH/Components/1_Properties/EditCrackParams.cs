using System;
using System.Linq;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH.Parameters;
using OasysUnits;

namespace AdSecGH.Components
{
  public class EditConcreteCrackCalculationParameters : GH_OasysComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("bc900b4b-11f1-496f-b949-a0be77e9bdc8");
        public EditConcreteCrackCalculationParameters()
          : base("Edit CrackCalcParams", "EditCalcParams", "Edit Concrete Crack Calculation Parameters for AdSec Material",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.EditCrackCalcParams;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            IQuantity quantity = new Pressure(0, Units.StressUnit);
            string unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));

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
            AdSecConcreteCrackCalculationParametersGoo concreteCrack = 
                new AdSecConcreteCrackCalculationParametersGoo(
                    GetInput.ConcreteCrackCalculationParameters(this, DA, 0));

            if (concreteCrack != null && concreteCrack.ConcreteCrackCalculationParameters != null)
            {
                // #### get the remaining inputs ####
                Pressure e = concreteCrack.Value.ElasticModulus;
                Pressure fck = concreteCrack.Value.CharacteristicCompressiveStrength;
                Pressure ft = concreteCrack.Value.CharacteristicTensileStrength;
                bool reCreate = false;

                // 1 Elastic modulus
                if (Params.Input[1].SourceCount > 0)
                {
                    
                    e = GetInput.GetStress(this, DA, 1, Units.StressUnit);
                    if (e.Value < 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Elastic Modulus value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
                        e = new Pressure(Math.Abs(e.Value), e.Unit);
                    }
                    reCreate = true;
                }

                // 2 Compression
                if (Params.Input[2].SourceCount > 0)
                {
                    fck = GetInput.GetStress(this, DA, 2, Units.StressUnit);
                    if (fck.Value > 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Compression value must be negative. Input value has been inverted. This service has been provided free of charge, enjoy!");
                        fck = new Pressure(fck.Value * -1, fck.Unit);
                    }
                    reCreate = true;
                }

                // 3 Tension
                if (Params.Input[3].SourceCount > 0)
                {
                    ft = GetInput.GetStress(this, DA, 3, Units.StressUnit);
                    if (ft.Value < 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Tension value must be positive. Input value has been inverted. This service has been provided free of charge, enjoy!");
                        ft = new Pressure(Math.Abs(ft.Value), ft.Unit);
                    }
                    reCreate = true;
                }

                if (reCreate)
                    concreteCrack = new AdSecConcreteCrackCalculationParametersGoo(e, fck, ft);

                // #### set outputs ####
                DA.SetData(0, concreteCrack);
                DA.SetData(1, new GH_UnitNumber(new Pressure(e.As(Units.StressUnit), Units.StressUnit)));
                DA.SetData(2, new GH_UnitNumber(new Pressure(fck.As(Units.StressUnit), Units.StressUnit)));
                DA.SetData(3, new GH_UnitNumber(new Pressure(ft.As(Units.StressUnit), Units.StressUnit)));

            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ConcreteCrackCalculationParameters are null");
                return;
            }
        }
    }
}
