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

namespace AdSecGH.Components
{
    /// <summary>
    /// Component to create a new Stress Strain Point
    /// </summary>
    public class CreateLoad : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("cbab2b74-2a01-4f05-ba24-2c79827c7415");
        public CreateLoad()
          : base("Create Load", "Load", "Create an AdSec Load from an axial force and biaxial moments",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat3())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.primary;

        //protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.StressStrainPoint;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // force
                dropdownitems.Add(AdSecGH.DocumentUnits.FilteredForceUnits);
                selecteditems.Add(forceUnit.ToString());
                
                // moment
                dropdownitems.Add(AdSecGH.DocumentUnits.FilteredMomentUnits);
                selecteditems.Add(momentUnit.ToString());

                IQuantity force = new UnitsNet.Force(0, forceUnit);
                
                forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
                momentUnitAbbreviation = Oasys.Units.Moment.GetAbbreviation(momentUnit);

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
                    forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), selecteditems[i]);
                    break;
                case 1:
                    momentUnit = (Oasys.Units.MomentUnit)Enum.Parse(typeof(Oasys.Units.MomentUnit), selecteditems[i]);
                    break;
            }

            // update name of inputs (to display unit on sliders)
            ExpireSolution(true);
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }

        private void UpdateUIFromSelectedItems()
        {
            forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), selecteditems[0]);
            momentUnit = (Oasys.Units.MomentUnit)Enum.Parse(typeof(Oasys.Units.MomentUnit), selecteditems[1]);

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
            "Force Unit",
            "Moment Unit"
        });
        private bool first = true;

        private UnitsNet.Units.ForceUnit forceUnit = AdSecGH.DocumentUnits.ForceUnit;
        private Oasys.Units.MomentUnit momentUnit = AdSecGH.DocumentUnits.MomentUnit;
        string forceUnitAbbreviation;
        string momentUnitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Fx [" + forceUnitAbbreviation + "]", "X", "The axial force. Positive x is tension.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Myy [" + momentUnitAbbreviation + "]", "YY", "The moment about local y-axis. Positive yy is anti - clockwise moment about local y-axis.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mzz [" + momentUnitAbbreviation + "]", "ZZ", "The moment about local z-axis. Positive zz is anti - clockwise moment about local z-axis.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Ld", "AdSec Load", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Create new load
            ILoad load = ILoad.Create(
                GetInput.Force(this, DA, 0, forceUnit),
                GetInput.Moment(this, DA, 1, momentUnit),
                GetInput.Moment(this, DA, 2, momentUnit));

            DA.SetData(0, new AdSecLoadGoo(load));
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
            IQuantity force = new UnitsNet.Force(0, forceUnit);
            forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
            momentUnitAbbreviation = Oasys.Units.Moment.GetAbbreviation(momentUnit);
            Params.Input[0].Name = "Fx [" + forceUnitAbbreviation + "]";
            Params.Input[1].Name = "Myy [" + momentUnitAbbreviation + "]";
            Params.Input[2].Name = "Mzz [" + momentUnitAbbreviation + "]";
        }
        #endregion

    }
}