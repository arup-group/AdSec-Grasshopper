using System.Collections.Generic;

using AdSecCore.Functions;

using Grasshopper.Kernel;

using OasysGH.Units;

namespace AdSecGH.Components {
  public class AdapterBase {

    private AdapterBase() {
    }

    public static void UpdateDefaultUnits(Function businessComponent) {
      if (businessComponent == null) {
        return;
      }

      businessComponent.MomentUnit = DefaultUnits.MomentUnit;
      businessComponent.LengthUnit = DefaultUnits.LengthUnitGeometry;
      businessComponent.StrainUnitResult = DefaultUnits.StrainUnitResult;
      businessComponent.StressUnitResult = DefaultUnits.StressUnitResult;
      businessComponent.CurvatureUnit = DefaultUnits.CurvatureUnit;
      businessComponent.LengthUnitResult = DefaultUnits.LengthUnitResult;
      businessComponent.AxialStiffnessUnit = DefaultUnits.AxialStiffnessUnit;
      businessComponent.BendingStiffnessUnit = DefaultUnits.BendingStiffnessUnit;
    }

    public static void RefreshParams(List<IGH_Param> parameters, Attribute[] attributes) {
      for (int id = 0; id < attributes.Length; id++) {
        parameters[id].Description = attributes[id].Description;
        parameters[id].Name = attributes[id].Name;
      }
    }
  }
}
