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
using AdSecGH.Parameters;
using Rhino.Geometry;
using System.Collections.Generic;
using Grasshopper.Kernel.Parameters;

namespace AdSecGH.Components
{
    public class CustomMaterial : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("29f87bee-c84c-5d11-9b30-492190df2910");
        public CustomMaterial()
          : base("Custom Material", "CustomMaterial", "Create a custom AdSec Material",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = true; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCustomMaterial;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                dropdownitems = new List<List<string>>();
                selecteditems = new List<string>();

                // populate lists
                dropdownitems.Add(Enum.GetNames(typeof(AdSecMaterial.AdSecMaterialType)).ToList());
                selecteditems.Add(AdSecMaterial.AdSecMaterialType.Concrete.ToString());

                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }

        public void SetSelected(int i, int j)
        {
            // change selected item
            selecteditems[i] = dropdownitems[i][j];
            
            // cast selection to material type enum
            Enum.TryParse(selecteditems[0], out type);

            // set bool if selection is concrete
            if (selecteditems[i] == AdSecMaterial.AdSecMaterialType.Concrete.ToString())
                isConcrete = true;
            else
                isConcrete = false;

            // update input params
            ChangeMode();
            ExpireSolution(true);
            Params.OnParametersChanged();
            this.OnDisplayExpired(true);
        }
        private void UpdateUIFromSelectedItems()
        {
            CreateAttributes();
            ChangeMode();
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
            "Material Type",
        });
        private bool first = true;
        private AdSecMaterial.AdSecMaterialType type = AdSecMaterial.AdSecMaterialType.Concrete;
        private bool isConcrete = true;
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("DesignCode", "Code", "[Optional] Set the Material's DesignCode", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("ULS Comp. Crv", "U_C", "ULS Stress Strain Curve for Compression", GH_ParamAccess.item);
            pManager.AddGenericParameter("ULS Tens. Crv", "U_T", "ULS Stress Strain Curve for Tension", GH_ParamAccess.item);
            pManager.AddGenericParameter("SLS Comp. Crv", "S_C", "SLS Stress Strain Curve for Compression", GH_ParamAccess.item);
            pManager.AddGenericParameter("SLS Tens. Crv", "S_T", "SLS Stress Strain Curve for Tension", GH_ParamAccess.item);
            pManager.AddGenericParameter("Crack Calc Params", "CCP", "[Optional] Material's Crack Calculation Parameters", GH_ParamAccess.item);
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Mat", "Custom AdSec Material", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 0 DesignCode
            AdSecDesignCode designCode = GetInput.AdSecDesignCode(this, DA, 0);

            // 1 StressStrain ULS Compression
            AdSecStressStrainCurveGoo ulsCompCrv = GetInput.StressStrainCurveGoo(this, DA, 1, true);
            

            // 2 StressStrain ULS Tension
            AdSecStressStrainCurveGoo ulsTensCrv = GetInput.StressStrainCurveGoo(this, DA, 2, false);

            // 3 StressStrain SLS Compression
            AdSecStressStrainCurveGoo slsCompCrv = GetInput.StressStrainCurveGoo(this, DA, 3, true);

            // 4 StressStrain SLS Tension
            AdSecStressStrainCurveGoo slsTensCrv = GetInput.StressStrainCurveGoo(this, DA, 4, false);
            
            // 5 Cracked params
            IConcreteCrackCalculationParameters concreteCrack = null;
            if (isConcrete)
            {
                concreteCrack = GetInput.ConcreteCrackCalculationParameters(this, DA, 5);
            }

            // create new empty material
            AdSecMaterial material = new AdSecMaterial();
            if (designCode != null)
                material.DesignCode = designCode;
            
            // set material type from dropdown input
            material.Type = type;

            // create tension-compression curves from input
            ITensionCompressionCurve ulsTC = ITensionCompressionCurve.Create(ulsTensCrv.StressStrainCurve, ulsCompCrv.StressStrainCurve);
            ITensionCompressionCurve slsTC = ITensionCompressionCurve.Create(slsTensCrv.StressStrainCurve, slsCompCrv.StressStrainCurve);
            
            // create api material based on type
            switch (type)
            {
                case AdSecMaterial.AdSecMaterialType.Concrete:
                    if (concreteCrack == null)
                        material.Material = IConcrete.Create(ulsTC, slsTC);
                    else
                        material.Material = IConcrete.Create(ulsTC, slsTC, concreteCrack);
                    break;

                case AdSecMaterial.AdSecMaterialType.FRP:
                    material.Material = IFrp.Create(ulsTC, slsTC);
                    break;

                case AdSecMaterial.AdSecMaterialType.Rebar:
                    material.Material = IReinforcement.Create(ulsTC, slsTC);
                    break;

                case AdSecMaterial.AdSecMaterialType.Tendon:
                    material.Material = IReinforcement.Create(ulsTC, slsTC);
                    break;

                case AdSecMaterial.AdSecMaterialType.Steel:
                    material.Material = ISteel.Create(ulsTC, slsTC);
                    break;
            }

            // set output
            DA.SetData(0, new AdSecMaterialGoo(material));
        }

        private void ChangeMode()
        {
            if (isConcrete)
                if (Params.Input.Count == 6) { return; }
            
            if (!isConcrete)
                if (Params.Input.Count == 5) { return; }

            RecordUndoEvent("Changed dropdown");

            // change number of input parameters
            if (isConcrete)
                Params.RegisterInputParam(new Param_GenericObject());
            else
                Params.UnregisterInputParameter(Params.Input[5], true);

            (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
        }

        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);
            writer.SetBoolean("isConcrete", isConcrete);
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);
            isConcrete = reader.GetBoolean("isConcrete");
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
            if (isConcrete)
            {
                Params.Input[5].Name = "Yield PointCrack Calc Params";
                Params.Input[5].NickName = "CCP";
                Params.Input[5].Description = "[Optional] Material's Crack Calculation Parameters";
                Params.Input[5].Access = GH_ParamAccess.item;
                Params.Input[5].Optional = true;
            }
        }
        #endregion
    }
}
