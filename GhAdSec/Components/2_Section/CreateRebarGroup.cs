using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Oasys.AdSec.Materials.StressStrainCurves;
using GhAdSec.Parameters;
using UnitsNet.GH;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement.Groups;
using UnitsNet;
using Oasys.AdSec.Reinforcement.Layers;

namespace GhAdSec.Components
{
    public class CreateReinforcementGroup : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        public CreateReinforcementGroup()
            : base("Create Reinforcement Layout", "Reinforcement Layout", "Create Reinforcement Layout for AdSec Section",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        { this.Hidden = true; }
        public override Guid ComponentGuid => new Guid("1250f456-de99-4834-8d7f-4019cc0c70ba");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.RebarLayout;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                List<string> list = Enum.GetNames(typeof(FoldMode)).ToList();
                dropdownitems = new List<List<string>>();
                dropdownitems.Add(list);

                selecteditems = new List<string>();
                selecteditems.Add(dropdownitems[0][0]);

                // populate unit abbriviations and add to selected items to have list length of 3 always
                IQuantity quantity = new UnitsNet.Length(0, lengthUnit);
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                selecteditems.Add(lengthUnit.ToString());

                IQuantity quantityAngle = new UnitsNet.Angle(0, angleUnit);
                angleAbbreviation = string.Concat(quantityAngle.ToString().Where(char.IsLetter));
                selecteditems.Add(angleUnit.ToString());

                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }

