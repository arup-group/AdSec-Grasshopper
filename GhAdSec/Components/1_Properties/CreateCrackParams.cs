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

        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.CreateCrackParams;
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
            // create new ccp
            AdSecConcreteCrackCalculationParametersGoo ccp = new AdSecConcreteCrackCalculationParametersGoo(
                GetInput.Stress(this, DA, 0, stressUnitE),
                GetInput.Stress(this, DA, 1, strengthUnit),
                GetInput.Stress(this, DA, 2, strengthUnit));

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