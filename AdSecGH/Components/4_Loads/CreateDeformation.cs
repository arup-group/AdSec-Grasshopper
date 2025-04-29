using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateDeformationGh : CreateDeformationFunction {
    public CreateDeformationGh() {
    }
  }

  public class CreateDeformation : DropdownAdapter<CreateDeformationGh> {
    private CurvatureUnit _curvatureUnit = DefaultUnits.CurvatureUnit;
    private StrainUnit _strainUnit = DefaultUnits.StrainUnitResult;
    public CreateDeformation() : base() { Hidden = true; }
    public override Guid ComponentGuid => new Guid("cbab2b58-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.DeformationLoad;


    protected override void BeforeSolveInstance() {
      // Update units before solving
      UpdateUnit();
      //update local unit
      BusinessComponent.StrainUnitResult = _strainUnit;
      BusinessComponent.CurvatureUnit = _curvatureUnit;
      BusinessComponent.UpdateOutputParameter();
      RefreshParameter();
    }

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


    protected override void UpdateUIFromSelectedItems() {
      BusinessComponent.UpdateOutputParameter();
      base.UpdateUIFromSelectedItems();
    }
  }
}
