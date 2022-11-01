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
using GH;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Layers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
    public class CreateRebarSpacing : GH_OasysComponent, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        public CreateRebarSpacing()
            : base("Create Rebar Spacing", "Spacing", "Create Rebar spacing (by Count or Pitch) for an AdSec Section",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat3())
        { this.Hidden = true; }
        public override Guid ComponentGuid => new Guid("846d546a-4284-4d69-906b-0e6985d7ddd3");
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.RebarSpacing;
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

                // length
                //dropdownitems.Add(Enum.GetNames(typeof(Units.LengthUnit)).ToList());
                dropdownitems.Add(Units.FilteredLengthUnits);
                selecteditems.Add(lengthUnit.ToString());

                IQuantity quantity = new Length(0, lengthUnit);
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
                if (_mode == FoldMode.Count)
                {
                    // remove the second dropdown (length)
                    while (dropdownitems.Count > 1)
                        dropdownitems.RemoveAt(dropdownitems.Count - 1);
                    while (selecteditems.Count > 1)
                        selecteditems.RemoveAt(selecteditems.Count - 1);
                }
                else
                {
                    // add second dropdown (length)
                    if (dropdownitems.Count != 2)
                    {
                        dropdownitems.Add(Units.FilteredLengthUnits);
                        selecteditems.Add(lengthUnit.ToString());
                    }
                }

                ToggleInput();
            }
            else
            {
                lengthUnit = (LengthUnit)Enum.Parse(typeof(LengthUnit), selecteditems[i]);
                
            }
        }
        private void UpdateUIFromSelectedItems()
        {
            _mode = (FoldMode)Enum.Parse(typeof(FoldMode), selecteditems[0]);
            if (_mode == FoldMode.Distance)
                lengthUnit = (LengthUnit)Enum.Parse(typeof(LengthUnit), selecteditems[1]);
            CreateAttributes();
            ToggleInput();
        }
        #endregion

        #region Input and output
        List<List<string>> dropdownitems;
        List<string> selecteditems;
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Spacing method",
            "Measure"
        });
        private LengthUnit lengthUnit = Units.LengthUnit;
        string unitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rebar", "Rb", "AdSec Rebar (single or bundle)", GH_ParamAccess.item);
            pManager.AddGenericParameter("Spacing [" + unitAbbreviation + "]", "S", "Number of bars is calculated based on the available length and the given bar pitch. " +
                    "The bar pitch is re-calculated to place the bars at equal spacing, with a maximum final pitch of the given value. " +
                    "Example: If the available length for the bars is 1000mm and the given bar pitch is 300mm, then the number of " +
                    "spacings that can fit in the available length is calculated as 1000 / 300 i.e. 3.333. The number of spacings is" +
                    " rounded up (3.333 rounds up to 4) and the bar pitch re-calculated (1000mm / 4), resulting in a final pitch of 250mm.", GH_ParamAccess.item);
            _mode = FoldMode.Distance;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Spaced Rebars", "RbS", "Rebars Spaced in a Layer for AdSec Reinforcement", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 0 rebar input
            AdSecRebarBundleGoo rebar = GetInput.AdSecRebarBundleGoo(this, DA, 0);

            switch (_mode)
            {
                case FoldMode.Distance:
                    AdSecRebarLayerGoo bundleD =
                    new AdSecRebarLayerGoo(
                        ILayerByBarPitch.Create(
                            rebar.Value,
                            GetInput.GetLength(this, DA, 1, lengthUnit)));
                    DA.SetData(0, bundleD);

                    break;

                case FoldMode.Count:
                    int count = 1;
                    DA.GetData(1, ref count);

                    AdSecRebarLayerGoo bundleC =
                        new AdSecRebarLayerGoo(
                            ILayerByBarCount.Create(
                                count,
                                rebar.Value));
                    DA.SetData(0, bundleC);

                    break;
            }
        }

        #region menu override
        
        private bool first = true;
        private enum FoldMode
        {
            Distance,
            Count
        }

        private FoldMode _mode = FoldMode.Distance;

        private void ToggleInput()
        {
            switch (_mode)
            {
                case FoldMode.Distance:
                    // remove any additional input parameters
                    while (Params.Input.Count > 1)
                        Params.UnregisterInputParameter(Params.Input[1], true);

                    Params.RegisterInputParam(new Param_GenericObject());

                    break;

                case FoldMode.Count:
                    // add input parameter
                    while (Params.Input.Count > 1)
                        Params.UnregisterInputParameter(Params.Input[1], true);

                    Params.RegisterInputParam(new Param_Integer());

                    break;
            }
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
            writer.SetString("mode", _mode.ToString());
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
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
            if (_mode == FoldMode.Distance)
            {
                IQuantity quantity = new Length(0, lengthUnit);
                unitAbbreviation = string.Concat(quantity.ToString().Where(char.IsLetter));
                Params.Input[1].Name = "Spacing [" + unitAbbreviation + "]";
                Params.Input[1].NickName = "S";
                Params.Input[1].Description = "Number of bars is calculated based on the available length and the given bar pitch. " +
                    "The bar pitch is re-calculated to place the bars at equal spacing, with a maximum final pitch of the given value. " +
                    "Example: If the available length for the bars is 1000mm and the given bar pitch is 300mm, then the number of " +
                    "spacings that can fit in the available length is calculated as 1000 / 300 i.e. 3.333. The number of spacings is" +
                    " rounded up (3.333 rounds up to 4) and the bar pitch re-calculated (1000mm / 4), resulting in a final pitch of 250mm.";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;
            }
            if (_mode == FoldMode.Count)
            {
                Params.Input[1].Name = "Count";
                Params.Input[1].NickName = "N";
                Params.Input[1].Description = "The number of bundles or single bars. The bundles or single bars are spaced out evenly over the available space.";
                Params.Input[1].Access = GH_ParamAccess.item;
                Params.Input[1].Optional = false;
            }
        }
        #endregion
    }
}
