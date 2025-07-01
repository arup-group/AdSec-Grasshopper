using System.Collections.Generic;

using AdSecCore.Functions;

using AdSecGH.Components;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

namespace AdSecGH.Helpers {
  public class FunctionHandler<T> where T : IFunction {
    public readonly T BusinessComponent;

    public FunctionHandler(T instance, GH_Component componentContext) {
      BusinessComponent = instance;
      BusinessComponent.UpdateProperties(componentContext);
    }

    public void SetDefaultValues(GH_Component componentContext) {
      BusinessComponent.SetDefaultValues(componentContext);
    }

    public void PopulateInputParams(GH_Component component) {
      BusinessComponent.PopulateInputParams(component);
    }

    public void PopulateInputParams(GH_Component component, Dictionary<string, IGH_Param> previous) {
      BusinessComponent.PopulateInputParams(component, previous);
    }

    public void PopulateOutputParams(GH_Component component) {
      BusinessComponent.PopulateOutputParams(component);
    }

    public void UpdateInputValues(GH_Component component, IGH_DataAccess da) {
      BusinessComponent.UpdateInputValues(component, da);
    }

    public void Compute() {
      BusinessComponent.Compute();
    }

    public void SetOutputValues(GH_Component component, IGH_DataAccess da) {
      BusinessComponent.SetOutputValues(component, da);
    }

    public void UpdateMessages(GH_Component component) {
      if (BusinessComponent is Function function) {
        AdapterBase.UpdateMessages(function, component);
      }
    }

    public bool HasErrors() {
      return BusinessComponent is Function function && function.ErrorMessages.Count > 0;
    }

    public void UpdateUnitsAndParameters(GH_Component adapter) {
      UpdateDefaultUnits();
      UpdateFromLocalUnits();
      RefreshParameter(adapter);
    }

    public void UpdateFromLocalUnits() {
      AdapterBase.UpdateFromLocalUnits(BusinessComponent);
    }

    public void UpdateDefaultUnits() {
      AdapterBase.UpdateDefaultUnits(BusinessComponent);
    }

    public void RefreshParameter(GH_Component component) {
      AdapterBase.RefreshParameter(BusinessComponent, component.Params);
    }
  }

}
