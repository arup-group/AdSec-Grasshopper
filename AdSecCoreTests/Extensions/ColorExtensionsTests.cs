using System.Drawing;

using AdSecCore;

namespace AdSecCoreTests.Extensions {
  public class ColorExtensionsTests {
    [Fact]
    public void IsRgbEqualTo_SameRgbDifferentAlpha_ReturnsTrue() {
      var color1 = Color.FromArgb(2, 1, 2, 3);
      var color2 = Color.FromArgb(1, 1, 2, 3);

      Assert.True(color1.IsRgbEqualTo(color2));
    }

    [Fact]
    public void IsRgbEqualTo_DifferentRgb_ReturnsFalse() {
      var color1 = Color.FromArgb(1, 2, 3, 5);
      var color2 = Color.FromArgb(1, 2, 4, 5);

      Assert.False(color1.IsRgbEqualTo(color2));
    }
  }
}
