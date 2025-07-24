using System;

using AdSecCore.Functions;

using AdSecGH.Components;

using OasysGH;
using OasysGH.Components;

namespace Oasys.GH.Helpers {

  public abstract class ProfileAdapter<T> : CreateOasysProfile where T : Function {
    public readonly T BusinessComponent = Activator.CreateInstance<T>();

    protected ProfileAdapter() : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) {
      BusinessComponent.UpdateProperties(this);
    }

    public override OasysPluginInfo PluginInfo { get; } = AdSecGH.PluginInfo.Instance;

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      BusinessComponent.PopulateInputParams(this);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      BusinessComponent.PopulateOutputParams(this);
    }

    internal virtual void SetLocalUnits() { }

    public override void VariableParameterMaintenance() {
      SetLocalUnits();
      base.VariableParameterMaintenance();
    }

    protected override void BeforeSolveInstance() {
      UpdateDefaultUnits(); // In Case the user has updated units from the settings dialogue
      UpdateFromLocalUnits();
    }

    private void UpdateFromLocalUnits() {
      AdapterBase.UpdateFromLocalUnits(BusinessComponent);
    }

    public void UpdateDefaultUnits() {
      AdapterBase.UpdateDefaultUnits(BusinessComponent);
    }
  }
}
