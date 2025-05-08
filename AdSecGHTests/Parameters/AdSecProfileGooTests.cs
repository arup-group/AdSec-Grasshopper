using System;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;
using Rhino.Geometry.Morphs;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecProfileGooTests {
    private readonly AdSecProfileGoo _testGoo;

    public AdSecProfileGooTests() {
      var length = new Length(1, LengthUnit.Meter);
      var thickness = new Length(0.2, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(thickness, length),
        new WebConstant(thickness)));
      _testGoo = new AdSecProfileGoo(profile, Plane.WorldXY);
    }

    [Fact]
    public void AdSecProfileGoo_ThrowsError_WhenNullProfileDesignProvided() {
      Assert.Throws<ArgumentNullException>(() => new AdSecProfileGoo(null));
    }

    [Fact]
    public void AdSecProfileGoo_ThrowsError_WhenNullIProfileProvided() {
      Assert.Throws<ArgumentNullException>(() => new AdSecProfileGoo(null, Plane.Unset));
    }

    [Fact]
    public void AdSecProfileGoo_ThrowsError_WhenNullPolygonProvided() {
      Assert.Throws<ArgumentNullException>(() => new AdSecProfileGoo(null, LengthUnit.Inch));
    }

    [Fact]
    public void IsValid_ReturnsTrue_Always() { // as it is geometry
      Assert.True(_testGoo.IsValid);
    }

    [Fact]
    public void IsReflected_ReturnsValidValue() {
      Assert.Equal(_testGoo.Profile.IsReflectedY, _testGoo.IsReflectedY);
      Assert.Equal(_testGoo.Profile.IsReflectedZ, _testGoo.IsReflectedZ);
    }

    [Fact]
    public void Rotation_ReturnsValidValue() {
      Assert.Equal(_testGoo.Profile.Rotation, _testGoo.Rotation);
    }

    [Fact]
    public void DuplicateGeometry_ReturnsValidValue() {
      var result = _testGoo.DuplicateGeometry();
      Assert.Equal(result.ToString(), _testGoo.ToString());
      Assert.Equal(result.IsValid, _testGoo.IsValid);
    }

    [Fact]
    public void GetBoundingBox_ReturnsValidValue() {
      var result = _testGoo.GetBoundingBox(new Transform());
      Assert.Equal(_testGoo.Polyline.BoundingBox.Area, result.Area);
    }

    [Fact]
    public void Morph_ReturnsNull_Always() {
      var result = _testGoo.Morph(null);
      Assert.Null(result);

      var result2 = _testGoo.Morph(new FlowSpaceMorph(new PolylineCurve(), new PolylineCurve(), false));
      Assert.Null(result2);
    }

    [Fact]
    public void Transform_ReturnsNull_Always() {
      var result = _testGoo.Transform(new Transform());
      Assert.Null(result);

      var result2 = _testGoo.Transform(Transform.Unset);
      Assert.Null(result2);
    }

    [Fact]
    public void ScriptVariable_ReturnsProfile() {
      object result = _testGoo.ScriptVariable();
      Assert.NotNull(result);
      Assert.Equal(_testGoo.Profile, result);
    }

    [Fact]
    public void ToString_ReturnsProfileDescription() {
      object result = _testGoo.ToString();
      Assert.Equal(_testGoo.Profile.Description(), result);
    }

  }
}
