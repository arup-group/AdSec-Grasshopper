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
                Ribbon.SubCategoryName.Cat0())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.CreateUnit;
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
                dropdownitems.Add(Enum.GetNames(typeof(DocumentUnits.AdSecUnits)).ToList());
                selecteditems.Add(dropdownitems[0][0]);

                // first type
                dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
                selecteditems.Add(DocumentUnits.LengthUnit.ToString());

                // set selected unit to
                quantity = new UnitsNet.Length(0, DocumentUnits.LengthUnit);
                selectedUnit = quantity.Unit;
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));

                // create new dictionary and set all unit measures from quantity to it
                unitDict = new Dictionary<string, Enum>();
                foreach (UnitsNet.UnitInfo unit in quantity.QuantityInfo.UnitInfos)
                {
                    unitDict.Add(unit.Name, unit.Value);
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
                DocumentUnits.AdSecUnits unit = (DocumentUnits.AdSecUnits)Enum.Parse(typeof(DocumentUnits.AdSecUnits), selecteditems[0]);

                // switch case
                switch (unit)
                {
                    case DocumentUnits.AdSecUnits.Length:
                        quantity = new UnitsNet.Length(val, DocumentUnits.LengthUnit);
                        selectedUnit = quantity.Unit;
                        break;
                    case DocumentUnits.AdSecUnits.Force:
                        quantity = new UnitsNet.Force(val, DocumentUnits.ForceUnit);
                        selectedUnit = quantity.Unit;
                        break;
                    case DocumentUnits.AdSecUnits.Moment:
                        quantity = new Oasys.Units.Moment(val, DocumentUnits.MomentUnit);
                        selectedUnit = quantity.Unit;
                        break;
                    case DocumentUnits.AdSecUnits.Stress:
                        quantity = new UnitsNet.Pressure(val, DocumentUnits.StressUnit);
                        selectedUnit = quantity.Unit;
                        break;
                    case DocumentUnits.AdSecUnits.Strain:
                        quantity = new Oasys.Units.Strain(val, DocumentUnits.StrainUnit);
                        selectedUnit = quantity.Unit;
                        break;
                    case DocumentUnits.AdSecUnits.AxialStiffness:
                        quantity = new Oasys.Units.AxialStiffness(val, DocumentUnits.AxialStiffnessUnit);
                        selectedUnit = quantity.Unit;
                        break;
                    case DocumentUnits.AdSecUnits.BendingStiffness:
                        quantity = new Oasys.Units.BendingStiffness(val, DocumentUnits.BendingStiffnessUnit);
                        selectedUnit = quantity.Unit;
                        break;
                    case DocumentUnits.AdSecUnits.Curvature:
                        quantity = new Oasys.Units.Curvature(val, DocumentUnits.CurvatureUnit);
                        selectedUnit = quantity.Unit;
                        break;
                }

                // create new dictionary and set all unit measures from quantity to it
                unitDict = new Dictionary<string, Enum>();
                foreach (UnitsNet.UnitInfo unitype in quantity.QuantityInfo.UnitInfos)
                {
                    unitDict.Add(unitype.Name, unitype.Value);
                }
                // update dropdown list
                dropdownitems[1] = unitDict.Keys.ToList();
                // set selected to default unit
                selecteditems[1] = selectedUnit.ToString();
                
            }
            else // if change is made to the measure of a unit
            {
                selectedUnit = unitDict[selecteditems.Last()];
            }

            // update name of input (to display unit on sliders)
            ExpireSolution(true);
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }

        private void UpdateUIFromSelectedItems()
        {
            // get selected unit
            DocumentUnits.AdSecUnits unit = (DocumentUnits.AdSecUnits)Enum.Parse(typeof(DocumentUnits.AdSecUnits), selecteditems[0]);

            // switch case
            switch (unit)
            {
                case DocumentUnits.AdSecUnits.Length:
                    quantity = new UnitsNet.Length(val, DocumentUnits.LengthUnit);
                    selectedUnit = quantity.Unit;
                    break;
                case DocumentUnits.AdSecUnits.Force:
                    quantity = new UnitsNet.Force(val, DocumentUnits.ForceUnit);
                    selectedUnit = quantity.Unit;
                    break;
                case DocumentUnits.AdSecUnits.Moment:
                    quantity = new Oasys.Units.Moment(val, DocumentUnits.MomentUnit);
                    selectedUnit = quantity.Unit;
                    break;
                case DocumentUnits.AdSecUnits.Stress:
                    quantity = new UnitsNet.Pressure(val, DocumentUnits.StressUnit);
                    selectedUnit = quantity.Unit;
                    break;
                case DocumentUnits.AdSecUnits.Strain:
                    quantity = new Oasys.Units.Strain(val, DocumentUnits.StrainUnit);
                    selectedUnit = quantity.Unit;
                    break;
                case DocumentUnits.AdSecUnits.AxialStiffness:
                    quantity = new Oasys.Units.AxialStiffness(val, DocumentUnits.AxialStiffnessUnit);
                    selectedUnit = quantity.Unit;
                    break;
                case DocumentUnits.AdSecUnits.BendingStiffness:
                    quantity = new Oasys.Units.BendingStiffness(val, DocumentUnits.BendingStiffnessUnit);
                    selectedUnit = quantity.Unit;
                    break;
                case DocumentUnits.AdSecUnits.Curvature:
                    quantity = new Oasys.Units.Curvature(val, DocumentUnits.CurvatureUnit);
                    selectedUnit = quantity.Unit;
                    break;
            }

            // create new dictionary and set all unit measures from quantity to it
            unitDict = new Dictionary<string, Enum>();
            foreach (UnitsNet.UnitInfo unitype in quantity.QuantityInfo.UnitInfos)
            {
                unitDict.Add(unitype.Name, unitype.Value);
            }
            // update dropdown list
            dropdownitems[1] = unitDict.Keys.ToList();

            selectedUnit = unitDict[selecteditems.Last()];

            CreateAttributes();
            ExpireSolution(true);
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }
        #endregion

        #region Input and output
        // get list of material types defined in material parameter

        // list of materials
        Dictionary<string, Enum> unitDict;
        Enum selectedUnit;
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
                DocumentUnits.AdSecUnits unit = (DocumentUnits.AdSecUnits)Enum.Parse(typeof(DocumentUnits.AdSecUnits), selecteditems[0]);

                // switch case
                switch (unit)
                {
                    case DocumentUnits.AdSecUnits.Length:
                        quantity = new UnitsNet.Length(val, (UnitsNet.Units.LengthUnit)selectedUnit);
                        break;
                    case DocumentUnits.AdSecUnits.Force:
                        quantity = new UnitsNet.Force(val, (UnitsNet.Units.ForceUnit)selectedUnit);
                        break;
                    case DocumentUnits.AdSecUnits.Moment:
                        quantity = new Oasys.Units.Moment(val, (Oasys.Units.MomentUnit)selectedUnit);
                        break;
                    case DocumentUnits.AdSecUnits.Stress:
                        quantity = new UnitsNet.Pressure(val, (UnitsNet.Units.PressureUnit)selectedUnit);
                        break;
                    case DocumentUnits.AdSecUnits.Strain:
                        quantity = new Oasys.Units.Strain(val, (Oasys.Units.StrainUnit)selectedUnit);
                        break;
                    case DocumentUnits.AdSecUnits.AxialStiffness:
                        quantity = new Oasys.Units.AxialStiffness(val, (Oasys.Units.AxialStiffnessUnit)selectedUnit);
                        break;
                    case DocumentUnits.AdSecUnits.BendingStiffness:
                        quantity = new Oasys.Units.BendingStiffness(val, (Oasys.Units.BendingStiffnessUnit)selectedUnit);
                        break;
                    case DocumentUnits.AdSecUnits.Curvature:
                        quantity = new Oasys.Units.Curvature(val, (Oasys.Units.CurvatureUnit)selectedUnit);
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
            AdSecGH.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);

            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            AdSecGH.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
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
            unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
            Params.Input[0].Name = "Number [" + unitAbbreviation + "]";
        }
        #endregion
    }
}