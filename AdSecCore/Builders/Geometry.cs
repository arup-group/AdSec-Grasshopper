using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Helpers {
  public class Geometry {
    public static IPoint Zero() {
      return IPoint.Create(Length.Zero, Length.Zero);
    }
  }
}
