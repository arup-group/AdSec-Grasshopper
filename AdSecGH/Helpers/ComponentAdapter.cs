using System;

using AdSecCore.Functions;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;

namespace Oasys.GH.Helpers {

  public abstract class ComponentAdapter<T> : GH_OasysComponent, IDefaultValues where T : IFunction {

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
      BusinessComponent.Compute();
      if (BusinessComponent is Function function) {
        foreach (var warning in function.WarningMessages) {
          AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning);
        }
      }

      BusinessComponent.SetOutputValues(this, DA);
    }
  }

  public interface IDefaultValues {
    void SetDefaultValues();
  }

}
