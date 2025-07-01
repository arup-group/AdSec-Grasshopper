using System;
using System.Collections.Generic;
using System.Linq;

using AdSecCore.Functions;

using AdSecGH.Helpers;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace Oasys.GH.Helpers {
  public abstract class DropdownAdapter<T> : GH_OasysDropDownComponent, IDefaultValues where T : IFunction {
    public readonly T BusinessComponent = Activator.CreateInstance<T>();
    private FunctionHandler<T> _handler;

    protected DropdownAdapter() : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) {
      InitHandler();

      if (BusinessComponent is IVariableInput variableInput) {
        variableInput.OnVariableInputChanged += UpdateInputs;
        UpdateInputs();
      }

      if (BusinessComponent is IDynamicDropdown dynamicDropdown) {
        dynamicDropdown.OnDropdownChanged += ProcessDropdownItems;
      }
    }

    private void InitHandler() {
      if (_handler == null) {
        _handler = new FunctionHandler<T>(BusinessComponent, this);
      }
    }

    private void UpdateInputs() {
      // Unregister All, but Keep the same ones, based on the name and re-register them (avoid wire disconnect)
      var previous = new Dictionary<string, IGH_Param>();
      for (int i = Params.Input.Count - 1; i >= 0; i--) {
        previous.Add(Params.Input[i].Name, Params.Input[i]);
        Params.UnregisterInputParameter(Params.Input[i], false);
      }

      _handler.PopulateInputParams(this, previous);
    }

    public override OasysPluginInfo PluginInfo { get; } = AdSecGH.PluginInfo.Instance;

    public void SetDefaultValues() {
      _handler.SetDefaultValues(this);
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      InitHandler();
      _handler.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      InitHandler();
      _handler.PopulateOutputParams(this);
    }

    public override void SetSelected(int i, int j) { }

    protected override void SolveInternal(IGH_DataAccess da) {
      _handler.UpdateInputValues(this, da);
      if (RuntimeMessages(GH_RuntimeMessageLevel.Error).Count > 0) {
        return;
      }

      _handler.Compute();
      _handler.UpdateMessages(this);

      if (_handler.HasErrors()) {
        return;
      }

      _handler.SetOutputValues(this, da);
    }

    protected override void InitialiseDropdowns() {
      InitHandler();
      ProcessDropdownItems();
      _isInitialised = true;
    }

    internal void ProcessDropdownItems() {
      if (!_isInitialised) {
        _spacerDescriptions = new List<string>();
        _dropDownItems = new List<List<string>>();
        _selectedItems = new List<string>();
      }

      _handler.UpdateDefaultUnits();
      if (BusinessComponent is IDropdownOptions dropdownOptions) {
        var options = dropdownOptions.Options();
        for (int i = 0; i < options.Length; i++) {
          var option = options[i];
          string description = option.Description;
          List<string> items = new List<string>();
          string selectedItem = string.Empty;

          if (option is EnumOptions enumOptions) {
            items = enumOptions.GetOptions().ToList();
            selectedItem = items.FirstOrDefault(x => x == enumOptions.Selected?.ToString()) ?? items[0];
          } else if (option is UnitOptions unitOptions) {
            var unitType = unitOptions.UnitType;
            var unitValue = unitOptions.UnitValue;
            items = UnitsHelper.GetFilteredAbbreviations(ToEngineeringUnits()[unitType]);
            selectedItem = UnitAbbreviation(unitType, unitValue);
          }

          if (_isInitialised && i < _dropDownItems.Count) {
            _spacerDescriptions[i] = description;
            _dropDownItems[i] = items;
            _selectedItems[i] = selectedItem;
          } else {
            _spacerDescriptions.Add(description);
            _dropDownItems.Add(items);
            _selectedItems.Add(selectedItem);
          }
        }

        // Remove any extra dropdowns that are not needed
        while (_dropDownItems.Count > options.Length) {
          _dropDownItems.RemoveAt(_dropDownItems.Count - 1);
          _selectedItems.RemoveAt(_selectedItems.Count - 1);
          _spacerDescriptions.RemoveAt(_spacerDescriptions.Count - 1);
        }
      }
    }

    internal virtual void SetLocalUnits() { }

    public override void VariableParameterMaintenance() {
      SetLocalUnits();
    }

    protected override void BeforeSolveInstance() {
      _handler.UpdateUnitsAndParameters(this);
    }

    public static Dictionary<Type, EngineeringUnits> ToEngineeringUnits() {
      return new Dictionary<Type, EngineeringUnits> {
        { typeof(LengthUnit), EngineeringUnits.Length },
        { typeof(AngleUnit), EngineeringUnits.Angle },
        { typeof(ForceUnit), EngineeringUnits.Force },
        { typeof(StrainUnit), EngineeringUnits.Strain },
        { typeof(PressureUnit), EngineeringUnits.Stress },
      };
    }

    public string UnitAbbreviation(Type unitType, int unitValue) {
      return OasysUnitsSetup.Default.UnitAbbreviations.GetDefaultAbbreviation(unitType, unitValue);
    }

    public void UpdateUnits() {
      _handler.UpdateDefaultUnits();
    }

    public void RefreshParameter(GH_Component component) {
      _handler.RefreshParameter(component);
    }
  }
}
