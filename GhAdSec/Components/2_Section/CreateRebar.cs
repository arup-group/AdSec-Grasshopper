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
using Oasys.AdSec.Reinforcement;
using UnitsNet;

namespace GhAdSec.Components
{
    public class CreateRebar : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        public CreateRebar()
            : base("Create Rebar", "Rebar", "Create Rebar (single or bundle) for AdSec Section",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat2())
        { this.Hidden = true; }
        public override Guid ComponentGuid => new Guid("024d241a-b6cc-4134-9f5c-ac9a6dcb2c4b");
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        //protected override System.Drawing.Bitmap Icon => GhSA.Properties.Resources.BeamLoad;
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
            this.OnDisplayExpired(true);
        }
        #endregion

        #region Input and output
        List<List<string>> dropdownitems;
        List<string> selecteditems;
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Rebar Type",
            "Measure"
        });
        private UnitsNet.Units.LengthUnit lengthUnit = GhAdSec.DocumentUnits.LengthUnit;
        string unitAbbreviation;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Mat", "AdSec Reinforcement Material", GH_ParamAccess.item);
            pManager.AddGenericParameter("Diameter [" + unitAbbreviation + "]", "Ø", "Diameter", GH_ParamAccess.item);
            _mode = FoldMode.Single;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Rebar", "Rb", "Rebar (single or bundle) for AdSec Reinforcement", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 0 material input
            AdSecMaterial material = new AdSecMaterial();
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(0, ref gh_typ))
            {
                if (gh_typ.Value is AdSecMaterialGoo)
                {
                    gh_typ.CastTo(ref material);
                    if (!(material.Type == AdSecMaterial.AdSecMaterialType.Rebar | material.Type == AdSecMaterial.AdSecMaterialType.Tendon))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 0: Wrong material type supplied"
                            + System.Environment.NewLine + "Material type is " + material.Type.ToString() + " but must be Reinforcement");
                        return;
                    }
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in material input - unable to cast from type " + gh_typ.Value.GetType().Name);
                }
            }

            GH_UnitNumber diameter = null;
            gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(1, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    diameter = (GH_UnitNumber)gh_typ.Value;
                    // check that unit is of right type
                    if (!diameter.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.LengthUnit)))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in input index 1: Wrong unit type supplied" 
                            + System.Environment.NewLine + "Unit type is " + diameter.Value.QuantityInfo.Name + " but must be Length");
                        return;
                    }
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    diameter = new GH_UnitNumber(new UnitsNet.Length(val, lengthUnit));
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input index 1");
                    return;
                }
            }

            if (_mode == FoldMode.Single)
            {
                AdSecRebarBundleGoo rebar = new AdSecRebarBundleGoo(IBarBundle.Create((Oasys.AdSec.Materials.IReinforcement)material.Material, (UnitsNet.Length)diameter.Value));
                DA.SetData(0, rebar);
            }
            else
            {
                int count = 1;
                if (DA.GetData(2, ref count))
                {
                    //if (count > 4 | count < 1)
                    //{
                    //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of bars in bundle must be between 1 and 4");
                    //    return;
                    //}

                    AdSecRebarBundleGoo rebar = new AdSecRebarBundleGoo(IBarBundle.Create((Oasys.AdSec.Materials.IReinforcement)material.Material, (UnitsNet.Length)diameter.Value, count));
                    DA.SetData(0, rebar);
                }
            }
        }

        #region menu override
        
        private bool first = true;
        private enum FoldMode
        {
            Single,
            Bundle
        }

        private FoldMode _mode = FoldMode.Single;

        private void ToggleInput()
        {
            RecordUndoEvent("Changed dropdown");

            switch (_mode)
            {
                case FoldMode.Single:
                    // remove any additional input parameters
                    while (Params.Input.Count > 2)
                        Params.UnregisterInputParameter(Params.Input[2], true);
                    break;

                case FoldMode.Bundle:
                    // add input parameter
                    while (Params.Input.Count != 3)
                        Params.RegisterInputParam(new Param_Integer());
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
            writer.SetString("enum", _mode.ToString());
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            GhAdSec.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
            _mode = (FoldMode)Enum.Parse(typeof(FoldMode), reader.GetString("mode"));
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
            Params.Input[1].Name = "Diameter [" + unitAbbreviation + "]";

            if (_mode == FoldMode.Bundle)
            {
                Params.Input[2].Name = "Count";
                Params.Input[2].NickName = "N";
                Params.Input[2].Description = "Count per bundle (1, 2, 3 or 4)";
                Params.Input[2].Access = GH_ParamAccess.item;
                Params.Input[2].Optional = false;
            }
        }
        #endregion
    }
}
