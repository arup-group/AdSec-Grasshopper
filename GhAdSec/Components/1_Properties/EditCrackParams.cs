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
using GhAdSec.Parameters;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GhAdSec.Components
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

        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.EditCrackParams;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            IQuantity quantity = new UnitsNet.Pressure(0, GhAdSec.DocumentUnits.StressUnit);
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
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            AdSecConcreteCrackCalculationParametersGoo concreteCrack = null;
            if (DA.GetData(0, ref gh_typ))
            {
                if (gh_typ.Value is AdSecConcreteCrackCalculationParametersGoo)
                {
                    concreteCrack = (AdSecConcreteCrackCalculationParametersGoo)gh_typ.Value;
                }
                else if (gh_typ.Value is IConcreteCrackCalculationParameters)
                {
                    IConcreteCrackCalculationParameters tempconcreteCrack = (IConcreteCrackCalculationParameters)gh_typ.Value;
                    concreteCrack = new AdSecConcreteCrackCalculationParametersGoo(tempconcreteCrack);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert CCP input to a ConcreteCrackCalculationParameters");
                    return;
                }
            }

            if (concreteCrack != null && concreteCrack.ConcreteCrackCalculationParameters != null)
            {
                // #### get the remaining inputs ####
                UnitsNet.Pressure e = concreteCrack.Value.ElasticModulus;
                UnitsNet.Pressure fck = concreteCrack.Value.CharacteristicCompressiveStrength;
                UnitsNet.Pressure ft = concreteCrack.Value.CharacteristicTensileStrength;
                bool reCreate = false;

                // 1 Elastic modulus
                gh_typ = new GH_ObjectWrapper();
                if (DA.GetData(1, ref gh_typ))
                {
                    GH_UnitNumber newElastic;

                    // try cast directly to quantity type
                    if (gh_typ.Value is GH_UnitNumber)
                    {
                        newElastic = (GH_UnitNumber)gh_typ.Value;
                        if (!newElastic.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.PressureUnit)))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 1: Wrong unit type supplied"
                                + System.Environment.NewLine + "Unit type is " + newElastic.Value.QuantityInfo.Name + " but must be Stress (Pressure)");
                            return;
                        }
                        e = (UnitsNet.Pressure)newElastic.Value.ToUnit(GhAdSec.DocumentUnits.StressUnit);
                    }
                    // try cast to double
                    else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                    {
                        // create new quantity from default units
                        newElastic = new GH_UnitNumber(new UnitsNet.Pressure(val, GhAdSec.DocumentUnits.StressUnit));
                        e = (UnitsNet.Pressure)newElastic.Value;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert E input");
                        return;
                    }
                    reCreate = true;
                }

                // 2 Compression strength
                gh_typ = new GH_ObjectWrapper();
                if (DA.GetData(2, ref gh_typ))
                {
                    GH_UnitNumber newCompression;

                    // try cast directly to quantity type
                    if (gh_typ.Value is GH_UnitNumber)
                    {
                        newCompression = (GH_UnitNumber)gh_typ.Value;
                        if (!newCompression.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.PressureUnit)))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 2: Wrong unit type supplied"
                                + System.Environment.NewLine + "Unit type is " + newCompression.Value.QuantityInfo.Name + " but must be Stress (Pressure)");
                            return;
                        }
                        fck = (UnitsNet.Pressure)newCompression.Value.ToUnit(GhAdSec.DocumentUnits.StressUnit);
                        if (fck.Value > 0)
                        {
                            fck = new UnitsNet.Pressure(fck.Value * -1, fck.Unit);
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Compression (fc) must be negative; note that value has been multiplied by -1");
                        }
                    }
                    // try cast to double
                    else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                    {
                        // create new quantity from default units
                        newCompression = new GH_UnitNumber(new UnitsNet.Pressure(Math.Abs(val) * -1, GhAdSec.DocumentUnits.StressUnit));
                        fck = (UnitsNet.Pressure)newCompression.Value;
                        if (val >= 0)
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Compression (fc) must be negative; note that value has been multiplied by -1");
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert fc input");
                        return;
                    }
                    reCreate = true;
                }

                // 3 Compression strength
                gh_typ = new GH_ObjectWrapper();
                if (DA.GetData(3, ref gh_typ))
                {
                    GH_UnitNumber newTensions;

                    // try cast directly to quantity type
                    if (gh_typ.Value is GH_UnitNumber)
                    {
                        newTensions = (GH_UnitNumber)gh_typ.Value;
                        if (!newTensions.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.PressureUnit)))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 3: Wrong unit type supplied"
                                + System.Environment.NewLine + "Unit type is " + newTensions.Value.QuantityInfo.Name + " but must be Stress (Pressure)");
                            return;
                        }
                        ft = (UnitsNet.Pressure)newTensions.Value.ToUnit(GhAdSec.DocumentUnits.StressUnit);
                    }
                    // try cast to double
                    else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                    {
                        // create new quantity from default units
                        newTensions = new GH_UnitNumber(new UnitsNet.Pressure(val, GhAdSec.DocumentUnits.StressUnit));
                        ft = (UnitsNet.Pressure)newTensions.Value;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert ft input");
                        return;
                    }
                    reCreate = true;
                }

                if (reCreate)
                    concreteCrack = new AdSecConcreteCrackCalculationParametersGoo(e, fck, ft);

                // #### set outputs ####
                DA.SetData(0, concreteCrack);
                DA.SetData(1, new GH_UnitNumber(new UnitsNet.Pressure(e.As(GhAdSec.DocumentUnits.StressUnit), GhAdSec.DocumentUnits.StressUnit)));
                DA.SetData(2, new GH_UnitNumber(new UnitsNet.Pressure(fck.As(GhAdSec.DocumentUnits.StressUnit), GhAdSec.DocumentUnits.StressUnit)));
                DA.SetData(3, new GH_UnitNumber(new UnitsNet.Pressure(ft.As(GhAdSec.DocumentUnits.StressUnit), GhAdSec.DocumentUnits.StressUnit)));

            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ConcreteCrackCalculationParameters are null");
                return;
            }
        }
    }
}
