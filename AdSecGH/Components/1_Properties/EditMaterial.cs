using System;
using System.Drawing;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;

using OasysGH;
using OasysGH.Components;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class EditMaterial : GH_OasysComponent {

    public EditMaterial() : base("Edit Material", "MaterialEdit", "Modify AdSec Material", CategoryName.Name(),
      SubCategoryName.Cat1()) {
      Hidden = false; // sets the initial state of the component to hiddens
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("87f26bee-c72c-4d88-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.EditMaterial;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Material", "Mat", "AdSet Material to Edit or get information from",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("DesignCode", "Code", "[Optional] Overwrite the Material's DesignCode",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("ULS Comp. Crv", "U_C", "ULS Stress Strain Curve for Compression",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("ULS Tens. Crv", "U_T", "ULS Stress Strain Curve for Tension", GH_ParamAccess.item);
      pManager.AddGenericParameter("SLS Comp. Crv", "S_C", "SLS Stress Strain Curve for Compression",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("SLS Tens. Crv", "S_T", "SLS Stress Strain Curve for Tension", GH_ParamAccess.item);
      pManager.AddGenericParameter("Crack Calc Params", "CCP",
        "[Optional] Overwrite the Material's ConcreteCrackCalculationParameters", GH_ParamAccess.item);
      // make all but first input optional
      for (int i = 1; i < pManager.ParamCount; i++) {
        pManager[i].Optional = true;
      }
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Material", "Mat", "Modified AdSec Material", GH_ParamAccess.item);
      pManager.AddGenericParameter("DesignCode", "Code", "DesignCode", GH_ParamAccess.item);
      pManager.AddGenericParameter("ULS Comp. Crv", "U_C", "ULS Stress Strain Curve for Compression",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("ULS Tens. Crv", "U_T", "ULS Stress Strain Curve for Tension", GH_ParamAccess.item);
      pManager.AddGenericParameter("SLS Comp. Crv", "S_C", "SLS Stress Strain Curve for Compression",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("SLS Tens. Crv", "S_T", "SLS Stress Strain Curve for Tension", GH_ParamAccess.item);
      pManager.AddGenericParameter("Crack Calc Params", "CCP", "ConcreteCrackCalculationParameters",
        GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // #### get material input and duplicate it ####
      var editMat = this.GetAdSecMaterial(DA, 0);

      if (editMat != null) {
        // #### get the remaining inputs ####

        // 1 DesignCode
        if (Params.Input[1].SourceCount > 0) {
          editMat.DesignCode = this.GetAdSecDesignCode(DA, 1);
        }

        bool rebuildCurves = false;

        // 2 StressStrain ULS Compression
        AdSecStressStrainCurveGoo ulsCompCrv;
        if (Params.Input[2].SourceCount > 0) {
          // use input
          ulsCompCrv = this.GetStressStrainCurveGoo(DA, 2, true);
          rebuildCurves = true;
        } else {
          // rebuild from existing material
          var ulsComp = AdSecStressStrainCurveGoo.CreateFromCode(editMat.Material.Strength.Compression, true);
          ulsCompCrv = new AdSecStressStrainCurveGoo(ulsComp.Item1, editMat.Material.Strength.Compression,
            AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, ulsComp.Item2);
        }

        // 3 StressStrain ULS Tension
        AdSecStressStrainCurveGoo ulsTensCrv;
        if (Params.Input[3].SourceCount > 0) {
          // use input
          ulsTensCrv = this.GetStressStrainCurveGoo(DA, 3, false);
          rebuildCurves = true;
        } else {
          // rebuild from existing material
          var ulsTens = AdSecStressStrainCurveGoo.CreateFromCode(editMat.Material.Strength.Tension, false);
          ulsTensCrv = new AdSecStressStrainCurveGoo(ulsTens.Item1, editMat.Material.Strength.Tension,
            AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, ulsTens.Item2);
        }

        // 4 StressStrain SLS Compression
        AdSecStressStrainCurveGoo slsCompCrv;
        if (Params.Input[4].SourceCount > 0) {
          // use input
          slsCompCrv = this.GetStressStrainCurveGoo(DA, 4, true);
          rebuildCurves = true;
        } else {
          // rebuild from existing material
          var slsComp = AdSecStressStrainCurveGoo.CreateFromCode(editMat.Material.Serviceability.Compression, true);
          slsCompCrv = new AdSecStressStrainCurveGoo(slsComp.Item1, editMat.Material.Serviceability.Compression,
            AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, slsComp.Item2);
        }

        // 5 StressStrain SLS Tension
        AdSecStressStrainCurveGoo slsTensCrv;
        if (Params.Input[5].SourceCount > 0) {
          // use input
          slsTensCrv = this.GetStressStrainCurveGoo(DA, 5, false);
          rebuildCurves = true;
        } else {
          // rebuild from existing material
          var slsTens = AdSecStressStrainCurveGoo.CreateFromCode(editMat.Material.Serviceability.Tension, false);
          slsTensCrv = new AdSecStressStrainCurveGoo(slsTens.Item1, editMat.Material.Serviceability.Tension,
            AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, slsTens.Item2);
        }

        // 6 Cracked params
        IConcreteCrackCalculationParameters concreteCrack = null;
        if (Params.Input[6].SourceCount > 0) {
          // use input
          concreteCrack = AdSecInput.ConcreteCrackCalculationParameters(this, DA, 6);
          rebuildCurves = true;
        }

        if (rebuildCurves) {
          if (ulsTensCrv.StressStrainCurve.FailureStrain.Value == 0) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
              "ULS Stress Strain Curve for Tension has zero failure strain." + Environment.NewLine
              + "The curve has been changed to a simulate a material with no tension capacity (ε = 1, σ = 0)");
            IStressStrainCurve crv = ILinearStressStrainCurve.Create(
              IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
            var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear,
              false);
            ulsTensCrv = new AdSecStressStrainCurveGoo(tuple.Item1, crv,
              AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, tuple.Item2);
          }

          if (ulsCompCrv.StressStrainCurve.FailureStrain.Value == 0) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
              "ULS Stress Strain Curve for Compression has zero failure strain.");
            return;
          }

          var ulsTC = ITensionCompressionCurve.Create(ulsTensCrv.StressStrainCurve, ulsCompCrv.StressStrainCurve);
          var slsTC = ITensionCompressionCurve.Create(slsTensCrv.StressStrainCurve, slsCompCrv.StressStrainCurve);
          switch (editMat.Type) {
            case AdSecMaterial.AdSecMaterialType.Concrete:

              if (concreteCrack == null) {
                editMat.Material = IConcrete.Create(ulsTC, slsTC);
              } else {
                editMat.Material = IConcrete.Create(ulsTC, slsTC, concreteCrack);
              }

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
        if (editMat.Type == AdSecMaterial.AdSecMaterialType.Concrete) {
          var concrete = (IConcrete)editMat.Material;
          DA.SetData(6, new AdSecConcreteCrackCalculationParametersGoo(concrete.ConcreteCrackCalculationParameters));
        }
      }
    }
  }
}
