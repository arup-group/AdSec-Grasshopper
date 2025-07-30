
using System;


using Oasys.AdSec;
namespace AdSecCore.Extensions {
  public static class LoadExtensions {
    public static bool IsValid(this ILoad load) {
      if (load == null) {
        return false;
      }

      double fx = load.X.Value;
      double myy = load.YY.Value;
      double mzz = load.ZZ.Value;
      return NotAllZero(fx, myy, mzz);
    }

    public static bool IsValid(this IDeformation deformation) {
      if (deformation == null) {
        return false;
      }

      double axialDeformation = deformation.X.Value;
      double curvatureYY = deformation.YY.Value;
      double curvatureZZ = deformation.ZZ.Value;
      return NotAllZero(axialDeformation, curvatureYY, curvatureZZ);
    }

    public static bool NotAllZero(double x, double y, double z) {
      return Math.Abs(x) > 0 || Math.Abs(y) > 0 || Math.Abs(z) > 0;
    }

  }
}
