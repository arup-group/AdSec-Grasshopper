using System;

using AdSecCore.Functions;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.Components;

namespace AdSecGH.Helpers {
  public abstract class ProfileAdapter<T> : CreateOasysProfile, IDefaultValues where T : IFunction {
    public readonly T BusinessComponent = Activator.CreateInstance<T>();
    private FunctionHandler<T> _handler;

    protected ProfileAdapter() : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) {
      InitHandler();
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

    internal virtual void SetLocalUnits() { }

    public override void VariableParameterMaintenance() {
      SetLocalUnits();
      base.VariableParameterMaintenance();
    }

    protected override void BeforeSolveInstance() {
      _handler.UpdateUnitsAndParameters(this);
    }

    private void InitHandler() {
      if (_handler == null) {
        _handler = new FunctionHandler<T>(BusinessComponent, this);
      }
    }
  }
}
