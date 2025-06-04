using System;
using System.Drawing;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.AdSec.Reinforcement.Layers;
using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateRebarSpacing : DropdownAdapter<CreateRebarSpacingFunction> {
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private CreateRebarSpacingFunction.FoldMode _mode = CreateRebarSpacingFunction.FoldMode.Distance;

    public override Guid ComponentGuid => new Guid("846d546a-4284-4d69-906b-0e6985d7ddd3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarSpacing;

    public override void SetSelected(int i, int j) {
      string selectedItem = _dropDownItems[i][j];
      _selectedItems[i] = selectedItem;
      if (i == 0) {
        _mode = (CreateRebarSpacingFunction.FoldMode)Enum.Parse(typeof(CreateRebarSpacingFunction.FoldMode),
          _selectedItems[i]);
        BusinessComponent.SetMode(_mode);
        _selectedItems[i] = selectedItem;
      } else {
        _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }

      base.UpdateUI();
    }

    protected override void SolveInternal(IGH_DataAccess da) {
      // 0 rebar input
      AdSecRebarBundleGoo rebar = this.GetAdSecRebarBundleGoo(da, 0);

      switch (_mode) {
        case CreateRebarSpacingFunction.FoldMode.Distance:
          var bundleD = new AdSecRebarLayerGoo(ILayerByBarPitch.Create(rebar.Value,
            (Length)Input.UnitNumber(this, da, 1, _lengthUnit)));
          da.SetData(0, bundleD);
          break;

        case CreateRebarSpacingFunction.FoldMode.Count:
          int count = 1;
          da.GetData(1, ref count);

          var bundleC = new AdSecRebarLayerGoo(ILayerByBarCount.Create(count, rebar.Value));
          da.SetData(0, bundleC);
          break;
      }
    }

    // protected override void UpdateUIFromSelectedItems() {
    //   _mode = (CreateRebarSpacingFunction.FoldMode)Enum.Parse(typeof(CreateRebarSpacingFunction.FoldMode),
    //     _selectedItems[0]);
    //   if (_mode == CreateRebarSpacingFunction.FoldMode.Distance) {
    //     _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
    //   }
    //
    //   BusinessComponent.SetMode(_mode);
    //   base.UpdateUIFromSelectedItems();
    // }
  }
}
