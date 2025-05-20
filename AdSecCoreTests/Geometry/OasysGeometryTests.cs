using AdSecCore.Functions;

namespace AdSecCoreTests {
  public class OasysGeometryTests {
    [Fact]
    public void ShouldHaveOnlYAndZ() {
      var plane = OasysPlane.PlaneYZ;
      Assert.True(plane.XAxis.Equals(new OasysPoint() { X = 0, Y = 1, Z = 0 }));
      Assert.True(plane.YAxis.Equals(new OasysPoint() { X = 0, Y = 0, Z = 1 }));
    }

    [Fact]
    public void ShouldHaveOnlXAndY() {
      var plane = OasysPlane.PlaneXY;
      Assert.True(plane.XAxis.Equals(new OasysPoint() { X = 1, Y = 0, Z = 0 }));
      Assert.True(plane.YAxis.Equals(new OasysPoint() { X = 0, Y = 1, Z = 0 }));
    }

    [Fact]
    public void ShouldInstantiateAt000() {
      Assert.Equal(OasysPoint.Zero, new OasysPlane().Origin);
    }

    [Fact]
    public void ShouldReturnFalseForNonPlanes() {
      Assert.False(OasysPlane.PlaneXY.Equals(new OasysPoint()));
    }

    [Fact]
    public void ShouldReturnTrueForTheSamePlane() {
      Assert.True(OasysPlane.PlaneYZ.Equals(new OasysPlane() {
        XAxis = OasysPoint.YAxis,
        YAxis = OasysPoint.ZAxis,
      }));
    }

    [Fact]
    public void ShouldReturnFalseForTheSamePlane() {
      Assert.False(new OasysPoint(1, 2, 3).Equals(OasysPlane.PlaneYZ));
    }

    [Fact]
    public void ShouldMatchXAxis() {
      Assert.True(new OasysPoint(1, 0, 0).Equals(OasysPoint.XAxis));
    }

    [Fact]
    public void ShouldMatchYAxis() {
      Assert.True(new OasysPoint(0, 1, 0).Equals(OasysPoint.YAxis));
    }

    [Fact]
    public void ShouldMatchZAxis() {
      Assert.True(new OasysPoint(0, 0, 1).Equals(OasysPoint.ZAxis));
    }

    [Fact]
    public void ShouldNotMatchAnyComponent() {
      Assert.True(new OasysPoint(1, 2, 3).Equals(new OasysPoint(4, 5, 6)));
    }

    [Fact]
    public void ShouldReturnWrongAtTheLastZAxisZ() {
      var uniquePlane = new OasysPlane() {
        Origin = new OasysPoint(7, 8, 9),
        XAxis = new OasysPoint(1, 2, 3),
        YAxis = new OasysPoint(4, 5, 6),
      };

      var uniquePlane2 = new OasysPlane() {
        Origin = new OasysPoint(7, 8, 9),
        XAxis = new OasysPoint(1, 2, 3),
        YAxis = new OasysPoint(4, 5, 7),
      };

      Assert.False(uniquePlane.Equals(uniquePlane2));
    }

    [Fact]
    public void ShouldReturnFalseForTotallyWrongPlanes() {
      var uniquePlane = new OasysPlane() {
        Origin = new OasysPoint(7, 8, 9),
        XAxis = new OasysPoint(1, 2, 3),
        YAxis = new OasysPoint(4, 5, 6),
      };

      var uniquePlane2 = new OasysPlane() {
        Origin = new OasysPoint(17, 18, 19),
        XAxis = new OasysPoint(11, 12, 13),
        YAxis = new OasysPoint(14, 15, 17),
      };

      Assert.False(uniquePlane.Equals(uniquePlane2));
    }

    [Fact]
    public void ShouldHaveTheSameHashCode() {
      Assert.Equal(OasysPlane.PlaneYZ.GetHashCode(), new OasysPlane() {
        XAxis = OasysPoint.YAxis,
        YAxis = OasysPoint.ZAxis,
      }.GetHashCode());
    }

  }
}
