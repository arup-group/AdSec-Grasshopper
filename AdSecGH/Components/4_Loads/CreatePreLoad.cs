using System;
using System.Collections.Generic;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
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
  /// Component to create a new Stress Strain Point
  /// </summary>
  public class CreatePreLoad : GH_OasysDropDownComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("cbab2b12-2a01-4f05-ba24-2c79827c7415");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Prestress;
    private ForceUnit _forceUnit = DefaultUnits.ForceUnit;
    private StrainUnit _strainUnit = DefaultUnits.MaterialStrainUnit;
    private PressureUnit _stressUnit = DefaultUnits.StressUnitResult;

    public CreatePreLoad() : base(
      "Create Prestress",
      "Prestress",
      "Create an AdSec Prestress Load for Reinforcement Layout as either Preforce, Prestrain or Prestress",
      CategoryName.Name(),
      SubCategoryName.Cat5()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader) {
      _forceUnit = (ForceUnit)Enum.Parse(typeof(ForceUnit), reader.GetString("force"));
      _strainUnit = (StrainUnit)Enum.Parse(typeof(StrainUnit), reader.GetString("strain"));
      _stressUnit = (PressureUnit)Enum.Parse(typeof(PressureUnit), reader.GetString("stress"));
      return base.Read(reader);
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      if (i == 0) {
        switch (_selectedItems[0]) {
          case "Force":
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force);
            _selectedItems[0] = _forceUnit.ToString();
            break;

          case "Strain":
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Strain);
            _selectedItems[0] = _strainUnit.ToString();
            break;

          case "Stress":
            _dropDownItems[1] = UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Stress);
            _selectedItems[0] = _stressUnit.ToString();
            break;
        }
      } else {
        switch (_selectedItems[0]) {
          case "Force":
            _forceUnit = (ForceUnit)Enum.Parse(typeof(ForceUnit), _selectedItems[i]);
            break;

          case "Strain":
            _strainUnit = (StrainUnit)Enum.Parse(typeof(StrainUnit), _selectedItems[i]);
            break;

          case "Stress":
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
        case "Force":
          Params.Input[1].Name = "Force [" + forceUnitAbbreviation + "]";
          Params.Input[1].NickName = "P";
          break;

        case "Strain":
          Params.Input[1].Name = "Strain [" + strainUnitAbbreviation + "]";
          Params.Input[1].NickName = "ε";
          break;

        case "Stress":
          Params.Input[1].Name = "Stress [" + stressUnitAbbreviation + "]";
          Params.Input[1].NickName = "σ";
          break;
      }
    }

    public override bool Write(GH_IO.Serialization.GH_IWriter writer) {
      writer.SetString("force", _forceUnit.ToString());
      writer.SetString("strain", _strainUnit.ToString());
      writer.SetString("stress", _stressUnit.ToString());
      return base.Write(writer);
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>() {
        "Force",
        "Strain",
        "Stress"
      };

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      // type
      var types = new List<string>() { "Force", "Strain", "Stress" };
      _dropDownItems.Add(types);
      _selectedItems.Add(_dropDownItems[0][0]);

      // force
      _dropDownItems.Add(UnitsHelper.GetFilteredAbbreviations(EngineeringUnits.Force));
      _selectedItems.Add(Force.GetAbbreviation(_forceUnit));

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      string forceUnitAbbreviation = Force.GetAbbreviation(_forceUnit);
      pManager.AddGenericParameter("RebarGroup", "RbG", "AdSec Reinforcement Group to apply Preload to", GH_ParamAccess.item);
      pManager.AddGenericParameter("Force [" + forceUnitAbbreviation + "]", "P", "The pre-load per reinforcement bar. Positive value is tension.", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Prestressed RebarGroup", "RbG", "Preloaded Rebar Group for AdSec Section", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // get rebargroup
      AdSecRebarGroupGoo rebar = AdSecInput.ReinforcementGroup(this, DA, 0);

      IPreload load = null;
      // Create new load
      switch (_selectedItems[0]) {
        case "Force":
          load = IPreForce.Create((Force)Input.UnitNumber(this, DA, 1, _forceUnit));
          break;

        case "Strain":
          load = IPreStrain.Create((Strain)Input.UnitNumber(this, DA, 1, _strainUnit));
          break;

        case "Stress":
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

      AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Applying prestress will change the up-stream (backwards) rebar object as well " +
          "- please make a copy of the input if you want to have both a rebar with and without prestress. " +
          "This will change in future releases, apologies for the inconvenience...");
    }
  }
}
