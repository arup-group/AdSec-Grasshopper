using System;
using System.Linq;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH.Components;
using UnitsNet;
using OasysGH;
using Oasys.Units;
using UnitsNet.Units;

namespace AdSecGH.Components
{
  /// <summary>
  /// Component to create a new Stress Strain Point
  /// </summary>
  public class CreateStressStrainPoint : GH_OasysDropDownComponent, IGH_VariableParameterComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon
    // including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("69a789d4-c11b-4396-b237-a10efdd6d0c4");
    public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.StressStrainPt;

    public CreateStressStrainPoint()
      : base("Create StressStrainPt", "StressStrainPt", "Create a Stress Strain Point for AdSec Stress Strain Curve",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat1())
    { this.Hidden = false; } // sets the initial state of the component to hidden
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // create new point
      AdSecStressStrainPointGoo pt = new AdSecStressStrainPointGoo(
          GetInput.Stress(this, DA, 1, StressUnit),
          GetInput.Strain(this, DA, 0, StrainUnit));

      DA.SetData(0, pt);
    }

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(this.StrainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(this.StressUnit);
      pManager.AddGenericParameter("Strain [" + strainUnitAbbreviation + "]", "ε", "Value for strain (X-axis)", GH_ParamAccess.item);
      pManager.AddGenericParameter("Stress [" + stressUnitAbbreviation + "]", "σ", "Value for stress (Y-axis)", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("StressStrainPt", "SPt", "AdSec Stress Strain Point", GH_ParamAccess.item);
    }
    #endregion

    #region Custom UI
    private StrainUnit StrainUnit = Units.StrainUnit;
    private PressureUnit StressUnit = Units.StressUnit;
    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Strain Unit",
        "Stress Unit"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // strain
      this.DropDownItems.Add(Units.FilteredStrainUnits);
      this.SelectedItems.Add(StrainUnit.ToString());

      // pressure
      this.DropDownItems.Add(Units.FilteredStressUnits);
      this.SelectedItems.Add(StressUnit.ToString());

      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // change selected item
      this.SelectedItems[i] = this.DropDownItems[i][j];

      switch (i)
      {
        case 0:
          StrainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), this.SelectedItems[i]);
          break;
        case 1:
          StressUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), this.SelectedItems[i]);
          break;
      }
      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      this.StrainUnit = (Oasys.Units.StrainUnit)Enum.Parse(typeof(Oasys.Units.StrainUnit), this.SelectedItems[0]);
      this.StressUnit = (UnitsNet.Units.PressureUnit)Enum.Parse(typeof(UnitsNet.Units.PressureUnit), this.SelectedItems[1]);

      base.UpdateUIFromSelectedItems();
    }
    #endregion

    #region IGH_VariableParameterComponent null implementation
    void IGH_VariableParameterComponent.VariableParameterMaintenance()
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(StrainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(StressUnit);
      Params.Input[0].Name = "Strain [" + strainUnitAbbreviation + "]";
      Params.Input[1].Name = "Stress [" + stressUnitAbbreviation + "]";
    }
    #endregion
  }
}
