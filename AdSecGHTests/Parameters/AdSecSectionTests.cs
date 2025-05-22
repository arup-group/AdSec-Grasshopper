using System;
using System.Collections.Generic;
using System.IO;

using AdSecCore;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.IO.Serialization;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.Taxonomy.Geometry;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

using IPerimeterProfile = Oasys.Profiles.IPerimeterProfile;
using IPolygon = Oasys.Taxonomy.Geometry.IPolygon;
using Point2d = Oasys.Taxonomy.Geometry.Point2d;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionTests {
    private readonly AdSecSection _sectionTest = null;

    public AdSecSectionTests() {
      var section = CreateSection();
      var designCode = EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013;
      _sectionTest = new AdSecSection(section, designCode, "test1", "test2", Plane.WorldXY);
    }

    [Fact]
    public void SerialiseUnflattenedSectionTest() {
      var section = CreateSection();

      var designCode = EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013;
      var adSec = IAdSec.Create(designCode);

      var flattened = adSec.Flatten(section);

      string fileName = $"{Path.GetTempPath()}AdSecSectionTest.ads";
      File.WriteAllText(fileName, CreateJson(designCode, section));

      string json = File.ReadAllText(fileName);
      var jsonParser = JsonParser.Deserialize(json);
      var actualSection = jsonParser.Sections[0];

      var expectedProfile = (IPerimeterProfile)flattened.Profile;
      var actualProfile = (IPerimeterProfile)actualSection.Profile;

      TestFlattenedSection(flattened, actualSection);
    }

    [Fact]
    public void SerialiaseFlattenedSectionTest() {
      var section = CreateSection();

      var designCode = EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013;
      var adSec = IAdSec.Create(designCode);
      var flattened = adSec.Flatten(section);

      string fileName = $"{Path.GetTempPath()}AdSecSectionTest.ads";
      File.WriteAllText(fileName, CreateJson(designCode, flattened));

      string json = File.ReadAllText(fileName);
      var jsonParser = JsonParser.Deserialize(json);
      var actualSection = jsonParser.Sections[0];

      var expectedProfile = (IPerimeterProfile)flattened.Profile;
      var actualProfile = (IPerimeterProfile)actualSection.Profile;

      TestFlattenedSection(flattened, actualSection);
    }

    [Fact]
    public void Duplicate_ReturnGoo_WhenIsValid() {
      var duplicate = _sectionTest.Duplicate();
      Assert.NotNull(duplicate);
      Assert.Equal(_sectionTest.IsValid, duplicate.IsValid);
      Assert.Equal(_sectionTest.DesignCode, duplicate.DesignCode);
      Assert.Equal(_sectionTest._materialName, duplicate._materialName);
      Assert.Equal(_sectionTest.LocalPlane, duplicate.LocalPlane);
      Assert.Equal(_sectionTest.Section.ToString(), duplicate.Section.ToString());
    }

    [Fact]
    public void Duplicate_ReturnNull_WhenIsValidIsFalse() {
      var duplicate = new AdSecSectionDummy().Duplicate();
      Assert.Null(duplicate);
    }

    [Fact]
    public void ToString_ReturnProfileDescription() {
      string result = _sectionTest.ToString();
      Assert.Equal(_sectionTest.Section.Profile.Description(), result);
    }

    private void TestFlattenedSection(ISection expected, ISection actual) {
      //A flattened section does not guarantee position points to be the same, as those sections may have positions relative
      //to different origin sets. Additionally, the flattened section splits the bundle of bars into single bars.
      var expectedProfile = (IPerimeterProfile)expected.Profile;
      var actualProfile = (IPerimeterProfile)actual.Profile;
      Assert.Equal(expectedProfile.Area().Value, actualProfile.Area().Value, new DoubleComparer());
      double actualBarArea = 0;
      foreach (var group in actual.ReinforcementGroups) {
        var singleBar = group as ISingleBars;
        actualBarArea += Math.PI * Math.Pow(singleBar.BarBundle.Diameter.Value / 2.0, 2)
          * singleBar.BarBundle.CountPerBundle;
      }

      double expectedBarArea = 0;
      foreach (var group in expected.ReinforcementGroups) {
        var singleBar = group as ISingleBars;
        expectedBarArea += Math.PI * Math.Pow(singleBar.BarBundle.Diameter.Value / 2.0, 2)
          * singleBar.BarBundle.CountPerBundle;
      }

      Assert.Equal(expectedBarArea, actualBarArea, new DoubleComparer());

      Assert.Equal(expectedProfile.ElasticModulus().Y.Value, actualProfile.ElasticModulus().Y.Value,
        new DoubleComparer());
      Assert.Equal(expectedProfile.ElasticModulus().Z.Value, actualProfile.ElasticModulus().Z.Value,
        new DoubleComparer());
      Assert.Equal(expectedProfile.SurfaceAreaPerUnitLength().Value, actualProfile.SurfaceAreaPerUnitLength().Value,
        new DoubleComparer());
      Assert.Equal(expectedProfile.LocalAxisSecondMomentOfArea().YY.Value,
        actualProfile.LocalAxisSecondMomentOfArea().YY.Value, new DoubleComparer());
      Assert.Equal(expectedProfile.LocalAxisSecondMomentOfArea().ZZ.Value,
        actualProfile.LocalAxisSecondMomentOfArea().ZZ.Value, new DoubleComparer());
      Assert.Equal(expectedProfile.LocalAxisSecondMomentOfArea().YZ.Value,
        actualProfile.LocalAxisSecondMomentOfArea().YZ.Value, new DoubleComparer());
      Assert.Equal(expectedProfile.PrincipalAxisSecondMomentOfArea().UU.Value,
        actualProfile.PrincipalAxisSecondMomentOfArea().UU.Value, new DoubleComparer());
      Assert.Equal(expectedProfile.PrincipalAxisSecondMomentOfArea().VV.Value,
        actualProfile.PrincipalAxisSecondMomentOfArea().VV.Value, new DoubleComparer());
    }

    private static string CreateJson(IDesignCode designCode, ISection section) {
      var json = new JsonConverter(designCode);
      return json.SectionToJson(section);
    }

    private static ISection CreateSection() {
      IMaterial material = Concrete.EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013.C40_50;
      var rebarMaterial = Reinforcement.Steel.EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013.S500B;

      var points = new List<IPoint2d> {
        new Point2d(new Length(3.1464410643837, LengthUnit.Meter), new Length(2.9552083887352, LengthUnit.Meter)),
        new Point2d(new Length(-0.6535618090254, LengthUnit.Meter), new Length(2.9552083887352, LengthUnit.Meter)),
        new Point2d(new Length(-0.6535618090254, LengthUnit.Meter), new Length(-1.2447891504457, LengthUnit.Meter)),
        new Point2d(new Length(5.1964353178991, LengthUnit.Meter), new Length(-1.2447891504457, LengthUnit.Meter)),
        new Point2d(new Length(5.1964353178991, LengthUnit.Meter), new Length(2.9552083911969, LengthUnit.Meter)),
        new Point2d(new Length(5.1964410643837, LengthUnit.Meter), new Length(3.2552083887352, LengthUnit.Meter)),
        new Point2d(new Length(6.2464410643837, LengthUnit.Meter), new Length(3.2551587793664, LengthUnit.Meter)),
        new Point2d(new Length(6.2464410643837, LengthUnit.Meter), new Length(-2.9947891504454, LengthUnit.Meter)),
        new Point2d(new Length(5.1964410643837, LengthUnit.Meter), new Length(-2.9947891504454, LengthUnit.Meter)),
        new Point2d(new Length(5.1964410643837, LengthUnit.Meter), new Length(-2.1447891504457, LengthUnit.Meter)),
        new Point2d(new Length(-0.60355893561307, LengthUnit.Meter), new Length(-2.1447891504457, LengthUnit.Meter)),
        new Point2d(new Length(-3.8535589356163, LengthUnit.Meter), new Length(-2.1447891504456, LengthUnit.Meter)),
        new Point2d(new Length(-3.8535589356163, LengthUnit.Meter), new Length(-2.9947938617324, LengthUnit.Meter)),
        new Point2d(new Length(-4.4535589356163, LengthUnit.Meter), new Length(-2.9947938617324, LengthUnit.Meter)),
        new Point2d(new Length(-4.4535589356163, LengthUnit.Meter), new Length(-2.1447891504456, LengthUnit.Meter)),
        new Point2d(new Length(-7.5035618086918, LengthUnit.Meter), new Length(-2.1447891504456, LengthUnit.Meter)),
        new Point2d(new Length(-7.5035093262381, LengthUnit.Meter), new Length(-1.2447891504456, LengthUnit.Meter)),
        new Point2d(new Length(-4.4535589356162, LengthUnit.Meter), new Length(-1.2447891504457, LengthUnit.Meter)),
        new Point2d(new Length(-4.4535589356163, LengthUnit.Meter), new Length(1.8052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-4.9785589356162, LengthUnit.Meter), new Length(1.8052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-4.9785589356162, LengthUnit.Meter), new Length(2.1052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-3.2285589356162, LengthUnit.Meter), new Length(2.1052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-3.2285589356162, LengthUnit.Meter), new Length(1.8052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-3.8535589356163, LengthUnit.Meter), new Length(1.8052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-3.8535589356163, LengthUnit.Meter), new Length(-1.2447891504457, LengthUnit.Meter)),
        new Point2d(new Length(-0.95356180902542, LengthUnit.Meter), new Length(-1.2447891504457, LengthUnit.Meter)),
        new Point2d(new Length(-0.95355893561614, LengthUnit.Meter), new Length(1.8052056355333, LengthUnit.Meter)),
        new Point2d(new Length(-1.6285589356162, LengthUnit.Meter), new Length(1.8052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-1.6285589356162, LengthUnit.Meter), new Length(2.1052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-0.95356180902541, LengthUnit.Meter), new Length(2.1052083886678, LengthUnit.Meter)),
        new Point2d(new Length(-0.9535618090254, LengthUnit.Meter), new Length(3.2552083887352, LengthUnit.Meter)),
        new Point2d(new Length(3.1464410643837, LengthUnit.Meter), new Length(3.2552083887352, LengthUnit.Meter)),
      };

      var polygon = new Polygon(points);
      var profile = new PerimeterProfile(polygon, new List<IPolygon>());
      var adsecProfile = AdSecProfiles.CreatePerimeterProfile(profile);
      var section = ISection.Create(adsecProfile, material);

      var bars = ISingleBars.Create(IBarBundle.Create(rebarMaterial, new Length(25, LengthUnit.Millimeter)));
      bars.Positions.Add(
        IPoint.Create(new Length(5400, LengthUnit.Millimeter), new Length(3000, LengthUnit.Millimeter)));
      section.ReinforcementGroups.Add(bars);

      section.Profile.Validate();

      return section;
    }

    private class AdSecSectionDummy : AdSecSection { }
  }
}
