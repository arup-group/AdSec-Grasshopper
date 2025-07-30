
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {
  public static class Geometry {
    public static IPoint Zero() {
      return IPoint.Create(Length.Zero, Length.Zero);
    }
    public static IPoint Position(double x, double y) {
      return IPoint.Create(Length.FromCentimeters(x), Length.FromCentimeters(y));
    }
  }
}
