using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {
  public static class Geometry {
    public static IPoint Zero() {
      return IPoint.Create(Length.Zero, Length.Zero);
    }
  }
}
