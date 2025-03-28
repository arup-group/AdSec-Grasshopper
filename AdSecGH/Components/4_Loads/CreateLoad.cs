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
  public class CreateLoad : GH_OasysDropDownComponent {
    private ForceUnit _forceUnit = DefaultUnits.ForceUnit;
    private MomentUnit _momentUnit = DefaultUnits.MomentUnit;

    public CreateLoad() : base($"Create{AdSecLoadGoo.Name.Replace(" ", string.Empty)}",
      AdSecLoadGoo.Name.Replace(" ", string.Empty),
      $"Create an{AdSecLoadGoo.Description} from an axial force and biaxial moments", CategoryName.Name(),
      SubCategoryName.Cat5()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("cbab2b74-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateLoad;

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      switch (i) {
        case 0:
          _forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), _selectedItems[i]);
          break;

        case 1:
          _momentUnit = (MomentUnit)UnitsHelper.Parse(typeof(MomentUnit), _selectedItems[i]);
          break;
      }

      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string forceUnitAbbreviation = Force.GetAbbreviation(_forceUnit);
      string momentUnitAbbreviation = Moment.GetAbbreviation(_momentUnit);
      Params.Input[0].Name = $"Fx [{forceUnitAbbreviation}]";
      Params.Input[1].Name = $"Myy [{momentUnitAbbreviation}]";
      Params.Input[2].Name = $"Mzz [{momentUnitAbbreviation}]";
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Force Unit",
        "Moment Unit",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      // force
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force));
      _selectedItems.Add(Force.GetAbbreviation(_forceUnit));

      // moment
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Moment));
      _selectedItems.Add(Moment.GetAbbreviation(_momentUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string forceUnitAbbreviation = Force.GetAbbreviation(_forceUnit);
      string momentUnitAbbreviation = Moment.GetAbbreviation(_momentUnit);
      pManager.AddGenericParameter($"Fx [{forceUnitAbbreviation}]", "X", "The axial force. Positive x is tension.",
        GH_ParamAccess.item);
      pManager.AddGenericParameter($"Myy [{momentUnitAbbreviation}]", "YY",
        "The moment about local y-axis. Positive yy is anti - clockwise moment about local y-axis.",
        GH_ParamAccess.item);
      pManager.AddGenericParameter($"Mzz [{momentUnitAbbreviation}]", "ZZ",
        "The moment about local z-axis. Positive zz is anti - clockwise moment about local z-axis.",
        GH_ParamAccess.item);
      // make all but last input optional
      for (int i = 0; i < pManager.ParamCount - 1; i++) {
        pManager[i].Optional = true;
      }
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter(AdSecLoadGoo.Name, AdSecLoadGoo.NickName, AdSecLoadGoo.Description,
        GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess da) {
      // Create new load
      var load = ILoad.Create((Force)Input.UnitNumber(this, da, 0, _forceUnit, true),
        (Moment)Input.UnitNumber(this, da, 1, _momentUnit, true),
        (Moment)Input.UnitNumber(this, da, 2, _momentUnit, true));

      // check for enough input parameters
      if (Params.Input[0].SourceCount == 0 && Params.Input[1].SourceCount == 0 && Params.Input[2].SourceCount == 0) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
          $"Input parameters {Params.Input[0].NickName}, {Params.Input[1].NickName}, and {Params.Input[2].NickName} failed to collect data!");
        return;
      }

      da.SetData(0, new AdSecLoadGoo(load));
    }

    protected override void UpdateUIFromSelectedItems() {
      _forceUnit = (ForceUnit)UnitsHelper.Parse(typeof(ForceUnit), _selectedItems[0]);
      _momentUnit = (MomentUnit)UnitsHelper.Parse(typeof(MomentUnit), _selectedItems[1]);
      base.UpdateUIFromSelectedItems();
    }
  }
}
