using System;
using System.Drawing;

using AdSecCore;
using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using GH_IO.Serialization;

using Grasshopper.Kernel;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.GH.Helpers;

using OasysGH;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateCustomMaterial : DropdownAdapter<CreateCustomMaterialFunction> {
    private bool isConcrete = true;
    private MaterialType _type = MaterialType.Concrete;

    public override Guid ComponentGuid => new Guid("29f87bee-c84c-5d11-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateCustomMaterial;

    public override bool Read(GH_IReader reader) {
      isConcrete = reader.GetBoolean("isConcrete");
      return base.Read(reader);
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      Enum.TryParse(_selectedItems[0], out _type);

      isConcrete = _selectedItems[i] == MaterialType.Concrete.ToString();

      BusinessComponent.SetMaterialType(_type);
      // RecordUndoEvent("Changed dropdown");
      base.UpdateUI();
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetBoolean("isConcrete", isConcrete);
      return base.Write(writer);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // 0 DesignCode
      var designCode = this.GetAdSecDesignCode(DA, 0);

      // 1 StressStrain ULS Compression
      AdSecStressStrainCurveGoo ulsCompCrv = this.GetStressStrainCurveGoo(DA, 1, true);

      // 2 StressStrain ULS Tension
      AdSecStressStrainCurveGoo ulsTensCrv = this.GetStressStrainCurveGoo(DA, 2, false);

      // 3 StressStrain SLS Compression
      AdSecStressStrainCurveGoo slsCompCrv = this.GetStressStrainCurveGoo(DA, 3, true);

      // 4 StressStrain SLS Tension
      AdSecStressStrainCurveGoo slsTensCrv = this.GetStressStrainCurveGoo(DA, 4, false);

      // 5 Cracked params
      IConcreteCrackCalculationParameters concreteCrack = null;
      if (isConcrete) {
        concreteCrack = this.GetIConcreteCrackCalculationParameters(DA, 5);
      }

      // create new empty material
      var material = new AdSecMaterial();
      if (designCode != null) {
        material.DesignCode = designCode;
      }

      // set material type from dropdown input
      material.Type = _type;

      var comparer = new DoubleComparer();
      // create tension-compression curves from input
      if (comparer.Equals(ulsTensCrv.StressStrainCurve.FailureStrain.Value, 0)) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
          $"ULS Stress Strain Curve for Tension has zero failure strain.{Environment.NewLine}The curve has been changed to a simulate a material with no tension capacity (ε = 1, σ = 0)");
        IStressStrainCurve crv = ILinearStressStrainCurve.Create(
          IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
        var tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear,
          false);
        ulsTensCrv = new AdSecStressStrainCurveGoo(tuple.Item1, crv,
          AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, tuple.Item2);
      }

      if (comparer.Equals(ulsCompCrv.StressStrainCurve.FailureStrain.Value, 0)) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
          "ULS Stress Strain Curve for Compression has zero failure strain.");
        return;
      }

      var ulsTC = ITensionCompressionCurve.Create(ulsTensCrv.StressStrainCurve, ulsCompCrv.StressStrainCurve);
      var slsTC = ITensionCompressionCurve.Create(slsTensCrv.StressStrainCurve, slsCompCrv.StressStrainCurve);

      // create api material based on type
      switch (_type) {
        case MaterialType.Concrete:
          material.Material = concreteCrack == null ? IConcrete.Create(ulsTC, slsTC) :
            (IMaterial)IConcrete.Create(ulsTC, slsTC, concreteCrack);

          break;

        case MaterialType.FRP:
          material.Material = IFrp.Create(ulsTC, slsTC);
          break;

        case MaterialType.Rebar:
        case MaterialType.Tendon:
          material.Material = IReinforcement.Create(ulsTC, slsTC);
          break;

        case MaterialType.Steel:
          material.Material = ISteel.Create(ulsTC, slsTC);
          break;
      }

      // set output
      var adSecDesignCode = material.DesignCode;
      var materialDesign = new MaterialDesign() {
        Material = material.Material,
        DesignCode = new DesignCode() {
          IDesignCode = adSecDesignCode.DesignCode,
          DesignCodeName = adSecDesignCode.DesignCodeName
        }
      };
      DA.SetData(0, new AdSecMaterialGoo(materialDesign));
    }

    protected override void UpdateUIFromSelectedItems() {
      Enum.TryParse(_selectedItems[0], out _type);
      CreateAttributes();

      BusinessComponent.SetMaterialType(_type);

      UpdateUI();
    }
  }
}
