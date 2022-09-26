using System;
using System.Linq;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.Units;
using OasysGH;
using OasysGH.Components;
using Rhino.Geometry;
using UnitsNet;

namespace AdSecGH.Components
{
  public class CreateCustomMaterial : GH_OasysDropDownComponent, IGH_VariableParameterComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon
    // including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("29f87bee-c84c-5d11-9b30-492190df2910");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateCustomMaterial;

    public CreateCustomMaterial()
      : base("Custom Material", "CustomMaterial", "Create a custom AdSec Material",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat1())
    { this.Hidden = true; } // sets the initial state of the component to hidden
    #endregion

    #region Input and output
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
      if (IsConcrete)
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
      if (ulsTensCrv.StressStrainCurve.FailureStrain.Value == 0)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "ULS Stress Strain Curve for Tension has zero failure strain."
            + System.Environment.NewLine + "The curve has been changed to a simulate a material with no tension capacity (ε = 1, σ = 0)");
        IStressStrainCurve crv = ILinearStressStrainCurve.Create(IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio)));
        Tuple<Curve, List<Point3d>> tuple = AdSecStressStrainCurveGoo.Create(crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, false);
        ulsTensCrv = new AdSecStressStrainCurveGoo(tuple.Item1, crv, AdSecStressStrainCurveGoo.StressStrainCurveType.Linear, tuple.Item2);
      }
      if (ulsCompCrv.StressStrainCurve.FailureStrain.Value == 0)
      {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "ULS Stress Strain Curve for Compression has zero failure strain.");
        return;
      }
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

    #region Custom UI
    private AdSecMaterial.AdSecMaterialType type = AdSecMaterial.AdSecMaterialType.Concrete;
    private bool IsConcrete = true;

    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Material Type"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // populate lists
      this.DropDownItems.Add(Enum.GetNames(typeof(AdSecMaterial.AdSecMaterialType)).ToList());
      this.SelectedItems.Add(AdSecMaterial.AdSecMaterialType.Concrete.ToString());

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // change selected item
      this.SelectedItems[i] = this.DropDownItems[i][j];

      // cast selection to material type enum
      Enum.TryParse(this.SelectedItems[0], out type);

      // set bool if selection is concrete
      if (this.SelectedItems[i] == AdSecMaterial.AdSecMaterialType.Concrete.ToString())
        IsConcrete = true;
      else
        IsConcrete = false;

      // update input params
      this.ChangeMode();
      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      // cast selection to material type enum
      Enum.TryParse(this.SelectedItems[0], out type);

      // don´t know if this needs to happen before ChangeMode()
      CreateAttributes();

      this.ChangeMode();

      this.UpdateUIFromSelectedItems();
    }
    #endregion

    private void ChangeMode()
    {
      if (IsConcrete)
        if (Params.Input.Count == 6) { return; }

      if (!IsConcrete)
        if (Params.Input.Count == 5) { return; }

      RecordUndoEvent("Changed dropdown");

      // change number of input parameters
      if (IsConcrete)
        Params.RegisterInputParam(new Param_GenericObject());
      else
        Params.UnregisterInputParameter(Params.Input[5], true);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
    }

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      writer.SetBoolean("isConcrete", IsConcrete);
      return base.Write(writer);
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      IsConcrete = reader.GetBoolean("isConcrete");
      return base.Read(reader);
    }
    #endregion

    #region IGH_VariableParameterComponent null implementation
    void IGH_VariableParameterComponent.VariableParameterMaintenance()
    {
      if (IsConcrete)
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
