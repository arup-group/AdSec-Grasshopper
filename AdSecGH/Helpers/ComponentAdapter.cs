using System;

using AdSecCore.Functions;

using AdSecGH.Components;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;
using OasysGH.Units;

using Attribute = AdSecCore.Functions.Attribute;

namespace Oasys.GH.Helpers {

  public abstract class ComponentAdapter<T> : GH_OasysComponent, IDefaultValues where T : Function {

    public readonly T BusinessComponent = Activator.CreateInstance<T>();

    protected ComponentAdapter() : base(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) {
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

    protected override void SolveInstance(IGH_DataAccess DA) {
      BusinessComponent.UpdateInputValues(this, DA);
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

      BusinessComponent.SetOutputValues(this, DA);
    }


    protected override void BeforeSolveInstance() {
      UpdateDefaultUnits();
      RefreshParameter();
    }

    public void UpdateDefaultUnits() {
      AdapterBase.UpdateDefaultUnits(BusinessComponent);
    }

    public void RefreshParameter() {
      AdapterBase.RefreshParameter(BusinessComponent, this.Params);
    }

  }

  public interface IDefaultValues {
    void SetDefaultValues();
  }

}
