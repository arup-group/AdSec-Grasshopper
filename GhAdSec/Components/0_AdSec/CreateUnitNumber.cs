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
using AdSecGH.Parameters;
using System.Resources;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using UnitsNet.GH;
using UnitsNet;

namespace AdSecGH.Components
{
    /// <summary>
    /// Component to create a new UnitNumber
    /// </summary>
    public class CreateUnitNumber : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("63f42580-8ed7-42fb-8cc6-c6f6171a0248");
        public CreateUnitNumber()
          : base("Create UnitNumber", "CreateUnit", "Create a unit number (quantity) from value, unit and measure",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat9())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateUnitNumber;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // unit types
                dropdownitems.Add(Enum.GetNames(typeof(Units.AdSecUnits)).ToList());
                selecteditems.Add(dropdownitems[0][0]);

                // first type
                dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
                selecteditems.Add(Units.LengthUnit.ToString());

                // set selected unit to
                quantity = new Length(0, Units.LengthUnit);
                selectedMeasure = quantity.Unit;
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));

                // create new dictionary and set all unit measures from quantity to it
                measureDictionary = new Dictionary<string, Enum>();
                foreach (UnitInfo unit in quantity.QuantityInfo.UnitInfos)
                {
                    measureDictionary.Add(unit.Name, unit.Value);
                }

                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }

        public void SetSelected(int i, int j)
        {
            // change selected item
            selecteditems[i] = dropdownitems[i][j];

            // if change is made to first (unit type) list we have to update lists
            if (i == 0)
            {
                // get selected unit
                Units.AdSecUnits unit = (Units.AdSecUnits)Enum.Parse(typeof(Units.AdSecUnits), selecteditems[0]);
                UpdateQuantityUnitTypeFromUnitString(unit);
                UpdateMeasureDictionary();
                selecteditems[1] = selectedMeasure.ToString();
            }
            else // if change is made to the measure of a unit
            {
                selectedMeasure = measureDictionary[selecteditems.Last()];
                UpdateUnitMeasureAndAbbreviation();
            }

            // update name of inputs (to display unit on sliders)
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            ExpireSolution(true);
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }

        private void UpdateUnitMeasureAndAbbreviation()
        {
            // switch case
            switch ((Units.AdSecUnits)Enum.Parse(typeof(Units.AdSecUnits), selecteditems[0]))
            {
                case Units.AdSecUnits.Length:
                    quantity = new Length(val, (UnitsNet.Units.LengthUnit)selectedMeasure);
                    unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                    break;
                case Units.AdSecUnits.Force:
                    quantity = new Force(val, (UnitsNet.Units.ForceUnit)selectedMeasure);
                    unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                    break;
                case Units.AdSecUnits.Moment:
                    quantity = new Oasys.Units.Moment(val, (Oasys.Units.MomentUnit)selectedMeasure);
                    unitAbbreviation = Oasys.Units.Moment.GetAbbreviation((Oasys.Units.MomentUnit)selectedMeasure);
                    break;
                case Units.AdSecUnits.Stress:
                    quantity = new Pressure(val, (UnitsNet.Units.PressureUnit)selectedMeasure);
                    unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                    break;
                case Units.AdSecUnits.Strain:
                    quantity = new Oasys.Units.Strain(val, (Oasys.Units.StrainUnit)selectedMeasure);
                    unitAbbreviation = Oasys.Units.Strain.GetAbbreviation((Oasys.Units.StrainUnit)selectedMeasure);
                    break;
                case Units.AdSecUnits.AxialStiffness:
                    quantity = new Oasys.Units.AxialStiffness(val, (Oasys.Units.AxialStiffnessUnit)selectedMeasure);
                    unitAbbreviation = Oasys.Units.AxialStiffness.GetAbbreviation((Oasys.Units.AxialStiffnessUnit)selectedMeasure);
                    break;
                case Units.AdSecUnits.BendingStiffness:
                    quantity = new Oasys.Units.BendingStiffness(val, (Oasys.Units.BendingStiffnessUnit)selectedMeasure);
                    unitAbbreviation = Oasys.Units.BendingStiffness.GetAbbreviation((Oasys.Units.BendingStiffnessUnit)selectedMeasure);
                    break;
                case Units.AdSecUnits.Curvature:
                    quantity = new Oasys.Units.Curvature(val, (Oasys.Units.CurvatureUnit)selectedMeasure);
                    unitAbbreviation = Oasys.Units.Curvature.GetAbbreviation((Oasys.Units.CurvatureUnit)selectedMeasure);
                    break;
            }
        }

        private void UpdateQuantityUnitTypeFromUnitString(Units.AdSecUnits unit)
        {
            // switch case
            switch (unit)
            {
                case Units.AdSecUnits.Length:
                    quantity = new Length(val, Units.LengthUnit);
                    break;
                case Units.AdSecUnits.Force:
                    quantity = new Force(val, Units.ForceUnit);
                    break;
                case Units.AdSecUnits.Moment:
                    quantity = new Oasys.Units.Moment(val, Units.MomentUnit);
                    break;
                case Units.AdSecUnits.Stress:
                    quantity = new Pressure(val, Units.StressUnit);
                    break;
                case Units.AdSecUnits.Strain:
                    quantity = new Oasys.Units.Strain(val, Units.StrainUnit);
                    break;
                case Units.AdSecUnits.AxialStiffness:
                    quantity = new Oasys.Units.AxialStiffness(val, Units.AxialStiffnessUnit);
                    break;
                case Units.AdSecUnits.BendingStiffness:
                    quantity = new Oasys.Units.BendingStiffness(val, Units.BendingStiffnessUnit);
                    break;
                case Units.AdSecUnits.Curvature:
                    quantity = new Oasys.Units.Curvature(val, Units.CurvatureUnit);
                    break;
            }

            selectedMeasure = quantity.Unit;
        }

        private void UpdateMeasureDictionary()
        {
            // create new dictionary and set all unit measures from quantity to it
            measureDictionary = new Dictionary<string, Enum>();
            foreach (UnitInfo unitype in quantity.QuantityInfo.UnitInfos)
            {
                measureDictionary.Add(unitype.Name, unitype.Value);
            }
            // update dropdown list
            dropdownitems[1] = measureDictionary.Keys.ToList();
        }

        private void UpdateUIFromSelectedItems()
        {
            // get selected unit
            Units.AdSecUnits unit = (Units.AdSecUnits)Enum.Parse(typeof(Units.AdSecUnits), selecteditems[0]);
            UpdateQuantityUnitTypeFromUnitString(unit);
            UpdateMeasureDictionary();
            UpdateUnitMeasureAndAbbreviation();
            selectedMeasure = measureDictionary[selecteditems.Last()];

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            CreateAttributes();
            ExpireSolution(true);
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }
        #endregion

        #region Input and output
        // get list of material types defined in material parameter

        // list of materials
        Dictionary<string, Enum> measureDictionary;
        Enum selectedMeasure;
        // list of lists with all dropdown lists conctent
        List<List<string>> dropdownitems;
        // list of selected items
        List<string> selecteditems;
        // list of descriptions 
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Unit type",
            "Measure"
        });
        private bool first = true;
        IQuantity quantity;
        double val;
        string unitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Number [" + unitAbbreviation + "]", "N", "Number representing the value of selected unit and measure", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("UnitNumber", "UN", "Number converted to selected unit", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (DA.GetData(0, ref val))
            {
                Units.AdSecUnits unit = (Units.AdSecUnits)Enum.Parse(typeof(Units.AdSecUnits), selecteditems[0]);

                // switch case
                switch (unit)
                {
                    case Units.AdSecUnits.Length:
                        quantity = new Length(val, (UnitsNet.Units.LengthUnit)selectedMeasure);
                        break;
                    case Units.AdSecUnits.Force:
                        quantity = new Force(val, (UnitsNet.Units.ForceUnit)selectedMeasure);
                        break;
                    case Units.AdSecUnits.Moment:
                        quantity = new Oasys.Units.Moment(val, (Oasys.Units.MomentUnit)selectedMeasure);
                        break;
                    case Units.AdSecUnits.Stress:
                        quantity = new Pressure(val, (UnitsNet.Units.PressureUnit)selectedMeasure);
                        break;
                    case Units.AdSecUnits.Strain:
                        quantity = new Oasys.Units.Strain(val, (Oasys.Units.StrainUnit)selectedMeasure);
                        break;
                    case Units.AdSecUnits.AxialStiffness:
                        quantity = new Oasys.Units.AxialStiffness(val, (Oasys.Units.AxialStiffnessUnit)selectedMeasure);
                        break;
                    case Units.AdSecUnits.BendingStiffness:
                        quantity = new Oasys.Units.BendingStiffness(val, (Oasys.Units.BendingStiffnessUnit)selectedMeasure);
                        break;
                    case Units.AdSecUnits.Curvature:
                        quantity = new Oasys.Units.Curvature(val, (Oasys.Units.CurvatureUnit)selectedMeasure);
                        break;
                }

                // convert unit to selected output
                GH_UnitNumber unitNumber = new GH_UnitNumber(quantity);

                // set output data
                DA.SetData(0, unitNumber);
            }
        }

        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);

            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
            UpdateUIFromSelectedItems();
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
            Params.Input[0].Name = "Number [" + unitAbbreviation + "]";
        }
        #endregion
    }
}