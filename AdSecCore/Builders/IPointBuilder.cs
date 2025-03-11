using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Builders {
  public static class IPointBuilder {
    public static IPoint InMillimeters(double width = 100, double height = 100) {
      return IPoint.Create(Length.FromMillimeters(width), Length.FromMillimeters(height));
    }
  }
}
