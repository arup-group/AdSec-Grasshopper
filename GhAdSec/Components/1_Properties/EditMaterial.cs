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

namespace GhAdSec.Components
{
    public class EditMaterial : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("87f26bee-c72c-4d88-9b30-492190df2910");
        public EditMaterial()
          : base("Edit Material", "MaterialEdit", "Modify AdSec Material",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = false; } // sets the initial state of the component to hidden

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.EditMaterial;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        #endregion

        #region Input and output

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Mat", "AdSet Material to Edit or get information from", GH_ParamAccess.item);
            pManager.AddGenericParameter("DesignCode", "Code", "[Optional] Overwrite the Material's DesignCode", GH_ParamAccess.item);
            pManager.AddGenericParameter("ULS Comp. Crv", "U_C", "ULS Stress Strain Curve for Compression", GH_ParamAccess.item);
            pManager.AddGenericParameter("ULS Tens. Crv", "U_T", "ULS Stress Strain Curve for Tension", GH_ParamAccess.item);
            pManager.AddGenericParameter("SLS Comp. Crv", "S_C", "SLS Stress Strain Curve for Compression", GH_ParamAccess.item);
            pManager.AddGenericParameter("SLS Tens. Crv", "S_T", "SLS Stress Strain Curve for Tension", GH_ParamAccess.item);
            pManager.AddGenericParameter("Crack Calc Params", "CCP", "[Optional] Overwrite the Material's ConcreteCrackCalculationParameters", GH_ParamAccess.item);
            
