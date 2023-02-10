using System;
using System.Collections.Generic;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
  public class CreateStressStrainPoint : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("69a789d4-c11b-4396-b237-a10efdd6d0c4");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.StressStrainPt;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;

    public CreateStressStrainPoint() : base(
      "Create StressStrainPt",
      "StressStrainPt",
      "Create a Stress Strain Point for AdSec Stress Strain Curve",
      CategoryName.Name(),
      SubCategoryName.Cat1())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(this._strainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(this._stressUnit);
      pManager.AddGenericParameter("Strain [" + strainUnitAbbreviation + "]", "ε", "Value for strain (X-axis)", GH_ParamAccess.item);
      pManager.AddGenericParameter("Stress [" + stressUnitAbbreviation + "]", "σ", "Value for stress (Y-axis)", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("StressStrainPt", "SPt", "AdSec Stress Strain Point", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // create new point
      AdSecStressStrainPointGoo pt = new AdSecStressStrainPointGoo(
        (Pressure)Input.UnitNumber(this, DA, 1, this._stressUnit),
        (Strain)Input.UnitNumber(this, DA, 0, this._strainUnit));

      DA.SetData(0, pt);
    }

    #region Custom UI
    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Strain Unit",
        "Stress Unit"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
      this.SelectedItems.Add(Strain.GetAbbreviation(this._strainUnit));

      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress));
      this.SelectedItems.Add(Pressure.GetAbbreviation(this._stressUnit));

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      this.SelectedItems[i] = this.DropDownItems[i][j];

      switch (i)
      {
        case 0:
          this._strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), this.SelectedItems[i]);
          break;
        case 1:
          this._stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[i]);
          break;
      }
      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      this._strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), this.SelectedItems[0]);
      this._stressUnit = (PressureUnit)UnitsHelper.Parse(typeof(PressureUnit), this.SelectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(this._strainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(this._stressUnit);
      Params.Input[0].Name = "Strain [" + strainUnitAbbreviation + "]";
      Params.Input[1].Name = "Stress [" + stressUnitAbbreviation + "]";
    }
  }
}
