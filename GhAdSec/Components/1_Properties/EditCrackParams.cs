using System;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.Units;
using UnitsNet;
using UnitsNet.GH;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using AdSecGH.Parameters;
using Rhino.Geometry;
using System.Collections.Generic;

namespace AdSecGH.Components
{
    public class EditConcreteCrackCalculationParameters : GH_Component
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
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.EditCrackCalcParams;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            IQuantity quantity = new UnitsNet.Pressure(0, DocumentUnits.StressUnit);
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
                UnitsNet.Pressure e = concreteCrack.Value.ElasticModulus;
                UnitsNet.Pressure fck = concreteCrack.Value.CharacteristicCompressiveStrength;
                UnitsNet.Pressure ft = concreteCrack.Value.CharacteristicTensileStrength;
                bool reCreate = false;

                // 1 Elastic modulus
                if (Params.Input[1].SourceCount > 0)
                {
                    
                    e = GetInput.Stress(this, DA, 1, DocumentUnits.StressUnit);
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
                    fck = GetInput.Stress(this, DA, 2, DocumentUnits.StressUnit);
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
                    ft = GetInput.Stress(this, DA, 3, DocumentUnits.StressUnit);
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
                DA.SetData(1, new GH_UnitNumber(new UnitsNet.Pressure(e.As(DocumentUnits.StressUnit), DocumentUnits.StressUnit)));
                DA.SetData(2, new GH_UnitNumber(new UnitsNet.Pressure(fck.As(DocumentUnits.StressUnit), DocumentUnits.StressUnit)));
                DA.SetData(3, new GH_UnitNumber(new UnitsNet.Pressure(ft.As(DocumentUnits.StressUnit), DocumentUnits.StressUnit)));

            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ConcreteCrackCalculationParameters are null");
                return;
            }
        }
    }
}