            // make all but first input optional
            for (int i = 1; i < pManager.ParamCount; i++)
                pManager[i].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Mat", "Modified AdSec Material", GH_ParamAccess.item);
            pManager.AddGenericParameter("DesignCode", "Code", "DesignCode", GH_ParamAccess.item);
            pManager.AddGenericParameter("ULS Comp. Crv", "U_C", "ULS Stress Strain Curve for Compression", GH_ParamAccess.item);
            pManager.AddGenericParameter("ULS Tens. Crv", "U_T", "ULS Stress Strain Curve for Tension", GH_ParamAccess.item);
            pManager.AddGenericParameter("SLS Comp. Crv", "S_C", "SLS Stress Strain Curve for Compression", GH_ParamAccess.item);
            pManager.AddGenericParameter("SLS Tens. Crv", "S_T", "SLS Stress Strain Curve for Tension", GH_ParamAccess.item);
            pManager.AddGenericParameter("Crack Calc Params", "CCP", "ConcreteCrackCalculationParameters", GH_ParamAccess.item);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // #### get material input and duplicate it ####
            AdSecMaterial input = new AdSecMaterial();
            AdSecMaterial editMat = new AdSecMaterial();
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(0, ref gh_typ))
            {
                if (gh_typ.Value is AdSecMaterialGoo)
                {
                    gh_typ.CastTo(ref input);
                    editMat = input.Duplicate();
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in material input - unable to cast from type " + gh_typ.Value.GetType().Name);
                }
            }

            if (editMat != null)
            {
                // #### get the remaining inputs ####

                // 1 DesignCode
                gh_typ = new GH_ObjectWrapper();
                if (DA.GetData(1, ref gh_typ))
                {
                    AdSecDesignCode designCode = new AdSecDesignCode();
                    if (gh_typ.Value is AdSecDesignCodeGoo)
                    {
                        gh_typ.CastTo(ref designCode);
                        editMat.DesignCode = designCode;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert DC input to a DesignCode" + System.Environment.NewLine + "DesignCode input has been ignored");
                    }
                }
                // create stress strain curves
                Tuple<Curve, List<Point3d>> ulsComp = GhAdSec.Parameters.AdSecStressStrainCurveGoo.CreateFromCode(editMat.Material.Strength.Compression, true);
                Tuple<Curve, List<Point3d>> ulsTens = GhAdSec.Parameters.AdSecStressStrainCurveGoo.CreateFromCode(editMat.Material.Strength.Tension, false);
                Tuple<Curve, List<Point3d>> slsComp = GhAdSec.Parameters.AdSecStressStrainCurveGoo.CreateFromCode(editMat.Material.Serviceability.Compression, true);
                Tuple<Curve, List<Point3d>> slsTens = GhAdSec.Parameters.AdSecStressStrainCurveGoo.CreateFromCode(editMat.Material.Serviceability.Tension, false);

                AdSecStressStrainCurveGoo ulsCompCrv = new AdSecStressStrainCurveGoo(ulsComp.Item1, editMat.Material.Strength.Compression,
                AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, ulsComp.Item2);
                AdSecStressStrainCurveGoo ulsTensCrv = new AdSecStressStrainCurveGoo(ulsTens.Item1, editMat.Material.Strength.Tension,
                    AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, ulsTens.Item2);
                AdSecStressStrainCurveGoo slsCompCrv = new AdSecStressStrainCurveGoo(slsComp.Item1, editMat.Material.Serviceability.Compression,
                    AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, slsComp.Item2);
                AdSecStressStrainCurveGoo slsTensCrv = new AdSecStressStrainCurveGoo(slsTens.Item1, editMat.Material.Serviceability.Tension,
                    AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, slsTens.Item2);

                bool rebuildCurves = false;
                // 2 StressStrain ULS Compression
                gh_typ = new GH_ObjectWrapper();
                if (DA.GetData(2, ref gh_typ))
                {
                    if (gh_typ.Value is AdSecStressStrainCurveGoo)
                    {
                        ulsCompCrv = (AdSecStressStrainCurveGoo)gh_typ.Value;
                        rebuildCurves = true;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert U_C input to a StressStrainCurve");
                        return;
                    }
                }
                // 3 StressStrain ULS Tension
                gh_typ = new GH_ObjectWrapper();
                if (DA.GetData(3, ref gh_typ))
                {
                    if (gh_typ.Value is AdSecStressStrainCurveGoo)
                    {
                        ulsTensCrv = (AdSecStressStrainCurveGoo)gh_typ.Value;
                        rebuildCurves = true;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert U_T input to a StressStrainCurve");
                        return;
                    }
                }
                // 4 StressStrain SLS Compression
                gh_typ = new GH_ObjectWrapper();
                if (DA.GetData(4, ref gh_typ))
                {
                    if (gh_typ.Value is AdSecStressStrainCurveGoo)
                    {
                        slsCompCrv = (AdSecStressStrainCurveGoo)gh_typ.Value;
                        rebuildCurves = true;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert S_C input to a StressStrainCurve");
                        return;
                    }
                }
                // 5 StressStrain SLS Tension
                gh_typ = new GH_ObjectWrapper();
                if (DA.GetData(5, ref gh_typ))
                {
                    if (gh_typ.Value is AdSecStressStrainCurveGoo)
                    {
                        slsTensCrv = (AdSecStressStrainCurveGoo)gh_typ.Value;
                        rebuildCurves = true;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert S_T input to a StressStrainCurve");
                        return;
                    }
                }

                // 6 Cracked params
                gh_typ = new GH_ObjectWrapper();
                IConcreteCrackCalculationParameters concreteCrack = null;
                if (DA.GetData(6, ref gh_typ))
                {
                    if (gh_typ.Value is IConcreteCrackCalculationParameters)
                    {
                        concreteCrack = (IConcreteCrackCalculationParameters)gh_typ.Value;
                        rebuildCurves = true;
                    }
                    else if (gh_typ.Value is AdSecConcreteCrackCalculationParametersGoo)
                    {
                        AdSecConcreteCrackCalculationParametersGoo adsecccp = (AdSecConcreteCrackCalculationParametersGoo)gh_typ.Value;
                        concreteCrack = adsecccp.ConcreteCrackCalculationParameters;
                        rebuildCurves = true;
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert CCP input to a ConcreteCrackCalculationParameters");
                        return;
                    }
                }

                if (rebuildCurves)
                {
                    ITensionCompressionCurve ulsTC = ITensionCompressionCurve.Create(ulsTensCrv.StressStrainCurve, ulsCompCrv.StressStrainCurve);
                    ITensionCompressionCurve slsTC = ITensionCompressionCurve.Create(slsTensCrv.StressStrainCurve, slsCompCrv.StressStrainCurve);
                    switch (editMat.Type)
                    {
                        case AdSecMaterial.AdSecMaterialType.Concrete:
                            
                            if (concreteCrack == null)
                                editMat.Material = IConcrete.Create(ulsTC, slsTC);
                            else
                                editMat.Material = IConcrete.Create(ulsTC, slsTC, concreteCrack);
                            break;

                        case AdSecMaterial.AdSecMaterialType.FRP:
                            editMat.Material = IFrp.Create(ulsTC, slsTC);
                            break;

                        case AdSecMaterial.AdSecMaterialType.Rebar:
                            editMat.Material = IReinforcement.Create(ulsTC, slsTC);
                            break;

                        case AdSecMaterial.AdSecMaterialType.Tendon:
                            editMat.Material = IReinforcement.Create(ulsTC, slsTC);
                            break;

                        case AdSecMaterial.AdSecMaterialType.Steel:
                            editMat.Material = ISteel.Create(ulsTC, slsTC);
                            break;
                    }
                }
                DA.SetData(0, new AdSecMaterialGoo(editMat));
                DA.SetData(1, new AdSecDesignCodeGoo(editMat.DesignCode));
                DA.SetData(2, ulsCompCrv);
                DA.SetData(3, ulsTensCrv);
                DA.SetData(4, slsCompCrv);
                DA.SetData(5, slsTensCrv);
                if (editMat.Type == AdSecMaterial.AdSecMaterialType.Concrete)
                {
                    IConcrete concrete = (IConcrete)editMat.Material;
                    DA.SetData(6, new AdSecConcreteCrackCalculationParametersGoo(concrete.ConcreteCrackCalculationParameters));
                }
            }
        }
    }
}
