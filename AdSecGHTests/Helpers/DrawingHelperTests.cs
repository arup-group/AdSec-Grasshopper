using System;
using System.Drawing;

using Grasshopper.Kernel;

using Rhino.Geometry;

using Xunit;

namespace AdSecGH.Helpers.Tests {
  [Collection("GrasshopperFixture collection")]
  public class DrawingHelperTests {
    [Fact]
    public void TestLightRedColor() {
      var expected = Color.FromArgb(255, 244, 96, 96);
      var actual = DrawingHelper.LightRed;

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestLightGreenColor() {
      var expected = Color.FromArgb(255, 96, 244, 96);
      var actual = DrawingHelper.LightGreen;

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestLightBlueColor() {
      var expected = Color.FromArgb(255, 96, 96, 234);
      var actual = DrawingHelper.LightBlue;

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestDrawLocalAxisThrowsArgumentNullExceptionIfArgsNull() {
      GH_PreviewWireArgs args = null;
      var lineX = new Line(new Point3d(0, 0, 0), new Point3d(1, 0, 0));
      var lineY = new Line(new Point3d(0, 0, 0), new Point3d(0, 1, 0));
      var lineZ = new Line(new Point3d(0, 0, 0), new Point3d(0, 0, 1));

      var exception
        = Assert.Throws<ArgumentNullException>(() => DrawingHelper.DrawLocalAxis(args, lineX, lineY, lineZ));
      Assert.Equal("args", exception.ParamName);
    }

  }
}
