using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using GhAdSec.Parameters;
using System.Resources;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using UnitsNet.GH;
using UnitsNet;

namespace GhAdSec.Components
{
    /// <summary>
    /// Component to create a new Concrete Crack Calculation Parameters
    /// </summary>
    public class CreateConcreteCrackCalculationParameters : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("bc810b4b-11f1-496f-b949-a0be77e9bdc8");
        public CreateConcreteCrackCalculationParameters()
          : base("Create CrackCalcParams", "CrackCalcParams", "Create Concrete Crack Calculation Parameters for AdSec Material",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        //protected override System.Drawing.Bitmap Icon => GhSA.Properties.Resources.CreateMaterial;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // pressure E
                dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.PressureUnit)).ToList());
                selecteditems.Add(strengthUnit.ToString());

                // pressure stress
                dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.PressureUnit)).ToList());
                selecteditems.Add(strengthUnit.ToString());

                IQuantity quantityE = new UnitsNet.Pressure(0, stressUnitE);
                unitEAbbreviation = string.Concat(quantityE.ToString().Where(char.IsLetter));
                IQuantity quantityS = new UnitsNet.Pressure(0, strengthUnit);
                unitSAbbreviation = string.Concat(quantityS.ToString().Where(char.IsLetter));

                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }

        public void SetSelected(int i, int j)
        {
            // change selected item
            selecteditems[i] = dropdownitems[i][j];

            switch (i)
            {
                case 0:
                    stressUnitE = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[i]);
                    break;
                case 1:
                    strengthUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[i]);
                    break;
            }

            // update name of inputs (to display unit on sliders)
            ExpireSolution(true);
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }
        #endregion

        #region Input and output
        
        // list of lists with all dropdown lists conctent
        List<List<string>> dropdownitems;
        // list of selected items
        List<string> selecteditems;
        // list of descriptions 
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Elasticity Unit",
            "Strength Unit"
        });
        private bool first = true;
        private UnitsNet.Units.PressureUnit stressUnitE = GhAdSec.DocumentUnits.StressUnit;
        private UnitsNet.Units.PressureUnit strengthUnit = GhAdSec.DocumentUnits.StressUnit;
        string unitEAbbreviation;
        string unitSAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elastic Modulus [" + unitEAbbreviation + "]", "E", "Value for Elastic Modulus", GH_ParamAccess.item);
            pManager.AddGenericParameter("Compression [" + unitSAbbreviation + "]", "fc", "Value for Characteristic Compressive Strength", GH_ParamAccess.item);
            pManager.AddGenericParameter("Tension [" + unitSAbbreviation + "]", "ft", "Value for Characteristic Tension Strength", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CrackCalcParams", "CCP", "AdSec ConcreteCrackCalculationParameters", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            UnitsNet.Pressure e = new UnitsNet.Pressure();
            UnitsNet.Pressure fck = new UnitsNet.Pressure();
            UnitsNet.Pressure ft = new UnitsNet.Pressure();

            // 0 Elastic modulus
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(0, ref gh_typ))
            {
                GH_UnitNumber newElastic;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    newElastic = (GH_UnitNumber)gh_typ.Value;
                    if (!newElastic.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.PressureUnit)))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 2: Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + newElastic.Value.QuantityInfo.Name + " but must be Stress (Pressure)");
                        return;
                    }
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    newElastic = new GH_UnitNumber(new UnitsNet.Pressure(val, stressUnitE));
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert E input");
                    return;
                }
                e = (UnitsNet.Pressure)newElastic.Value.ToUnit(stressUnitE);
            }

            // 1 Compression strength
            gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(1, ref gh_typ))
            {
                GH_UnitNumber newCompression;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    newCompression = (GH_UnitNumber)gh_typ.Value;
                    if (!newCompression.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.PressureUnit)))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 1: Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + newCompression.Value.QuantityInfo.Name + " but must be Stress (Pressure)");
                        return;
                    }
                    fck = (UnitsNet.Pressure)newCompression.Value.ToUnit(strengthUnit);
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
                    newCompression = new GH_UnitNumber(new UnitsNet.Pressure(Math.Abs(val) * -1, strengthUnit));
                    fck = (UnitsNet.Pressure)newCompression.Value;
                    if (val >= 0)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Compression (fc) must be negative; note that value has been multiplied by -1");
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert fck input");
                    return;
                }
            }

            // 2 Compression strength
            gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(2, ref gh_typ))
            {
                GH_UnitNumber newTensions;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    newTensions = (GH_UnitNumber)gh_typ.Value;
                    if (!newTensions.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.PressureUnit)))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 2: Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + newTensions.Value.QuantityInfo.Name + " but must be Stress (Pressure)");
                        return;
                    }
                    ft = (UnitsNet.Pressure)newTensions.Value.ToUnit(strengthUnit);
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    newTensions = new GH_UnitNumber(new UnitsNet.Pressure(val, strengthUnit));
                    ft = (UnitsNet.Pressure)newTensions.Value;
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert ft input");
                    return;
                }
            }

            // create new ccp
            AdSecConcreteCrackCalculationParametersGoo ccp = new AdSecConcreteCrackCalculationParametersGoo(e, fck, ft);

            DA.SetData(0, ccp);
        }
        
        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            GhAdSec.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);

            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            GhAdSec.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

            stressUnitE = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[0]);
            strengthUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[1]);

            first = false;
            return base.Read(reader);
        }
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            return null;
        }
        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return false;
        }
        #endregion

        #region IGH_VariableParameterComponent null implementation
        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {
            IQuantity quantityE = new UnitsNet.Pressure(0, stressUnitE);
            unitEAbbreviation = string.Concat(quantityE.ToString().Where(char.IsLetter));
            IQuantity quantityS = new UnitsNet.Pressure(0, strengthUnit);
            unitSAbbreviation = string.Concat(quantityS.ToString().Where(char.IsLetter));
            Params.Input[0].Name = "Elastic Modulus [" + unitEAbbreviation + "]";
            Params.Input[1].Name = "Compression [" + unitSAbbreviation + "]";
            Params.Input[2].Name = "Tension [" + unitSAbbreviation + "]";
        }
        #endregion
    }
}