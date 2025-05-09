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

using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  public class CreateRebar : DropdownAdapter<CreateRebarFunction> {
    private LengthUnit _lengthUnit = DefaultUnits.LengthUnitGeometry;
    private FoldMode _mode = FoldMode.Single;

    public override Guid ComponentGuid => new Guid("024d241a-b6cc-4134-9f5c-ac9a6dcb2c4b");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Rebar;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[i]);
        ToggleInput();
      } else {
        _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[i]);
      }
    }

    public override void VariableParameterMaintenance() {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      Params.Input[1].Name = $"Diameter [{unitAbbreviation}]";
      if (_mode == FoldMode.Bundle) {
        Params.Input[2].Name = "Count";
        Params.Input[2].NickName = "N";
        Params.Input[2].Description = "Count per bundle (1, 2, 3 or 4)";
        Params.Input[2].Access = GH_ParamAccess.item;
        Params.Input[2].Optional = false;
      }
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Rebar Type",
        "Measure",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(Enum.GetNames(typeof(FoldMode)).ToList());
      _selectedItems.Add(_dropDownItems[0][0]);

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Length));
      _selectedItems.Add(Length.GetAbbreviation(_lengthUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string unitAbbreviation = Length.GetAbbreviation(_lengthUnit);
      pManager.AddGenericParameter("Material", "Mat", "AdSec Reinforcement Material", GH_ParamAccess.item);
      pManager.AddGenericParameter($"Diameter [{unitAbbreviation}]", "Ø", "Bar Diameter", GH_ParamAccess.item);
      _mode = FoldMode.Single;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Rebar", "Rb", "Rebar (single or bundle) for AdSec Reinforcement",
        GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // 0 material input
      var material = this.GetAdSecMaterial(DA, 0);

      switch (_mode) {
        case FoldMode.Single:
          var rebar = new AdSecRebarBundleGoo(IBarBundle.Create((IReinforcement)material.Material,
            (Length)Input.UnitNumber(this, DA, 1, _lengthUnit)));
          DA.SetData(0, rebar);
          break;

        case FoldMode.Bundle:
          int count = 1;
          DA.GetData(2, ref count);

          var bundle = new AdSecRebarBundleGoo(IBarBundle.Create((IReinforcement)material.Material,
            (Length)Input.UnitNumber(this, DA, 1, _lengthUnit), count));

          DA.SetData(0, bundle);
          break;
      }
    }

    protected override void UpdateUIFromSelectedItems() {
      _mode = (FoldMode)Enum.Parse(typeof(FoldMode), _selectedItems[0]);
      _lengthUnit = (LengthUnit)UnitsHelper.Parse(typeof(LengthUnit), _selectedItems[1]);
      ToggleInput();
      base.UpdateUIFromSelectedItems();
    }

    private void ToggleInput() {
      RecordUndoEvent("Changed dropdown");
      switch (_mode) {
        case FoldMode.Single:
          // remove any additional input parameters
          while (Params.Input.Count > 2) {
            Params.UnregisterInputParameter(Params.Input[2], true);
          }

          break;

        case FoldMode.Bundle:
          // add input parameter
          while (Params.Input.Count != 3) {
            Params.RegisterInputParam(new Param_Integer());
          }

          break;
      }
    }

    private enum FoldMode {
      Single,
      Bundle,
    }
  }
}
