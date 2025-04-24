using System;
using System.Collections.Generic;
using System.Drawing;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using GH_IO.Serialization;

using Grasshopper.Kernel;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;

using OasysGH;
using OasysGH.Components;
using OasysGH.Helpers;
using OasysGH.Units;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Components {
  /// <summary>
  ///   Component to create a new Stress Strain Point
  /// </summary>
  public class CreatePreLoad : GH_OasysDropDownComponent {
    private const string _selectedForceUnit = "Force";
    private const string _selectedStrainUnit = "Strain";
    private const string _selectedStressUnit = "Stress";

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("cbab2b12-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.Prestress;
    private ForceUnit _forceUnit = DefaultUnits.ForceUnit;
    private StrainUnit _strainUnit = DefaultUnits.MaterialStrainUnit;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;

    public CreatePreLoad() : base("Create Prestress", "Prestress",
      "Create an AdSec Prestress Load for Reinforcement Layout as either Preforce, Prestrain or Prestress",
      CategoryName.Name(), SubCategoryName.Cat5()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    public override bool Read(GH_IReader reader) {
      _forceUnit = (ForceUnit)Enum.Parse(typeof(ForceUnit), reader.GetString("force"));
      _strainUnit = (StrainUnit)Enum.Parse(typeof(StrainUnit), reader.GetString("strain"));
      _stressUnit = (PressureUnit)Enum.Parse(typeof(PressureUnit), reader.GetString("stress"));
      return base.Read(reader);
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];
      if (i == 0) {
        switch (_selectedItems[0]) {
          case _selectedForceUnit:
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force);
            _selectedItems[0] = _forceUnit.ToString();
            break;

          case _selectedStrainUnit:
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain);
            _selectedItems[0] = _strainUnit.ToString();
            break;

          case _selectedStressUnit:
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress);
            _selectedItems[0] = _stressUnit.ToString();
            break;
        }
      } else {
        switch (_selectedItems[0]) {
          case _selectedForceUnit:
            _forceUnit = (ForceUnit)Enum.Parse(typeof(ForceUnit), _selectedItems[i]);
            break;

          case _selectedStrainUnit:
            _strainUnit = (StrainUnit)Enum.Parse(typeof(StrainUnit), _selectedItems[i]);
            break;

          case _selectedStressUnit:
            _stressUnit = (PressureUnit)Enum.Parse(typeof(PressureUnit), _selectedItems[i]);
            break;
        }
      }

      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      string forceUnitAbbreviation = Force.GetAbbreviation(_forceUnit);
      string strainUnitAbbreviation = Strain.GetAbbreviation(_strainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(_stressUnit);

      switch (_selectedItems[0]) {
        case _selectedForceUnit:
          Params.Input[1].Name = $"{_selectedForceUnit} [{forceUnitAbbreviation}]";
          Params.Input[1].NickName = "P";
          break;

        case _selectedStrainUnit:
          Params.Input[1].Name = $"{_selectedStrainUnit} [{strainUnitAbbreviation}]";
          Params.Input[1].NickName = "ε";
          break;

        case _selectedStressUnit:
          Params.Input[1].Name = $"{_selectedStressUnit} [{stressUnitAbbreviation}]";
          Params.Input[1].NickName = "σ";
          break;
      }
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetString("force", _forceUnit.ToString());
      writer.SetString("strain", _strainUnit.ToString());
      writer.SetString("stress", _stressUnit.ToString());
      return base.Write(writer);
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string> {
        _selectedForceUnit,
        _selectedStrainUnit,
        _selectedStressUnit,
      };

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      _dropDownItems.Add(_spacerDescriptions);
      _selectedItems.Add(_dropDownItems[0][0]);

      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force));
      _selectedItems.Add(Force.GetAbbreviation(_forceUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string forceUnitAbbreviation = Force.GetAbbreviation(_forceUnit);
      pManager.AddGenericParameter("RebarGroup", "RbG", "AdSec Reinforcement Group to apply Preload to",
        GH_ParamAccess.item);
      pManager.AddGenericParameter($"Force [{forceUnitAbbreviation}]", "P",
        "The pre-load per reinforcement bar. Positive value is tension.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Prestressed RebarGroup", "RbG", "Preloaded Rebar Group for AdSec Section",
        GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // get rebargroup
      var rebar = this.GetReinforcementGroup(DA, 0);

      IPreload load = null;
      // Create new load
      switch (_selectedItems[0]) {
        case _selectedForceUnit:
          load = IPreForce.Create((Force)Input.UnitNumber(this, DA, 1, _forceUnit));
          break;

        case _selectedStrainUnit:
          load = IPreStrain.Create((Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
          break;

        case _selectedStressUnit:
          load = IPreStress.Create((Pressure)Input.UnitNumber(this, DA, 1, _stressUnit));
          break;
      }

      var longitudinal = (ILongitudinalGroup)rebar.Value.Group;
      longitudinal.Preload = load;
      var out_rebar = new AdSecRebarGroupGoo(longitudinal);
      if (rebar.Cover != null) {
        out_rebar.Cover = ICover.Create(rebar.Cover.UniformCover);
      }

      DA.SetData(0, out_rebar);

      AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
        "Applying prestress will change the up-stream (backwards) rebar object as well "
        + "- please make a copy of the input if you want to have both a rebar with and without prestress. "
        + "This will change in future releases, apologies for the inconvenience...");
    }
  }
}
