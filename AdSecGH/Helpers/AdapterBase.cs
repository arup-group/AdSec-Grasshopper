using System.Collections.Generic;

using AdSecCore.Functions;

using Grasshopper.Kernel;

using OasysGH.Units;

namespace AdSecGH.Components {
  public class AdapterBase {
    private readonly Function _businessComponent;
    private readonly GH_Component _component;

    public AdapterBase(Function businessComponent, GH_Component component) {
      _businessComponent = businessComponent;
      _component = component;
    }

    public void UpdateDefaultUnits() {
      if (_businessComponent == null) {
        return;
      }

      _businessComponent.MomentUnit = DefaultUnits.MomentUnit;
      _businessComponent.LengthUnitGeometry = DefaultUnits.LengthUnitGeometry;
      _businessComponent.StrainUnitResult = DefaultUnits.StrainUnitResult;
      _businessComponent.StressUnitResult = DefaultUnits.StressUnitResult;
      _businessComponent.CurvatureUnit = DefaultUnits.CurvatureUnit;
      _businessComponent.LengthUnitResult = DefaultUnits.LengthUnitResult;
      _businessComponent.AxialStiffnessUnit = DefaultUnits.AxialStiffnessUnit;
      _businessComponent.BendingStiffnessUnit = DefaultUnits.BendingStiffnessUnit;
    }

    public void RefreshParameter() {
      if (_businessComponent == null) {
        return;
      }
      _businessComponent.UpdateParameter();
      RefreshParams(_component.Params.Input, _businessComponent.GetAllInputAttributes());
      RefreshParams(_component.Params.Output, _businessComponent.GetAllOutputAttributes());
    }

    private static void RefreshParams(List<IGH_Param> parameters, Attribute[] attributes) {
      for (int i = 0; i < parameters.Count; i++) {
        parameters[i].Name = attributes[i].Name;
        parameters[i].NickName = attributes[i].NickName;
        parameters[i].Description = attributes[i].Description;
      }
    }
  }
}
