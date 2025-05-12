using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

using CreateRebarFunction = AdSecCore.Functions.CreateRebarFunction;

namespace AdSecGH.Components {
  public class CreateRebar : DropdownAdapter<CreateRebarFunction> {

    public override Guid ComponentGuid => new Guid("024d241a-b6cc-4134-9f5c-ac9a6dcb2c4b");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Rebar;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        BusinessComponent.SetMode(UpdateMode());
        ToggleInput();
      } else {
        BusinessComponent.LengthUnitGeometry = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }
    }

    private RebarMode UpdateMode() {
      return (RebarMode)Enum.Parse(typeof(RebarMode), _selectedItems[0]);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // 0 material input
      var material = this.GetAdSecMaterial(DA, 0);

      switch (BusinessComponent.Mode) {
        case RebarMode.Single:
          var rebar = new AdSecRebarBundleGoo(IBarBundle.Create((IReinforcement)material.Material,
            (Length)Input.UnitNumber(this, DA, 1, BusinessComponent.LengthUnitGeometry)));
          DA.SetData(0, rebar);
          break;

        case RebarMode.Bundle:
          int count = 1;
          DA.GetData(2, ref count);

          var bundle = new AdSecRebarBundleGoo(IBarBundle.Create((IReinforcement)material.Material,
            (Length)Input.UnitNumber(this, DA, 1, BusinessComponent.LengthUnitGeometry), count));

          DA.SetData(0, bundle);
          break;
      }
    }

    protected override void UpdateUIFromSelectedItems() {
      BusinessComponent.SetMode((UpdateMode()));
      BusinessComponent.LengthUnitGeometry = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      ToggleInput();
      base.UpdateUIFromSelectedItems();
    }

    private void ToggleInput() {
      RecordUndoEvent("Changed dropdown");
      switch (BusinessComponent.Mode) {
        case RebarMode.Single:
          // remove any additional input parameters
          while (Params.Input.Count > 2) {
            Params.UnregisterInputParameter(Params.Input[2], true);
          }

          break;

        case RebarMode.Bundle:
          // add input parameter
          while (Params.Input.Count != 3) {
            Params.RegisterInputParam(new Param_Integer());
          }

          break;
      }
    }
  }
}
