using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using Oasys.AdSec;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateDeformation : GH_OasysDropDownComponent {
    private CurvatureUnit _curvatureUnit = DefaultUnits.CurvatureUnit;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;

    public CreateDeformation() : base("Create Deformation Load", "Deformation",
      "Create an AdSec Deformation Load from an axial strain and biaxial curvatures", CategoryName.Name(),
      SubCategoryName.Cat5()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("cbab2b58-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.DeformationLoad;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      switch (i) {
        case 0:
          _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[i]);
          break;

        case 1:
          _curvatureUnit = (CurvatureUnit)UnitsHelper.Parse(typeof(CurvatureUnit), _selectedItems[i]);
          break;
      }

      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string strainUnitAbbreviation = Strain.GetAbbreviation(_strainUnit);
      string curvatureUnitAbbreviation = Curvature.GetAbbreviation(_curvatureUnit);
      Params.Input[0].Name = $"εx [{strainUnitAbbreviation}]";
      Params.Input[1].Name = $"κyy [{curvatureUnitAbbreviation}]";
      Params.Input[2].Name = $"κzz [{curvatureUnitAbbreviation}]";
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Strain Unit",
        "Curvature Unit",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      // strain
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain));
      _selectedItems.Add(Strain.GetAbbreviation(_strainUnit));

      // curvature
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Curvature));
      _selectedItems.Add(Curvature.GetAbbreviation(_curvatureUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string strainUnitAbbreviation = Strain.GetAbbreviation(_strainUnit);
      string curvatureUnitAbbreviation = Curvature.GetAbbreviation(_curvatureUnit);
      pManager.AddGenericParameter($"εx [{strainUnitAbbreviation}]", "X",
        "The axial strain. Positive X indicates tension.", GH_ParamAccess.item);
      pManager.AddGenericParameter($"κyy [{curvatureUnitAbbreviation}]", "YY",
        "The curvature about local y-axis. It follows the right hand grip rule about the axis. Positive YY is anti-clockwise curvature about local y-axis.",
        GH_ParamAccess.item);
      pManager.AddGenericParameter($"κzz [{curvatureUnitAbbreviation}]", "ZZ",
        "The curvature about local z-axis. It follows the right hand grip rule about the axis. Positive ZZ is anti-clockwise curvature about local z-axis.",
        GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Load", "Ld", "AdSec Load", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess da) {
      // Create new load
      var deformation = IDeformation.Create((Strain)Input.UnitNumber(this, da, 0, _strainUnit),
        (Curvature)Input.UnitNumber(this, da, 1, _curvatureUnit),
        (Curvature)Input.UnitNumber(this, da, 2, _curvatureUnit));

      da.SetData(0, new AdSecDeformationGoo(deformation));
    }

    protected override void UpdateUIFromSelectedItems() {
      _strainUnit = (StrainUnit)UnitsHelper.Parse(typeof(StrainUnit), _selectedItems[0]);
      _curvatureUnit = (CurvatureUnit)UnitsHelper.Parse(typeof(CurvatureUnit), _selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
