
using System.Drawing;

namespace AdSecCore {
  public static class ColorExtensions {
    public static bool IsRgbEqualTo(this Color a, Color b) {
      return a.R == b.R && a.G == b.G && a.B == b.B;
    }
  }
}
