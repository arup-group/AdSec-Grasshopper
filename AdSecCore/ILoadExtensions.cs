using System;

using AdSecCore.Functions;

using Oasys.AdSec;
namespace AdSecCore.Extensions {
  public static class ILoadExtensions {

    public static bool IsValid(this ILoad load) {
      if (load == null) {
        return false;
      }
      var fx = Math.Abs(load.X.Value) > 0;
      var myy = Math.Abs(load.YY.Value) > 0;
      var mzz = Math.Abs(load.ZZ.Value) > 0;
      return fx || myy || mzz;
    }

    public static bool IsValid(this ILoad load, Function function) {
      if (!load.IsValid()) {
        function.ErrorMessages.Add("Deformation Input should be finite number. Zero deformation has no boundary");
        return false;
      }
      return true;
    }
  }
}
