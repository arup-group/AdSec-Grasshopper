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
using Oasys.AdSec.Materials.StressStrainCurves;
using UnitsNet.GH;
using UnitsNet;
using Oasys.AdSec;
using Oasys.AdSec.Reinforcement.Preloads;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement;

namespace AdSecGH.Components
{
    /// <summary>
    /// Component to create a new Stress Strain Point
    /// </summary>
    public class NMDiagram : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("21cd9e4c-6c85-4077-b575-1e04127f2998");
        public NMDiagram()
          : base("N-M Diagram", "N-M", "Calculates a force-moment (N-M) or moment-moment (M-M) interaction curve.",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat5())
        { this.Hidden = false; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.Prestress;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // type
                List<string> types = new List<string>() { "N-M", "M-M" };
                dropdownitems.Add(types);
                selecteditems.Add(dropdownitems[0][0]);

                // force
                dropdownitems.Add(DocumentUnits.FilteredAngleUnits);
                selecteditems.Add(angleUnit.ToString());

                IQuantity force = new UnitsNet.Force(0, forceUnit);
                forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
                IQuantity angle = new UnitsNet.Angle(0, angleUnit);
                angleUnitAbbreviation = string.Concat(angle.ToString().Where(char.IsLetter));

                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }

        public void SetSelected(int i, int j)
        {
            // change selected item
            selecteditems[i] = dropdownitems[i][j];

            if (i == 0)
            {
                switch (selecteditems[0])
                {
                    case ("N-M"):
                        dropdownitems[1] = DocumentUnits.FilteredAngleUnits;
                        selecteditems[0] = angleUnit.ToString();
                        break;
                    case ("M-M"):
                        dropdownitems[1] = DocumentUnits.FilteredForceUnits;
                        selecteditems[0] = forceUnit.ToString();
                        break;
                }
            }
            else
            {
                switch (selecteditems[0])
                {
                    case ("N-M"):
                        angleUnit = (UnitsNet.Units.AngleUnit)Enum.Parse(typeof(UnitsNet.Units.AngleUnit), selecteditems[i]);
                        break;
                    case ("M-M"):
                        forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), selecteditems[i]);
                        break;
                }
            }

            // update name of inputs (to display unit on sliders)
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            ExpireSolution(true);
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }
        private void UpdateUIFromSelectedItems()
        {
            CreateAttributes();
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
            "Interaction",
            "Measure"
        });
        private bool first = true;

        private UnitsNet.Units.ForceUnit forceUnit = DocumentUnits.ForceUnit;
        private UnitsNet.Units.AngleUnit angleUnit = UnitsNet.Units.AngleUnit.Radian;
        string forceUnitAbbreviation;
        string angleUnitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Results", "Res", "AdSec Results to calculate interaction diagram from", GH_ParamAccess.item);
            pManager.AddGenericParameter("Moment Angle [" + angleUnitAbbreviation + "]", "A", "The moment angle, which must be in the range -180 degrees to +180 degrees.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Prestressed RebarGroup", "RbG", "Preloaded Rebar Group for AdSec Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
        }

        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            AdSecGH.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
            writer.SetString("force", forceUnit.ToString());
            writer.SetString("angle", angleUnit.ToString());
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            AdSecGH.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

            forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), reader.GetString("force"));
            angleUnit = (UnitsNet.Units.AngleUnit)Enum.Parse(typeof(UnitsNet.Units.AngleUnit), reader.GetString("angle"));
            
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
            
            

            switch (selecteditems[0])
            {
                case ("N-M"):
                    IQuantity angle = new UnitsNet.Angle(0, angleUnit);
                    angleUnitAbbreviation = string.Concat(angle.ToString().Where(char.IsLetter));
                    Params.Input[1].Name = "Moment Angle [" + angleUnitAbbreviation + "]";
                    Params.Input[1].NickName = "A";
                    Params.Input[1].Description = "The moment angle, which must be in the range -180 degrees to +180 degrees.";
                    break;
                case ("M-M"):
                    IQuantity force = new UnitsNet.Force(0, forceUnit);
                    forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
                    Params.Input[1].Name = "Axial Force [" + forceUnitAbbreviation + "]";
                    Params.Input[1].NickName = "F";
                    Params.Input[1].Description = "The axial force to calculate the moment-moment diagram for.";
                    break;
            }
        }
        #endregion

    }
}