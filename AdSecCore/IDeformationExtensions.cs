using System;

using AdSecCore.Functions;

using Oasys.AdSec;
namespace AdSecCore.Extensions {
  public static class IDeformationExtensions {

    public static bool IsValid(this IDeformation deformation) {
      if (deformation == null) {
        return false;
      }
      var axialDeformation = Math.Abs(deformation.X.Value) > 0;
      var curvatureYY = Math.Abs(deformation.YY.Value) > 0;
      var curvatureZZ = Math.Abs(deformation.ZZ.Value) > 0;
      return axialDeformation || curvatureYY || curvatureZZ;
    }

    public static bool IsValid(this IDeformation deformation, Function function) {
      if (!deformation.IsValid()) {
        function.ErrorMessages.Add("Deformation Input should be finite number. Zero deformation has no boundary");
        return false;
      }
      return true;
    }
  }
}
