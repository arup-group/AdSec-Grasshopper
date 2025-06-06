using AdSecGH.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AxisHelperTests {
    [Fact]
    public void ShouldReturnLocalAxisLinesOfExpectedLength() {
      var profile = AdSecProfiles.CreateProfile(SampleProfiles.GetABeamProfile());
      var plane = Plane.WorldXY;
      var (x, y, z) = AxisHelper.GetLocalAxisLines(profile, plane);

      double expectedLength = 0.108; //defaultUnits

      Assert.Equal(expectedLength, x.Length, 3);
      Assert.Equal(expectedLength, y.Length, 3);
      Assert.Equal(expectedLength, z.Length, 3);
    }
  }

}
