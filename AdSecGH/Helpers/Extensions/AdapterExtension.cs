using System.Collections.Generic;

using AdSecCore.Functions;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysGH.Units;

namespace AdSecGH.Helpers {
  public static class AdapterExtension {

    private static void UpdateDefaultUnits(Function businessComponent) {
      if (businessComponent != null) {
        businessComponent.MomentUnit = DefaultUnits.MomentUnit;
        businessComponent.LengthUnit = DefaultUnits.LengthUnitGeometry;
        businessComponent.StrainUnitResult = DefaultUnits.StrainUnitResult;
        businessComponent.StressUnitResult = DefaultUnits.StressUnitResult;
        businessComponent.CurvatureUnit = DefaultUnits.CurvatureUnit;
        businessComponent.LengthUnitResult = DefaultUnits.LengthUnitResult;
        businessComponent.AxialStiffnessUnit = DefaultUnits.AxialStiffnessUnit;
        businessComponent.BendingStiffnessUnit = DefaultUnits.BendingStiffnessUnit;
      }
    }

    private static void RefreshParams(List<IGH_Param> parameters, Attribute[] attributes) {
      for (int id = 0; id < attributes.Length; id++) {
        parameters[id].Description = attributes[id].Description;
        parameters[id].Name = attributes[id].Name;
      }
    }

    private static void RefreshOutputParameter<T>(T owner) where T : IGH_Component {
      if (owner is IGH_Component component) {
        var outputAttributes = (owner as dynamic).BusinessComponent.GetAllOutputAttributes();
        RefreshParams(component.Params.Output, outputAttributes);
      }
    }

    private static void RefreshInputParameter<T>(T owner) where T : IGH_Component {
      if (owner is IGH_Component component) {
        var inputAttributes = (owner as dynamic).BusinessComponent.GetAllInputAttributes();
        RefreshParams(component.Params.Input, inputAttributes);
      }
    }

    public static void UpdateDefaultUnits<T>(this DropdownAdapter<T> owner) where T : Function, new() {
      UpdateDefaultUnits(owner.BusinessComponent);
    }

    public static void UpdateDefaultUnits<T>(this ComponentAdapter<T> owner) where T : Function, new() {
      UpdateDefaultUnits(owner.BusinessComponent);
    }

    public static void RefreshParameter<T>(this DropdownAdapter<T> owner) where T : Function, new() {
      owner.BusinessComponent.UpdateParameter();
      RefreshOutputParameter(owner);
      RefreshInputParameter(owner);
    }

    public static void RefreshParameter<T>(this ComponentAdapter<T> owner) where T : Function, new() {
      owner.BusinessComponent.UpdateParameter();
      RefreshOutputParameter(owner);
      RefreshInputParameter(owner);
    }
  }
}
