using System;
using System.Diagnostics.CodeAnalysis;

using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysUnits;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  [SuppressMessage("Major Bug", "S4143:Collection elements should not be replaced unconditionally",
    Justification = "<Pending>")]
  public class AdSecLoadGooTests {
    private readonly AdSecLoadGoo _testGoo;

    public AdSecLoadGooTests() {
      _testGoo = new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment()), Plane.WorldXY);
    }

    [Fact]
    public void IsValidReturnTrueForValidLoad() {
      Assert.True(_testGoo.IsValid);
    }

    [Fact]
    public void Duplicate_ReturnsNewInstance_WhenIsValidIsTrue() {
      var result = (AdSecLoadGoo)_testGoo.Duplicate();
      Assert.NotNull(result);
      Assert.True(result.Boundingbox.IsValid);
      Assert.Equal(_testGoo.Value, result.Value);
    }

    [Fact]
    public void AdSecLoadGooCreatedWithNullLoadWillThrowException() {
      Assert.Throws<ArgumentNullException>(() => new AdSecLoadGoo(null));
    }

    [Fact]
    public void LoadIsValidWhenPlaneNotSpecified() {
      var loadGoo = new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment()));
      Assert.True(loadGoo.IsValid);
      Assert.True(loadGoo.Boundingbox.IsValid);
    }

    [Fact]
    public void LoadIsValidWhenPlaneIsSetToUnset() {
      var loadGoo = new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment()), Plane.Unset);
      Assert.True(loadGoo.IsValid);
      Assert.True(loadGoo.Boundingbox.IsValid);
    }


    [Fact]
    public void DuplicateGeometry_ReturnsNewInstance_WhenIsValidIsTrue() {
      var result = (AdSecLoadGoo)_testGoo.DuplicateGeometry();
      Assert.NotNull(result);
      Assert.True(result.Boundingbox.IsValid);
      Assert.Equal(_testGoo.Value, result.Value);
    }

    [Fact]
    public void GetBoundingBox_ReturnsNonEmpty_WhenIsValidIsTrue() {
      var result = _testGoo.GetBoundingBox(Transform.Translation(1, 0, 0));
      Assert.True(result.IsValid);
    }

    [Fact]
    public void Morph_ReturnsNull_Always() {
      var result = _testGoo.Morph(new TestSpaceMorph());
      Assert.Null(result);
    }

    [Fact]
    public void Transform_ReturnsNull_Always() {
      var result = _testGoo.Transform(Transform.Identity);
      Assert.Null(result);
    }

    private class TestSpaceMorph : SpaceMorph {
      public override Point3d MorphPoint(Point3d point) {
        return new Point3d(point.X + 1, point.Y, point.Z);
      }
    }

    [Fact]
    public void CastFromTest() {
      var loadGoo = new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment()));
      Assert.False(loadGoo.CastFrom(null));
      Assert.True(loadGoo.CastFrom(new Line()));
      Assert.True(loadGoo.CastFrom(new Point3d()));
      Assert.True(loadGoo.CastFrom(new GH_Point()));
    }

    [Fact]
    public void CastToTest() {
      var adsecLoad = ILoad.Create(new Force(), new Moment(), new Moment());
      var loadGoo = new AdSecLoadGoo(adsecLoad);

      AdSecLoadGoo load = new AdSecLoadGoo(adsecLoad);
      Assert.True(loadGoo.CastTo<AdSecLoadGoo>(ref load));
      Assert.True(load.IsValid);

      Point3d point = Point3d.Unset;
      Assert.True(loadGoo.CastTo<Point3d>(ref point));
      Assert.True(point.IsValid);

      GH_Point ghPoint = null;
      Assert.True(loadGoo.CastTo<GH_Point>(ref ghPoint));
      Assert.NotNull(ghPoint);

      Line line = Line.Unset;
      Assert.False(loadGoo.CastTo<Line>(ref line));
      Assert.False(line.IsValid);

    }
  }
}
