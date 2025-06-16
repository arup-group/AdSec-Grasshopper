using System;
using System.Collections.Generic;

using AdSecCore.Functions;

using AdSecGH.Components;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;
using OasysGH.Units.Helpers;

using OasysUnits;
using OasysUnits.Units;

namespace Oasys.GH.Helpers {
  public abstract class ProfileAdapter<T> : CreateOasysProfile, IDefaultValues where T : IFunction {
    public readonly T BusinessComponent = Activator.CreateInstance<T>();

    protected ProfileAdapter() : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) {
      BusinessComponent.UpdateProperties(this);
    }

    public override OasysPluginInfo PluginInfo { get; } = AdSecGH.PluginInfo.Instance;
    public void SetDefaultValues() { BusinessComponent.SetDefaultValues(this); }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      BusinessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      BusinessComponent.PopulateOutputParams(this);
    }

    protected override void SolveInternal(IGH_DataAccess da) {
      BusinessComponent.UpdateInputValues(this, da);
      if (RuntimeMessages(GH_RuntimeMessageLevel.Error).Count > 0) {
        return;
      }

      BusinessComponent.Compute();
      if (BusinessComponent is Function function) {
        AdapterBase.UpdateMessages(function, this);

        if (function.ErrorMessages.Count > 0) {
          return;
        }
      }

      BusinessComponent.SetOutputValues(this, da);
    }

    internal virtual void SetLocalUnits() { }

    public override void VariableParameterMaintenance() {
      SetLocalUnits();
      base.VariableParameterMaintenance();
    }

    protected override void BeforeSolveInstance() {
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
      AdapterBase.RefreshParameter(BusinessComponent, Params);
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
  }
}
