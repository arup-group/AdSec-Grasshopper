using System;
using System.Linq;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using OasysGH.Components;
using UnitsNet;

namespace AdSecGH.Components
{
    /// <summary>
    /// Component to create a new Stress Strain Point
    /// </summary>
    public class CreateLoad : GH_OasysComponent, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("cbab2b74-2a01-4f05-ba24-2c79827c7415");
        public CreateLoad()
          : base("Create Load", "Load", "Create an AdSec Load from an axial force and biaxial moments",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat5())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateLoad;
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
                dropdownitems.Add(Units.FilteredForceUnits);
                selecteditems.Add(forceUnit.ToString());
                
                // moment
                dropdownitems.Add(Units.FilteredMomentUnits);
                selecteditems.Add(momentUnit.ToString());

                IQuantity force = new Force(0, forceUnit);
                
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
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            ExpireSolution(true);
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }

        private void UpdateUIFromSelectedItems()
        {
            forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), selecteditems[0]);
            momentUnit = (Oasys.Units.MomentUnit)Enum.Parse(typeof(Oasys.Units.MomentUnit), selecteditems[1]);

            CreateAttributes();
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            ExpireSolution(true);
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

        private UnitsNet.Units.ForceUnit forceUnit = Units.ForceUnit;
        private Oasys.Units.MomentUnit momentUnit = Units.MomentUnit;
        string forceUnitAbbreviation;
        string momentUnitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Fx [" + forceUnitAbbreviation + "]", "X", "The axial force. Positive x is tension.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Myy [" + momentUnitAbbreviation + "]", "YY", "The moment about local y-axis. Positive yy is anti - clockwise moment about local y-axis.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Mzz [" + momentUnitAbbreviation + "]", "ZZ", "The moment about local z-axis. Positive zz is anti - clockwise moment about local z-axis.", GH_ParamAccess.item);
            // make all but last input optional
            for (int i = 0; i < pManager.ParamCount - 1; i++)
                pManager[i].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Ld", "AdSec Load", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Create new load
            ILoad load = ILoad.Create(
                GetInput.Force(this, DA, 0, forceUnit, true),
                GetInput.Moment(this, DA, 1, momentUnit, true),
                GetInput.Moment(this, DA, 2, momentUnit, true));

            // check for enough input parameters
            if (this.Params.Input[0].SourceCount == 0 && this.Params.Input[1].SourceCount == 0
                && this.Params.Input[2].SourceCount == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameters " + this.Params.Input[0].NickName + ", " +
                    this.Params.Input[1].NickName + ", and " + this.Params.Input[2].NickName + " failed to collect data!");
                return;
            }

            DA.SetData(0, new AdSecLoadGoo(load));
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
            IQuantity force = new Force(0, forceUnit);
            forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
            momentUnitAbbreviation = Oasys.Units.Moment.GetAbbreviation(momentUnit);
            Params.Input[0].Name = "Fx [" + forceUnitAbbreviation + "]";
            Params.Input[1].Name = "Myy [" + momentUnitAbbreviation + "]";
            Params.Input[2].Name = "Mzz [" + momentUnitAbbreviation + "]";
        }
        #endregion

    }
}