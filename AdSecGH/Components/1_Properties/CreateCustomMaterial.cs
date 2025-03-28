using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using GH_IO.Serialization;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;

using OasysGH;
using OasysGH.Components;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateCustomMaterial : GH_OasysDropDownComponent {
    private bool _isConcrete = true;
    private AdSecMaterial.AdSecMaterialType _type = AdSecMaterial.AdSecMaterialType.Concrete;

    public CreateCustomMaterial() : base("Custom Material", "CustomMaterial", "Create a custom AdSec Material",
      CategoryName.Name(), SubCategoryName.Cat1()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("29f87bee-c84c-5d11-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateCustomMaterial;

    public override bool Read(GH_IReader reader) {
      _isConcrete = reader.GetBoolean("isConcrete");
      return base.Read(reader);
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      Enum.TryParse(_selectedItems[0], out _type);

      // set bool if selection is concrete
      if (_selectedItems[i] == AdSecMaterial.AdSecMaterialType.Concrete.ToString()) {
        _isConcrete = true;
      } else {
        _isConcrete = false;
      }

      // update input params
      ChangeMode();
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      if (_isConcrete) {
        Params.Input[5].Name = "Yield PointCrack Calc Params";
        Params.Input[5].NickName = "CCP";
        Params.Input[5].Description = "[Optional] Material's Crack Calculation Parameters";
        Params.Input[5].Access = GH_ParamAccess.item;
        Params.Input[5].Optional = true;
      }
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetBoolean("isConcrete", _isConcrete);
      return base.Write(writer);
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Material Type",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(Enum.GetNames(typeof(AdSecMaterial.AdSecMaterialType)).ToList());
      _selectedItems.Add(AdSecMaterial.AdSecMaterialType.Concrete.ToString());

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("DesignCode", "Code", "[Optional] Set the Material's DesignCode",
        GH_ParamAccess.item);
      pManager[0].Optional = true;
      pManager.AddGenericParameter("ULS Comp. Crv", "U_C", "ULS Stress Strain Curve for Compression",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("ULS Tens. Crv", "U_T", "ULS Stress Strain Curve for Tension", GH_ParamAccess.item);
      pManager.AddGenericParameter("SLS Comp. Crv", "S_C", "SLS Stress Strain Curve for Compression",
        GH_ParamAccess.item);
      pManager.AddGenericParameter("SLS Tens. Crv", "S_T", "SLS Stress Strain Curve for Tension", GH_ParamAccess.item);
      pManager.AddGenericParameter("Crack Calc Params", "CCP", "[Optional] Material's Crack Calculation Parameters",
        GH_ParamAccess.item);
      pManager[5].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Material", "Mat", "Custom AdSec Material", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // 0 DesignCode
      var designCode = this.GetAdSecDesignCode(DA, 0);

      // 1 StressStrain ULS Compression
      var ulsCompCrv = this.GetStressStrainCurveGoo(DA, 1, true);

      // 2 StressStrain ULS Tension
      var ulsTensCrv = this.GetStressStrainCurveGoo(DA, 2, false);

      // 3 StressStrain SLS Compression
      var slsCompCrv = this.GetStressStrainCurveGoo(DA, 3, true);

      // 4 StressStrain SLS Tension
      var slsTensCrv = this.GetStressStrainCurveGoo(DA, 4, false);

      // 5 Cracked params
      IConcreteCrackCalculationParameters concreteCrack = null;
      if (_isConcrete) {
        concreteCrack = this.GetIConcreteCrackCalculationParameters(DA, 5);
      }

      // create new empty material
      var material = new AdSecMaterial();
      if (designCode != null) {
        material.DesignCode = designCode;
      }

      // set material type from dropdown input
      material.Type = _type;

      // create tension-compression curves from input
      if (ulsTensCrv.StressStrainCurve.FailureStrain.Value == 0) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
          $"ULS Stress Strain Curve for Tension has zero failure strain.{Environment.NewLine}The curve has been changed to a simulate a material with no tension capacity (ε = 1, σ = 0)");
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

      // create api material based on type
      switch (_type) {
        case AdSecMaterial.AdSecMaterialType.Concrete:
          if (concreteCrack == null) {
            material.Material = IConcrete.Create(ulsTC, slsTC);
          } else {
            material.Material = IConcrete.Create(ulsTC, slsTC, concreteCrack);
          }

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
      // cast selection to material type enum
      Enum.TryParse(_selectedItems[0], out _type);

      // don´t know if this needs to happen before ChangeMode()
      CreateAttributes();

      ChangeMode();

      UpdateUI();
    }

    private void ChangeMode() {
      if (_isConcrete) {
        if (Params.Input.Count == 6) {
          return;
        }
      }

      if (!_isConcrete) {
        if (Params.Input.Count == 5) {
          return;
        }
      }

      RecordUndoEvent("Changed dropdown");

      // change number of input parameters
      if (_isConcrete) {
        Params.RegisterInputParam(new Param_GenericObject());
      } else {
        Params.UnregisterInputParameter(Params.Input[5], true);
      }

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
    }
  }
}
