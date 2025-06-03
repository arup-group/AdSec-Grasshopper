using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

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
    private FoldMode _mode = FoldMode.Distance;

    public override Guid ComponentGuid => new Guid("846d546a-4284-4d69-906b-0e6985d7ddd3");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.RebarSpacing;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[i]);
        if (_mode == FoldMode.Count) {
          // remove the second dropdown (length)
          while (_dropDownItems.Count > 1) {
            _dropDownItems.RemoveAt(_dropDownItems.Count - 1);
          }

          while (_selectedItems.Count > 1) {
            _selectedItems.RemoveAt(_selectedItems.Count - 1);
          }
        } else {
          // add second dropdown (length)
          if (_dropDownItems.Count != 2) {
            _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
            _selectedItems.Add(_lengthUnit.ToString());
          }
        }

        ToggleInput();
      } else {
        _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }

      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      if (_mode == FoldMode.Distance) {
        string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
        Params.Input[1].Name = $"Spacing [{unitAbbreviation}]";
        Params.Input[1].NickName = "S";
        Params.Input[1].Description
          = "Number of bars is calculated based on the available length and the given bar pitch. The bar pitch is re-calculated to place the bars at equal spacing, with a maximum final pitch of the given value. Example: If the available length for the bars is 1000mm and the given bar pitch is 300mm, then the number of spacings that can fit in the available length is calculated as 1000 / 300 i.e. 3.333. The number of spacings is rounded up (3.333 rounds up to 4) and the bar pitch re-calculated (1000mm / 4), resulting in a final pitch of 250mm.";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }

      if (_mode == FoldMode.Count) {
        Params.Input[1].Name = "Count";
        Params.Input[1].NickName = "N";
        Params.Input[1].Description
          = "The number of bundles or single bars. The bundles or single bars are spaced out evenly over the available space.";
        Params.Input[1].Access = GH_ParamAccess.item;
        Params.Input[1].Optional = false;
      }
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Spacing method",
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      _selectedItems.Add(_dropDownItems[0][0]);

      // length
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));

      _isInitialised = true;
      _mode = FoldMode.Distance;
    }

    // protected override void RegisterInputParams(GH_InputParamManager pManager) {
    //   string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
    //   pManager.AddGenericParameter("Rebar", "Rb", "AdSec Rebar (single or bundle)", GH_ParamAccess.item);
    //   pManager.AddGenericParameter($"Spacing [{unitAbbreviation}]", "S",
    //     "Number of bars is calculated based on the available length and the given bar pitch. The bar pitch is re-calculated to place the bars at equal spacing, with a maximum final pitch of the given value. Example: If the available length for the bars is 1000mm and the given bar pitch is 300mm, then the number of spacings that can fit in the available length is calculated as 1000 / 300 i.e. 3.333. The number of spacings is rounded up (3.333 rounds up to 4) and the bar pitch re-calculated (1000mm / 4), resulting in a final pitch of 250mm.",
    //     GH_ParamAccess.item);
    // }

    protected override void SolveInternal(IGH_DataAccess da) {
      // 0 rebar input
      AdSecRebarBundleGoo rebar = this.GetAdSecRebarBundleGoo(da, 0);

      switch (_mode) {
        case FoldMode.Distance:
          var bundleD = new AdSecRebarLayerGoo(ILayerByBarPitch.Create(rebar.Value,
            (Length)Input.UnitNumber(this, da, 1, _lengthUnit)));
          da.SetData(0, bundleD);
          break;

        case FoldMode.Count:
          int count = 1;
          da.GetData(1, ref count);

          var bundleC = new AdSecRebarLayerGoo(ILayerByBarCount.Create(count, rebar.Value));
          da.SetData(0, bundleC);
          break;
      }
    }

    protected override void UpdateUIFromSelectedItems() {
      _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[0]);
      if (_mode == FoldMode.Distance) {
        _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      }

      ToggleInput();
      base.UpdateUIFromSelectedItems();
    }

    private void ToggleInput() {
      switch (_mode) {
        case FoldMode.Distance:
          // remove any additional input parameters
          while (Params.Input.Count > 1) {
            Params.UnregisterInputParameter(Params.Input[1], true);
          }

          Params.RegisterInputParam(new Param_GenericObject());
          break;

        case FoldMode.Count:
          // add input parameter
          while (Params.Input.Count > 1) {
            Params.UnregisterInputParameter(Params.Input[1], true);
          }

          Params.RegisterInputParam(new Param_Integer());
          break;
      }
    }

    private enum FoldMode {
      Distance,
      Count,
    }
  }
}
