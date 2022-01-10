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
using UnitsNet;

namespace AdSecGH.Components
{
    public class CreateProfileWeb : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        public CreateProfileWeb()
            : base("Create Web", "Web", "Create a Web for AdSec Profile",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        { this.Hidden = true; }
        public override Guid ComponentGuid => new Guid("0f9a9223-e745-44b9-add2-8b2e5950e86a");
        public override GH_Exposure Exposure => GH_Exposure.quarternary;

        protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.CreateWeb;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (Grasshopper.Instances.DocumentEditor == null) { base.CreateAttributes(); return; } // skip this class during GH loading

            if (first)
            {
                List<string> list = Enum.GetNames(typeof(FoldMode)).ToList();
                dropdownitems = new List<List<string>>();
                dropdownitems.Add(list);

                selecteditems = new List<string>();
                selecteditems.Add(dropdownitems[0][0]);

                // length
                //dropdownitems.Add(Enum.GetNames(typeof(UnitsNet.Units.LengthUnit)).ToList());
                dropdownitems.Add(Units.FilteredLengthUnits);
                selecteditems.Add(lengthUnit.ToString());

                IQuantity quantity = new UnitsNet.Length(0, lengthUnit);
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                
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
            }
            else
            {
                lengthUnit = (UnitsNet.Units.LengthUnit)Enum.Parse(typeof(UnitsNet.Units.LengthUnit), selecteditems[i]);
            }
            ToggleInput();
        }
        private void UpdateUIFromSelectedItems()
        {
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
            "Web Type",
            "Measure"
        });
        private UnitsNet.Units.LengthUnit lengthUnit = Units.LengthUnit;
        string unitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Thickness [" + unitAbbreviation + "]", "t", "Web thickness", GH_ParamAccess.item);
            _mode = FoldMode.Constant;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("WebProfile", "Web", "Web Profile for AdSec Profile", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            switch (_mode)
            {
                case FoldMode.Constant:
                    
                    AdSecProfileWebGoo webConst = new AdSecProfileWebGoo(
                    IWebConstant.Create(
                        GetInput.Length(this, DA, 0, lengthUnit)));

                    DA.SetData(0, webConst);
                    break;

                case FoldMode.Tapered:
                    AdSecProfileWebGoo webTaper = new AdSecProfileWebGoo(
                    IWebTapered.Create(
                        GetInput.Length(this, DA, 0, lengthUnit),
                        GetInput.Length(this, DA, 1, lengthUnit)));

                    DA.SetData(0, webTaper);
                    break;
            }
        }

        #region menu override
        
        private bool first = true;
        private enum FoldMode
        {
            Constant,
            Tapered
        }

        private FoldMode _mode = FoldMode.Constant;

        private void ToggleInput()
        {
            switch (_mode)
            {
                case FoldMode.Constant:
                    // remove any additional input parameters
                    while (Params.Input.Count > 1)
                        Params.UnregisterInputParameter(Params.Input[1], true);
                    break;

                case FoldMode.Tapered:
                    // add input parameter
                    while (Params.Input.Count != 2)
                        Params.RegisterInputParam(new Param_GenericObject());
                    break;
            }

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
            Params.OnParametersChanged();
            ExpireSolution(true);
            this.OnDisplayExpired(true);
        }
        #endregion

        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            AdSecGH.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
            writer.SetString("mode", _mode.ToString());
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (Grasshopper.Instances.DocumentEditor == null) { return base.Read(reader); } // skip this class during GH loading

            AdSecGH.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
            _mode = (FoldMode)Enum.Parse(typeof(FoldMode), reader.GetString("mode"));
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
            IQuantity quantity = new UnitsNet.Length(0, lengthUnit);
            unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));

            if (_mode == FoldMode.Constant)
            {
                Params.Input[0].Name = "Thickness [" + unitAbbreviation + "]";
                Params.Input[0].NickName = "t";
                Params.Input[0].Description = "Web thickness";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

            }
            if (_mode == FoldMode.Tapered)
            {
                Params.Input[0].Name = "Top Thickness [" + unitAbbreviation + "]";
                Params.Input[0].NickName = "Tt";
                Params.Input[0].Description = "Web thickness at the top";
                Params.Input[0].Access = GH_ParamAccess.item;
                Params.Input[0].Optional = false;

                Params.Input[1].Name = "Bottom Thickness [" + unitAbbreviation + "]";
                Params.Input[1].NickName = "Bt";
                Params.Input[1].Description = "Web thickness at the bottom";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;
            }
        }
        #endregion
    }
}
