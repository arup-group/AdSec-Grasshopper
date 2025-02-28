using AdSecCore.Functions;

using AdSecGH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class CoreToGhExtensionsTests {
    [Fact]
    public void ShouldConvertPlane() {
      // Arrange
      var plane = new OasysPlane {
        Origin = new OasysPoint { X = 1, Y = 2, Z = 3 },
        XAxis = new OasysPoint { X = 1, Y = 0, Z = 0 },
        YAxis = new OasysPoint { X = 0, Y = 0, Z = 1 }
      };

      // Act
      var result = plane.ToGh();

      // Assert
      Assert.Equal(1, result.Origin.X);
      Assert.Equal(2, result.Origin.Y);
      Assert.Equal(3, result.Origin.Z);
      Assert.Equal(1, result.XAxis.X);
      Assert.Equal(0, result.XAxis.Y);
      Assert.Equal(0, result.XAxis.Z);
      Assert.Equal(0, result.YAxis.X);
      Assert.Equal(0, result.YAxis.Y);
      Assert.Equal(1, result.YAxis.Z);
    }
  }
}
