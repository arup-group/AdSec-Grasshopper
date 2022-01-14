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
using AdSecGH.Parameters;
using UnitsNet.GH;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement.Groups;
using UnitsNet;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.Reinforcement;

namespace AdSecGH.Components
{
    public class CreateReinforcementGroup : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        public CreateReinforcementGroup()
            : base("Create Reinforcement Group", "Reinforcement Group", "Create a Template Reinforcement Group for an AdSec Section",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        { this.Hidden = true; }
        public override Guid ComponentGuid => new Guid("9876f456-de99-4834-8d7f-4019cc0c70ba");
        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        //
        protected override string HtmlHelp_Source()
        {
            string help = "GOTO:https://arup-group.github.io/oasys-combined/adsec-api/api/Oasys.AdSec.Reinforcement.Groups.ITemplateGroup.Face.html";
            return help;
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.RebarGroup;
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
                IQuantity quantity = new Length(0, lengthUnit);
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                selecteditems.Add(lengthUnit.ToString());

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
                ToggleInput();
            }
            else
            {
                lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[i]);
            }
            
        }
        private void UpdateUIFromSelectedItems()
        {
            _mode = (FoldMode)Enum.Parse(typeof(FoldMode), selecteditems[0]);
            lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[1]);
            CreateAttributes();
            ToggleInput();
        }
        #endregion

        #region Input and output
        List<List<string>> dropdownitems;
        List<string> selecteditems;
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Group Type",
            "Measure",
        });
        private UnitsNet.Units.LengthUnit lengthUnit = Units.LengthUnit;
        string unitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Top Rebars", "TRs", "Top Face AdSec Rebars Spaced in a Layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Left Side Rebars", "LRs", "Left Side Face AdSec Rebars Spaced in a Layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Right Side Rebars", "RRs", "Right Side Face AdSec Rebars Spaced in a Layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Bottom Rebars", "BRs", "Bottom Face AdSec Rebars Spaced in a Layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Cover [" + unitAbbreviation + "]", "Cov", "The reinforcement-free zone around the faces of a profile.", GH_ParamAccess.item);
            _mode = FoldMode.Template;
            // make all but last input optional
            for (int i = 0; i < pManager.ParamCount - 1; i++)
                pManager[i].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Layout", "RbG", "Rebar Groups for AdSec Section", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<AdSecRebarGroupGoo> groups = new List<AdSecRebarGroupGoo>();

            // cover
            ICover cover = ICover.Create(GetInput.Length(this, DA, this.Params.Input.Count - 1, lengthUnit));

            switch (_mode)
            {
                case FoldMode.Template:
                    // check for enough input parameters
                    if (this.Params.Input[0].SourceCount == 0 && this.Params.Input[1].SourceCount == 0 
                        && this.Params.Input[2].SourceCount == 0 && this.Params.Input[3].SourceCount == 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameters " + this.Params.Input[0].NickName + ", " +
                            this.Params.Input[1].NickName + ", " + this.Params.Input[2].NickName + ", and " + this.Params.Input[3].NickName + " failed to collect data!");
                        return;
                    }
                    // top
                    if (this.Params.Input[0].SourceCount != 0)
                    {
                        ITemplateGroup grp = ITemplateGroup.Create(ITemplateGroup.Face.Top);
                        grp.Layers.Add(GetInput.ILayer(this, DA, 0));
                        groups.Add(new AdSecRebarGroupGoo(grp));
                        groups.Last().Cover = cover;
                    }
                    // left
                    if (this.Params.Input[1].SourceCount != 0)
                    {
                        ITemplateGroup grp = ITemplateGroup.Create(ITemplateGroup.Face.LeftSide);
                        grp.Layers.Add(GetInput.ILayer(this, DA, 1));
                        groups.Add(new AdSecRebarGroupGoo(grp));
                        groups.Last().Cover = cover;
                    }
                    // right
                    if (this.Params.Input[2].SourceCount != 0)
                    {
                        ITemplateGroup grp = ITemplateGroup.Create(ITemplateGroup.Face.RightSide);
                        grp.Layers.Add(GetInput.ILayer(this, DA, 2));
                        groups.Add(new AdSecRebarGroupGoo(grp));
                        groups.Last().Cover = cover;
                    }
                    // bottom
                    if (this.Params.Input[3].SourceCount != 0)
                    {
                        ITemplateGroup grp = ITemplateGroup.Create(ITemplateGroup.Face.Bottom);
                        grp.Layers.Add(GetInput.ILayer(this, DA, 3));
                        groups.Add(new AdSecRebarGroupGoo(grp));
                        groups.Last().Cover = cover;
                    }
                    
                    break;

                case FoldMode.Perimeter:
                    // check for enough input parameters
                    if (this.Params.Input[0].SourceCount == 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + this.Params.Input[0].NickName + " failed to collect data!");
                        return;
                    }
                    // top
                    if (this.Params.Input[0].SourceCount != 0)
                    {
                        IPerimeterGroup grp = IPerimeterGroup.Create();
                        grp.Layers.Add(GetInput.ILayer(this, DA, 0));
                        groups.Add(new AdSecRebarGroupGoo(grp));
                        groups.Last().Cover = cover;
                    }
                    break;

                case FoldMode.Link:
                    // check for enough input parameters
                    if (this.Params.Input[0].SourceCount == 0)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + this.Params.Input[0].NickName + " failed to collect data!");
                        return;
                    }
                    // top
                    if (this.Params.Input[0].SourceCount != 0)
                    {
                        ILinkGroup grp = ILinkGroup.Create(GetInput.IBarBundle(this, DA, 0));
                        groups.Add(new AdSecRebarGroupGoo(grp));
                        groups.Last().Cover = cover;
                    }
                    break;
            }

            // set output
            DA.SetDataList(0, groups);
        }

        #region menu override
        
        private bool first = true;
        private enum FoldMode
        {
            Template,
            Perimeter,
            Link
        }

        private FoldMode _mode = FoldMode.Template;

        private void ToggleInput()
        {
            // remove cover temporarily
            IGH_Param param_Cover = Params.Input[Params.Input.Count - 1];
            Params.UnregisterInputParameter(Params.Input[Params.Input.Count - 1], false);
            
            // remove any additional input parameters
            while (Params.Input.Count > 1)
                Params.UnregisterInputParameter(Params.Input[1]);
            
            if (_mode == FoldMode.Template)
            {
                // register 3 generic
                Params.RegisterInputParam(new Param_GenericObject());
                Params.RegisterInputParam(new Param_GenericObject());
                Params.RegisterInputParam(new Param_GenericObject());
            }
            // add cover back
            Params.RegisterInputParam(param_Cover);

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            ExpireSolution(true);
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }
        #endregion

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
            if (_mode == FoldMode.Template)
            {
                Params.Input[0].Name = "Top Rebars";
                Params.Input[0].NickName = "TRs";
                Params.Input[0].Description = "Top Face AdSec Rebars Spaced in a Layer";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = true;

                Params.Input[1].Name = "Left Side Rebars";
                Params.Input[1].NickName = "LRs";
                Params.Input[1].Description = "Left Side Face AdSec Rebars Spaced in a Layer";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = true;

                Params.Input[2].Name = "Right Side Rebars";
                Params.Input[2].NickName = "RRs";
                Params.Input[2].Description = "Right Side Face AdSec Rebars Spaced in a Layer";
                Params.Input[2].Access = GH_ParamAccess.item;
                Params.Input[2].Optional = true;

                Params.Input[3].Name = "Bottom Rebars";
                Params.Input[3].NickName = "BRs";
                Params.Input[3].Description = "Bottom Face AdSec Rebars Spaced in a Layer";
                Params.Input[3].Access = GH_ParamAccess.item;
                Params.Input[3].Optional = true;
            }
            if (_mode == FoldMode.Perimeter)
            {
                Params.Input[0].Name = "Spaced Rebars";
                Params.Input[0].NickName = "RbS";
                Params.Input[0].Description = "AdSec Rebars Spaced in a Layer";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;
            }
            if (_mode == FoldMode.Link)
            {
                Params.Input[0].Name = "Rebar";
                Params.Input[0].NickName = "Rb";
                Params.Input[0].Description = "AdSec Rebar (single or bundle)";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;
            }

            IQuantity quantity = new Length(0, lengthUnit);
            unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));

            Params.Input[Params.Input.Count - 1].Name = "Cover [" + unitAbbreviation + "]";
            Params.Input[Params.Input.Count - 1].NickName = "Cov";
            Params.Input[Params.Input.Count - 1].Description = "AdSec Rebars Spaced in a Layer";
            Params.Input[Params.Input.Count - 1].Access = GH_ParamAccess.item;
            Params.Input[Params.Input.Count - 1].Optional = false;
        }
        #endregion
    }
}
