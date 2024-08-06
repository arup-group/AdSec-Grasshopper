using System.Collections.Generic;
using System.IO;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.IO.Serialization;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.Interop;
using Oasys.Profiles;
using Oasys.Taxonomy.Geometry;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionTests {
    [Fact]
    public void SerialiseUnflattenedSectionTest() {
      var section = CreateSection();

      var designCode = EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013;
      var adSec = IAdSec.Create(designCode);
      var flattened = adSec.Flatten(section);
      
      string fileName = Path.GetTempPath() + "AdSecSectionTest-Unflattened.ads";
      File.WriteAllText(fileName, CreateJson(designCode, section));
      
      string json = File.ReadAllText(fileName);
      var jsonParser = JsonParser.Deserialize(json);
      var actualSection = jsonParser.Sections[0];
      
      TestSection(flattened, actualSection, true);
    }

    [Fact]
    public void SerialiaseFlattenedSectionTest() {
      var section = CreateSection();

      var designCode = EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013;
      var adSec = IAdSec.Create(designCode);
      var flattened = adSec.Flatten(section);

      string fileName = Path.GetTempPath() + "AdSecSectionTest.ads";
      File.WriteAllText(fileName, CreateJson(designCode, flattened));

      string json = File.ReadAllText(fileName);
      var jsonParser = JsonParser.Deserialize(json);
      var actualSection = jsonParser.Sections[0];

      TestSection(flattened, actualSection);
    }

    private static void TestSection(ISection expected, ISection actual, bool unflattened = false) {
      var expectedProfile = (IPerimeterProfile)expected.Profile;
      var actualProfile = (IPerimeterProfile)actual.Profile;

      for (int i = 0; i < expectedProfile.SolidPolygon.Points.Count; i++) {
        var expectedPoint = expectedProfile.SolidPolygon.Points[i];
        var actualPoint = actualProfile.SolidPolygon.Points[i];

        Assert.Equal(expectedPoint.Y.Value, actualPoint.Y.Value, 4);
        Assert.Equal(expectedPoint.Z.Value, actualPoint.Z.Value, 4);
      }
      if (!unflattened) {
        Assert.Equal(((ISingleBars)expected.ReinforcementGroups[0]).Positions[0].Y.Value, ((ISingleBars)actual.ReinforcementGroups[0]).Positions[0].Y.Value, 4);
        Assert.Equal(((ISingleBars)expected.ReinforcementGroups[0]).Positions[0].Z.Value, ((ISingleBars)actual.ReinforcementGroups[0]).Positions[0].Z.Value, 4);
        Assert.Equal(((ISingleBars)expected.ReinforcementGroups[0]).BarBundle.CountPerBundle, ((ISingleBars)actual.ReinforcementGroups[0]).BarBundle.CountPerBundle);
        Assert.Equal(((ISingleBars)expected.ReinforcementGroups[0]).BarBundle.Diameter, ((ISingleBars)actual.ReinforcementGroups[0]).BarBundle.Diameter);
      }

      Assert.Equal(expectedProfile.ElasticModulus().Y.Value, actualProfile.ElasticModulus().Y.Value, 10);
      Assert.Equal(expectedProfile.ElasticModulus().Z.Value, actualProfile.ElasticModulus().Z.Value, 10);
      Assert.Equal(expectedProfile.SurfaceAreaPerUnitLength().Value, actualProfile.SurfaceAreaPerUnitLength().Value, 10);
      Assert.Equal(expectedProfile.LocalAxisSecondMomentOfArea().YY.Value, actualProfile.LocalAxisSecondMomentOfArea().YY.Value, 10);
      Assert.Equal(expectedProfile.LocalAxisSecondMomentOfArea().ZZ.Value, actualProfile.LocalAxisSecondMomentOfArea().ZZ.Value, 10);
      Assert.Equal(expectedProfile.LocalAxisSecondMomentOfArea().YZ.Value, actualProfile.LocalAxisSecondMomentOfArea().YZ.Value, 10);
      Assert.Equal(expectedProfile.PrincipalAxisSecondMomentOfArea().UU.Value, actualProfile.PrincipalAxisSecondMomentOfArea().UU.Value, 10);
      Assert.Equal(expectedProfile.PrincipalAxisSecondMomentOfArea().VV.Value, actualProfile.PrincipalAxisSecondMomentOfArea().VV.Value, 10);
    }

    private static string CreateJson(IDesignCode designCode, ISection section) {
      var json = new JsonConverter(designCode);
      return json.SectionToJson(section);
    }

    private static ISection CreateSection() {
      IMaterial material = Oasys.AdSec.StandardMaterials.Concrete.EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013.C40_50;
      var rebarMaterial = Oasys.AdSec.StandardMaterials.Reinforcement.Steel.EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013.S500B;

      var points = new List<IPoint2d>() {
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
      var profile = new Oasys.Taxonomy.Profiles.PerimeterProfile(polygon, new List<Oasys.Taxonomy.Geometry.IPolygon>());
      var adsecProfile = AdSecProfiles.CreatePerimeterProfile(profile);
      var section = ISection.Create(adsecProfile, material);

      var bars = ISingleBars.Create(IBarBundle.Create(rebarMaterial, new Length(25, LengthUnit.Millimeter)));
      bars.Positions.Add(IPoint.Create(new Length(5400, LengthUnit.Millimeter), new Length(3000, LengthUnit.Millimeter)));
      section.ReinforcementGroups.Add(bars);

      section.Profile.Validate();

      return section;
    }
  }
}
