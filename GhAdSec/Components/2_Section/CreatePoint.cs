using System;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.Units;
using UnitsNet;
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
using UnitsNet.GH;

namespace GhAdSec.Components
{
    public class CreatePoint : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("1a0cdb3c-d66d-420e-a9d8-35d31587a122");
        public CreatePoint()
          : base("Create Vertex Point", "Vertex Point", "Create a A 2D vertex in the yz-plane for AdSec Profile and Reinforcement",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.CreateVertex;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // length
                dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
                selecteditems.Add(lengthUnit.ToString());

                IQuantity quantity = new UnitsNet.Length(0, lengthUnit);
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));

                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }
        public void SetSelected(int i, int j)
        {
            // change selected item
            selecteditems[i] = dropdownitems[i][j];

            lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[i]);

            // update name of inputs (to display unit on sliders)
            ExpireSolution(true);
            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }
        // list of lists with all dropdown lists conctent
        List<List<string>> dropdownitems;
        // list of selected items
        List<string> selecteditems;
        // list of descriptions 
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Measure",
        });
        private bool first = true;
        private UnitsNet.Units.LengthUnit lengthUnit = GhAdSec.DocumentUnits.LengthUnit;
        string unitAbbreviation;
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Y [" + unitAbbreviation + "]", "Y", "The local Y coordinate in yz-plane", GH_ParamAccess.item);
            pManager.AddGenericParameter("Z [" + unitAbbreviation + "]", "Z", "The local Z coordinate in yz-plane", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Vertex Point", "Vx", "A 2D vertex in the yz-plane for AdSec Profile and Reinforcement", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // get inputs
            Length y = GetInput.Length(this, DA, 0, lengthUnit);
            Length z = GetInput.Length(this, DA, 1, lengthUnit);

            // create IPoint
            IPoint pt = IPoint.Create(y, z);

            // Convert to AdSecPointGoo param
            AdSecPointGoo point = new AdSecPointGoo(pt);

            // set output
            DA.SetData(0, point);
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

            lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[0]);

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
            IQuantity quantity = new UnitsNet.Length(0, lengthUnit);
            unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
            Params.Input[0].Name = "Y [" + unitAbbreviation + "]";
            Params.Input[1].Name = "Z [" + unitAbbreviation + "]";
        }
        #endregion
    }
}
