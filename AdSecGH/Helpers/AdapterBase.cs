﻿using System.Collections.Generic;

using AdSecCore.Functions;

using Grasshopper.Kernel;

using OasysGH.Units;

namespace AdSecGH.Components {
  public class AdapterBase {
    protected AdapterBase() { }

    public static void UpdateDefaultUnits<T>(T BusinessComponent) {
      if (BusinessComponent is Function function) {
        function.MomentUnit = DefaultUnits.MomentUnit;
        function.ForceUnit = DefaultUnits.ForceUnit;
        function.LengthUnitGeometry = DefaultUnits.LengthUnitGeometry;
        function.StrainUnitResult = DefaultUnits.StrainUnitResult;
        function.StressUnitResult = DefaultUnits.StressUnitResult;
        function.CurvatureUnit = DefaultUnits.CurvatureUnit;
        function.LengthUnitResult = DefaultUnits.LengthUnitResult;
        function.AxialStiffnessUnit = DefaultUnits.AxialStiffnessUnit;
        function.BendingStiffnessUnit = DefaultUnits.BendingStiffnessUnit;
      }
    }

    public static void RefreshParameter<T>(T BusinessComponent, GH_ComponentParamServer parameter) {
      if (BusinessComponent is Function function) {
        RefreshParams(parameter.Input, function.GetAllInputAttributes());
        RefreshParams(parameter.Output, function.GetAllOutputAttributes());
      }

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
