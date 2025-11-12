using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateDeformationLoadGh : CreateDeformationLoadFunction {
    public CreateDeformationLoadGh() {
    }
  }

  public class CreateDeformationLoad : DropdownAdapter<CreateDeformationLoadGh> {
    private CurvatureUnit _curvatureUnit = DefaultUnits.CurvatureUnit;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;

    public CreateDeformationLoad() : base() { Hidden = true; }
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
      UpdateUnits();
      base.UpdateUI();
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

    protected override void BeforeSolveInstance() {
      UpdateUnits();
    }

    private void UpdateUnits() {
      UpdateDefaultUnits();
      //update local unit if any
      BusinessComponent.StrainUnitResult = _strainUnit;
      BusinessComponent.CurvatureUnit = _curvatureUnit;
      RefreshParameter();
    }

  }
}
