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
    public class CreateDeformation : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("cbab2b58-2a01-4f05-ba24-2c79827c7415");
        public CreateDeformation()
          : base("Create Deformation Load", "Deformation", "Create an AdSec Deformation Load from an axial strain and biaxial curvatures",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat5())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.DeformationLoad;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // strain
                dropdownitems.Add(Units.FilteredStrainUnits);
                selecteditems.Add(strainUnit.ToString());

                // curvature
                dropdownitems.Add(Units.FilteredCurvatureUnits);
                selecteditems.Add(curvatureUnit.ToString());

                strainUnitAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);
                curvatureUnitAbbreviation = Oasys.Units.Curvature.GetAbbreviation(curvatureUnit);

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
                    strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[i]);
                    break;
                case 1:
                    curvatureUnit = (Oasys.Units.CurvatureUnit)Enum.Parse(typeof(Oasys.Units.CurvatureUnit), selecteditems[i]);
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
            strainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), selecteditems[0]);
            curvatureUnit = (Oasys.Units.CurvatureUnit)Enum.Parse(typeof(Oasys.Units.CurvatureUnit), selecteditems[1]);

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
            "Strain Unit",
            "Curvature Unit"
        });
        private bool first = true;

        private Oasys.Units.StrainUnit strainUnit = Units.StrainUnit;
        private Oasys.Units.CurvatureUnit curvatureUnit = Units.CurvatureUnit;
        string strainUnitAbbreviation;
        string curvatureUnitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("εx [" + strainUnitAbbreviation + "]", "X", "The axial strain. Positive X indicates tension.", GH_ParamAccess.item);
            pManager.AddGenericParameter("κyy [" + curvatureUnitAbbreviation + "]", "YY", "The curvature about local y-axis. It follows the right hand grip rule about the axis. Positive YY is anti-clockwise curvature about local y-axis.", GH_ParamAccess.item);
            pManager.AddGenericParameter("κzz [" + curvatureUnitAbbreviation + "]", "ZZ", "The curvature about local z-axis. It follows the right hand grip rule about the axis. Positive ZZ is anti-clockwise curvature about local z-axis.", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "Ld", "AdSec Load", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Create new load
            IDeformation deformation = IDeformation.Create(
                GetInput.Strain(this, DA, 0, strainUnit),
                GetInput.Curvature(this, DA, 1, curvatureUnit),
                GetInput.Curvature(this, DA, 2, curvatureUnit));

            DA.SetData(0, new AdSecDeformationGoo(deformation));
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
            strainUnitAbbreviation = Oasys.Units.Strain.GetAbbreviation(strainUnit);
            curvatureUnitAbbreviation = Oasys.Units.Curvature.GetAbbreviation(curvatureUnit);
            Params.Input[0].Name = "εx [" + strainUnitAbbreviation + "]";
            Params.Input[1].Name = "κyy [" + curvatureUnitAbbreviation + "]";
            Params.Input[2].Name = "κzz [" + curvatureUnitAbbreviation + "]";
        }
        #endregion

    }
}