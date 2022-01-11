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
    public class CreatePreLoad : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("cbab2b12-2a01-4f05-ba24-2c79827c7415");
        public CreatePreLoad()
          : base("Create Prestress", "Prestress", "Create an AdSec Prestress Load for Reinforcement Layout as either Preforce, Prestrain or Prestress",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat3())
        { this.Hidden = false; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.Prestress;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (Grasshopper.Instances.DocumentEditor == null) { base.CreateAttributes(); return; } // skip this class during GH loading

            if (first)
            {
                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // type
                List<string> types = new List<string>() { "Force", "Strain", "Stress" };
                dropdownitems.Add(types);
                selecteditems.Add(dropdownitems[0][0]);

                // force
                dropdownitems.Add(Units.FilteredForceUnits);
                selecteditems.Add(forceUnit.ToString());

                IQuantity force = new UnitsNet.Force(0, forceUnit);
                forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
                strainUnitAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);
                IQuantity stress = new UnitsNet.Pressure(0, stressUnit);
                stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));

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
                    case ("Force"):
                        dropdownitems[1] = Units.FilteredForceUnits;
                        selecteditems[0] = forceUnit.ToString();
                        break;
                    case ("Strain"):
                        dropdownitems[1] = Units.FilteredStrainUnits;
                        selecteditems[0] = strainUnit.ToString();
                        break;
                    case ("Stress"):
                        dropdownitems[1] = Units.FilteredStressUnits;
                        selecteditems[0] = stressUnit.ToString();
                        break;
                }
            }
            else
            {
                switch (selecteditems[0])
                {
                    case ("Force"):
                        forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), selecteditems[i]);
                        break;
                    case ("Strain"):
                        strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[i]);
                        break;
                    case ("Stress"):
                        stressUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), selecteditems[i]);
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
            "Type",
            "Measure"
        });
        private bool first = true;

        private UnitsNet.Units.ForceUnit forceUnit = Units.ForceUnit;
        private Oasys.Units.StrainUnit strainUnit = Units.StrainUnit;
        private UnitsNet.Units.PressureUnit stressUnit = Units.StressUnit;
        string forceUnitAbbreviation;
        string strainUnitAbbreviation;
        string stressUnitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("RebarGroup", "RbG", "AdSec Reinforcement Group to apply Preload to", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force [" + forceUnitAbbreviation + "]", "P", "The pre-load per reinforcement bar. Positive value is tension.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Prestressed RebarGroup", "RbG", "Preloaded Rebar Group for AdSec Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get rebargroup
            AdSecRebarGroupGoo in_rebar = GetInput.ReinforcementGroup(this, DA, 0);

            IPreload load = null;
            // Create new load
            switch (selecteditems[0])
            {
                case ("Force"):
                    load = IPreForce.Create(GetInput.Force(this, DA, 1, forceUnit));
                    break;
                case ("Strain"):
                    load = IPreStrain.Create(GetInput.Strain(this, DA, 1, strainUnit));
                    break;
                case ("Stress"):
                    load = IPreStress.Create(GetInput.Stress(this, DA, 1, stressUnit));
                    break;
            }
            ILongitudinalGroup longitudinal = (ILongitudinalGroup)in_rebar.Value;
            longitudinal.Preload = load;
            AdSecRebarGroupGoo out_rebar = new AdSecRebarGroupGoo(longitudinal);
            if (in_rebar.Cover != null)
                out_rebar.Cover = ICover.Create(in_rebar.Cover.UniformCover);

            DA.SetData(0, out_rebar);
            
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Applying prestress will change the up-stream (backwards) rebar object as well " +
                "- please make a copy of the input if you want to have both a rebar with and without prestress. " +
                "This will change in future releases, apologies for the inconvenience...");
        }

        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            AdSecGH.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
            writer.SetString("force", forceUnit.ToString());
            writer.SetString("strain", strainUnit.ToString());
            writer.SetString("stress", stressUnit.ToString());
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (Grasshopper.Instances.DocumentEditor == null) { return base.Read(reader); } // skip this class during GH loading

            AdSecGH.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

            forceUnit = (UnitsNet.Units.ForceUnit)Enum.Parse(typeof(UnitsNet.Units.ForceUnit), reader.GetString("force"));
            strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), reader.GetString("strain"));
            stressUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), reader.GetString("stress"));
            
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
            strainUnitAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);
            IQuantity stress = new UnitsNet.Pressure(0, stressUnit);
            stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));

            if (selecteditems == null) return; // skip this during GH loading

            switch (selecteditems[0])
            {
                case ("Force"):
                    Params.Input[1].Name = "Force [" + forceUnitAbbreviation + "]";
                    Params.Input[1].NickName = "P";
                    break;
                case ("Strain"):
                    Params.Input[1].Name = "Strain [" + strainUnitAbbreviation + "]";
                    Params.Input[1].NickName = "ε";
                    break;
                case ("Stress"):
                    Params.Input[1].Name = "Stress [" + stressUnitAbbreviation + "]";
                    Params.Input[1].NickName = "σ";
                    break;
            }
        }
        #endregion

    }
}