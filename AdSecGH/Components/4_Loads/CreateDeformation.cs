using System;
using System.Collections.Generic;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components
{
    public class CreateDeformation : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("cbab2b58-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.DeformationLoad;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;
    private CurvatureUnit _curvatureUnit = DefaultUnits.CurvatureUnit;

    public CreateDeformation() : base(
      "Create Deformation Load",
      "Deformation",
      "Create an AdSec Deformation Load from an axial strain and biaxial curvatures", CategoryName.Name(),
      SubCategoryName.Cat5())
    {
      this.Hidden = true; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(this._strainUnit);
      string curvatureUnitAbbreviation = Curvature.GetAbbreviation(this._curvatureUnit);
      pManager.AddGenericParameter("εx [" + strainUnitAbbreviation + "]", "X", "The axial strain. Positive X indicates tension.", GH_ParamAccess.item);
      pManager.AddGenericParameter("κyy [" + curvatureUnitAbbreviation + "]", "YY", "The curvature about local y-axis. It follows the right hand grip rule about the axis. Positive YY is anti-clockwise curvature about local y-axis.", GH_ParamAccess.item);
      pManager.AddGenericParameter("κzz [" + curvatureUnitAbbreviation + "]", "ZZ", "The curvature about local z-axis. It follows the right hand grip rule about the axis. Positive ZZ is anti-clockwise curvature about local z-axis.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Load", "Ld", "AdSec Load", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // Create new load
      IDeformation deformation = IDeformation.Create(
        (Strain)Input.UnitNumber(this, DA, 0, this._strainUnit),
        (Curvature)Input.UnitNumber(this, DA, 1, this._curvatureUnit),
        (Curvature)Input.UnitNumber(this, DA, 2, this._curvatureUnit));

      DA.SetData(0, new AdSecDeformationGoo(deformation));
    }

    #region Custom UI
    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[]
      {
        "Strain Unit",
        "Curvature Unit"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      // strain
      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
      this.SelectedItems.Add(_strainUnit.ToString());

      // curvature
      this.DropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Curvature));
      this.SelectedItems.Add(this._curvatureUnit.ToString());

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
          this._curvatureUnit = (CurvatureUnit)UnitsHelper.Parse(typeof(CurvatureUnit), this.SelectedItems[i]);
          break;
      }
      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      this._strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), this.SelectedItems[0]);
      this._curvatureUnit = (CurvatureUnit)UnitsHelper.Parse(typeof(CurvatureUnit), this.SelectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      string strainUnitAbbreviation = Strain.GetAbbreviation(this._strainUnit);
      string curvatureUnitAbbreviation = Curvature.GetAbbreviation(this._curvatureUnit);
      Params.Input[0].Name = "εx [" + strainUnitAbbreviation + "]";
      Params.Input[1].Name = "κyy [" + curvatureUnitAbbreviation + "]";
      Params.Input[2].Name = "κzz [" + curvatureUnitAbbreviation + "]";
    }
  }
}