        public void SetSelected(int i, int j)
        {
            // set selected item
            selecteditems[i] = dropdownitems[i][j];
            if (i == 0)
            {
                _mode = (FoldMode)Enum.Parse(typeof(FoldMode), selecteditems[i]);

                switch (_mode)
                {
                    case FoldMode.Line:
                        while (dropdownitems.Count > 1)
                            dropdownitems.RemoveAt(1);
                        spacerDescriptions[1] = "Measure";
                        break;
                    case FoldMode.Arc:
                        if (dropdownitems.Count < 2)
                            dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
                        if (dropdownitems.Count < 3)
                            dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.AngleUnit)).ToList());
                        spacerDescriptions[1] = "Length measure";
                        break;
                    case FoldMode.Circle:
                        if (dropdownitems.Count < 2)
                            dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
                        if (dropdownitems.Count < 3)
                            dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.AngleUnit)).ToList());
                        spacerDescriptions[1] = "Length measure";
                        break;
                    case FoldMode.SingleBars:
                        while (dropdownitems.Count > 1)
                            dropdownitems.RemoveAt(1);
                        break;
                }
            }
            else
            {
                switch (i)
                {
                    case 1:
                        lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[i]);
                        break;

                    case 2:
                        angleUnit = (UnitsNet.Units.AngleUnit)Enum.Parse(typeof(UnitsNet.Units.AngleUnit), selecteditems[i]);
                        break;
                }
            }
            ToggleInput();
            this.OnDisplayExpired(true);
        }
        #endregion

        #region Input and output
        List<List<string>> dropdownitems;
        List<string> selecteditems;
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Group Type",
            "Measure",
            "Angular measure"
        });
        private UnitsNet.Units.LengthUnit lengthUnit = GhAdSec.DocumentUnits.LengthUnit;
        private UnitsNet.Units.AngleUnit angleUnit = UnitsNet.Units.AngleUnit.Radian;
        string unitAbbreviation;
        string angleAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Spaced Rebars", "RbS", "AdSec Rebars Spaced in a Layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Position 1", "Vx1", "First bar position", GH_ParamAccess.item);
            pManager.AddGenericParameter("Position 2", "Vx2", "Last bar position", GH_ParamAccess.item);
            _mode = FoldMode.Line;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Layout", "RbL", "Rebar Layout for AdSec Section", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            AdSecRebarGroupGoo group = null;

            switch (_mode)
            {
                case FoldMode.Line:

                    // create line group
                    group = new AdSecRebarGroupGoo(
                        ILineGroup.Create(
                            GetInput.IPoint(this, DA, 1),
                            GetInput.IPoint(this, DA, 2),
                            GetInput.ILayer(this, DA, 0)));
                    break;

                case FoldMode.Circle:

                    // create circle rebar group
                    group = new AdSecRebarGroupGoo(
                        ICircleGroup.Create(
                            GetInput.IPoint(this, DA, 1),
                            GetInput.Length(this, DA, 2, lengthUnit),
                            GetInput.Angle(this, DA, 3, angleUnit),
                            GetInput.ILayer(this, DA, 0)));

                    break;

                case FoldMode.Arc:

                    // create arc rebar grouup
                    group = new AdSecRebarGroupGoo(
                        IArcGroup.Create(
                            GetInput.IPoint(this, DA, 1),
                            GetInput.Length(this, DA, 2, lengthUnit),
                            GetInput.Angle(this, DA, 3, angleUnit),
                            GetInput.Angle(this, DA, 4, angleUnit),
                            GetInput.ILayer(this, DA, 0)));
                    break;

                case FoldMode.SingleBars:

                    // create single rebar group
                    ISingleBars bars = ISingleBars.Create(
                        GetInput.IBarBundle(this, DA, 0));

                    bars.Positions = GetInput.IPoints(this, DA, 1);

                    group = new AdSecRebarGroupGoo(bars);

                    break;
            }

            // set output
            DA.SetData(0, group);
        }

        #region menu override
        
        private bool first = true;
        private enum FoldMode
        {
            Line,
            SingleBars,
            Circle,
            Arc,
        }

        private FoldMode _mode = FoldMode.Line;

        private void ToggleInput()
        {
            RecordUndoEvent("Changed dropdown");

            switch (_mode)
            {
                case FoldMode.Line:
                    // remove any additional input parameters
                    while (Params.Input.Count > 1)
                        Params.UnregisterInputParameter(Params.Input[1], true);
                    // register 2 generic
                    Params.RegisterInputParam(new Param_GenericObject());
                    Params.RegisterInputParam(new Param_GenericObject());
                    break;

                case FoldMode.SingleBars:
                    // remove any additional input parameters
                    while (Params.Input.Count > 1)
                        Params.UnregisterInputParameter(Params.Input[1], true);
                    // register 1 generic
                    Params.RegisterInputParam(new Param_GenericObject());
                    break;

                case FoldMode.Circle:
                    // remove any additional input parameters
                    while (Params.Input.Count > 1)
                        Params.UnregisterInputParameter(Params.Input[1], true);
                    // register 3 generic
                    Params.RegisterInputParam(new Param_GenericObject());
                    Params.RegisterInputParam(new Param_GenericObject());
                    Params.RegisterInputParam(new Param_GenericObject());
                    break;

                case FoldMode.Arc:
                    // remove any additional input parameters
                    while (Params.Input.Count > 1)
                        Params.UnregisterInputParameter(Params.Input[1], true);
                    // register 4 generic
                    Params.RegisterInputParam(new Param_GenericObject());
                    Params.RegisterInputParam(new Param_GenericObject());
                    Params.RegisterInputParam(new Param_GenericObject());
                    Params.RegisterInputParam(new Param_GenericObject());
                    break;
            }

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
        #endregion

        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            GhAdSec.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            GhAdSec.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
            _mode = (FoldMode)Enum.Parse(typeof(FoldMode), reader.GetString(selecteditems[0]));
            lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[1]);
            angleUnit = (UnitsNet.Units.AngleUnit)Enum.Parse(typeof(UnitsNet.Units.AngleUnit), selecteditems[2]);
            
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
            

            if (_mode == FoldMode.Line)
            {
                //pManager.AddGenericParameter("Spaced Rebars", "RbS", "AdSec Rebars Spaced in a Layer", GH_ParamAccess.item);
                //pManager.AddGenericParameter("Position 1", "P1", "First bar position", GH_ParamAccess.item);
                //pManager.AddGenericParameter("Position 2", "P2", "Last bar position", GH_ParamAccess.item);

                Params.Input[0].Name = "Spaced Rebars";
                Params.Input[0].NickName = "RbS";
                Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Position 1";
                Params.Input[1].NickName = "Vx1";
                Params.Input[1].Description = "First bar position";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;

                Params.Input[2].Name = "Position 2";
                Params.Input[2].NickName = "Vx2";
                Params.Input[2].Description = "Last bar position";
                Params.Input[2].Access = GH_ParamAccess.item;
                Params.Input[2].Optional = false;

            }
            if (_mode == FoldMode.SingleBars)
            {
                Params.Input[0].Name = "Rebar";
                Params.Input[0].NickName = "Rb";
                Params.Input[0].Description = "AdSec Rebar (single or bundle)";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Position(s)";
                Params.Input[1].NickName = "Vxs";
                Params.Input[1].Description = "List of bar positions";
                Params.Input[1].Access = GH_ParamAccess.list;
                Params.Input[1].Optional = false;
            }
            if (_mode == FoldMode.Circle)
            {
                Params.Input[0].Name = "Spaced Rebars";
                Params.Input[0].NickName = "RbS";
                Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Centre";
                Params.Input[1].NickName = "CVx";
                Params.Input[1].Description = "Vertex Point representing the centre of the circle";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;

                IQuantity quantity = new UnitsNet.Length(0, lengthUnit);
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                IQuantity quantityAngle = new UnitsNet.Angle(0, angleUnit);
                angleAbbreviation = string.Concat(quantityAngle.ToString().Where(char.IsLetter));

                Params.Input[2].Name = "Radius [" + unitAbbreviation + "]";
                Params.Input[2].NickName = "r";
                Params.Input[2].Description = "Distance representing the radius of the circle";
                Params.Input[2].Access = GH_ParamAccess.item;
                Params.Input[2].Optional = false;

                Params.Input[3].Name = "StartAngle [" + angleAbbreviation + "]";
                Params.Input[3].NickName = "s°";
                Params.Input[3].Description = "[Optional] The starting angle (in " + angleAbbreviation + ") of the circle. Positive angle is considered anti-clockwise. Default is 0";
                Params.Input[3].Access = GH_ParamAccess.item;
                Params.Input[3].Optional = true;
            }
            if (_mode == FoldMode.Arc)
            {
                Params.Input[0].Name = "Spaced Rebars";
                Params.Input[0].NickName = "RbS";
                Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Centre";
                Params.Input[1].NickName = "CVx";
                Params.Input[1].Description = "Vertex Point representing the centre of the circle";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;

                IQuantity quantity = new UnitsNet.Length(0, lengthUnit);
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                IQuantity quantityAngle = new UnitsNet.Angle(0, angleUnit);
                angleAbbreviation = string.Concat(quantityAngle.ToString().Where(char.IsLetter));

                Params.Input[2].Name = "Radius [" + unitAbbreviation + "]";
                Params.Input[2].NickName = "r";
                Params.Input[2].Description = "Distance representing the radius of the circle";
                Params.Input[2].Access = GH_ParamAccess.item;
                Params.Input[2].Optional = false;

                Params.Input[3].Name = "StartAngle [" + angleAbbreviation + "]";
                Params.Input[3].NickName = "s°";
                Params.Input[3].Description = "[Optional] The starting angle (in " + angleAbbreviation + ")) of the circle. Positive angle is considered anti-clockwise. Default is 0";
                Params.Input[3].Access = GH_ParamAccess.item;
                Params.Input[3].Optional = true;

                Params.Input[4].Name = "SweepAngle [" + angleAbbreviation + "]";
                Params.Input[4].NickName = "e°";
                Params.Input[4].Description = "The angle (in " + angleAbbreviation + ") sweeped by the arc from its start angle. Positive angle is considered anti-clockwise. Default is π/2";
                Params.Input[4].Access = GH_ParamAccess.item;
                Params.Input[4].Optional = true;
            }
        }
        #endregion
    }
}
