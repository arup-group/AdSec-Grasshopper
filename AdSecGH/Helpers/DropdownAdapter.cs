using System;
using System.Collections.Generic;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Components;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace Oasys.GH.Helpers {
  public abstract class DropdownAdapter<T> : GH_OasysDropDownComponent, IDefaultValues where T : IFunction {
    public readonly T BusinessComponent = Activator.CreateInstance<T>();

    protected DropdownAdapter() : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) {
      BusinessComponent.UpdateProperties(this);
      if (BusinessComponent is IVariableInput variableInput) {
        variableInput.OnVariableInputChanged += UpdateInputs;
        UpdateInputs();
      }
    }

    private void UpdateInputs() {
      // Unregister All, but Keep the same ones, based on the name and re-register them (avoid wire disconnect)
      var previous = new Dictionary<string, IGH_Param>();
      for (int i = Params.Input.Count - 1; i >= 0; i--) {
        previous.Add(Params.Input[i].Name, Params.Input[i]);
        Params.UnregisterInputParameter(Params.Input[i], false);
      }

      BusinessComponent.PopulateInputParams(this, previous);
    }

    public override OasysPluginInfo PluginInfo { get; } = AdSecGH.PluginInfo.Instance;
    public void SetDefaultValues() { BusinessComponent.SetDefaultValues(this); }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      BusinessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      BusinessComponent.PopulateOutputParams(this);
    }

    public override void SetSelected(int i, int j) { }

    protected override void SolveInternal(IGH_DataAccess da) {
      BusinessComponent.UpdateInputValues(this, da);
      BusinessComponent.Compute();
      BusinessComponent.SetOutputValues(this, da);
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>();
      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      UpdateDefaultUnits();
      if (BusinessComponent is IDropdownOptions dropdownOptions) {
        foreach (var option in dropdownOptions.Options()) {
          string description = string.Empty;
          var items = new List<string>();
          string selectedItem = string.Empty;

          if (option is EnumOptions enumOptions) {
            description = enumOptions.Description;
            items = enumOptions.GetOptions().ToList();
            selectedItem = items[0];
          } else if (option is UnitOptions unitOptions) {
            description = unitOptions.Description;
            var unitType = unitOptions.UnitType;
            var unitValue = unitOptions.UnitValue;
            items = UnitsHelper.GetFilteredAbbreviations(ToEngineeringUnits()[unitType]);
            selectedItem = UnitAbbreviation(unitType, unitValue);
          }
          _spacerDescriptions.Add(description);
          _dropDownItems.Add(items);
          _selectedItems.Add(selectedItem);
        }
      }
      _isInitialised = true;
    }

    internal virtual void SetLocalUnits() { }

    protected override void BeforeSolveInstance() {
      SetLocalUnits();
      UpdateDefaultUnits(); // In Case the user has updated units from the settings dialogue
      UpdateFromLocalUnits();
      RefreshParameter(); // Simply passing the function names into the GH names. As we have the logic to update the names on the Core
    }

    private void UpdateFromLocalUnits() {
      AdapterBase.UpdateFromLocalUnits(BusinessComponent);
    }

    public void UpdateDefaultUnits() {
      AdapterBase.UpdateDefaultUnits(BusinessComponent);
    }

    public void RefreshParameter() {
      AdapterBase.RefreshParameter(BusinessComponent, this.Params);
    }

    public static Dictionary<Type, EngineeringUnits> ToEngineeringUnits() {
      return new Dictionary<Type, EngineeringUnits> {
        { typeof(LengthUnit), EngineeringUnits.Length },
      };
    }

    public string UnitAbbreviation(Type unitType, int unitValue) {
      return OasysUnitsSetup.Default.UnitAbbreviations.GetDefaultAbbreviation(unitType, unitValue);
    }
  }
}
