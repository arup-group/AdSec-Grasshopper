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
    public void FlattenSectionTest() {
      IMaterial material = Oasys.AdSec.StandardMaterials.Concrete.EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013.C40_50;
      IReinforcement rebarMaterial = Oasys.AdSec.StandardMaterials.Reinforcement.Steel.EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013.S500B;
      IDesignCode designCode = EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013;

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
      IPerimeterProfile adsecProfile = AdSecProfiles.CreatePerimeterProfile(profile);
      var section = ISection.Create(adsecProfile, material);

      var bars = ISingleBars.Create(IBarBundle.Create(rebarMaterial, new Length(25, LengthUnit.Millimeter)));
      bars.Positions.Add(IPoint.Create(new Length(5400, LengthUnit.Millimeter), new Length(3000, LengthUnit.Millimeter)));
      section.ReinforcementGroups.Add(bars);

      section.Profile.Validate();

      var adSec = IAdSec.Create(designCode);
      ISection flattened = adSec.Flatten(section);

      string fileName = Path.GetTempPath() + "AdSecSectionTest.ads";
      File.WriteAllText(fileName, CreateJson(designCode, section));

      string json = File.ReadAllText(fileName);
      ParsedResult jsonParser = JsonParser.Deserialize(json);
      ISection actualSection = jsonParser.Sections[0];

      var expectedProfile = (IPerimeterProfile)flattened.Profile;
      var actualProfile = (IPerimeterProfile)actualSection.Profile;

      for (int i = 0; i < expectedProfile.SolidPolygon.Points.Count; i++) {
        IPoint expectedPoint = expectedProfile.SolidPolygon.Points[i];
        IPoint actualPoint = actualProfile.SolidPolygon.Points[i];

        Assert.Equal(expectedPoint.Y.Value, actualPoint.Y.Value, 8);
        Assert.Equal(expectedPoint.Z.Value, actualPoint.Z.Value, 8);
      }

      Assert.Equal(((ISingleBars)flattened.ReinforcementGroups[0]).Positions[0].Y.Value, ((ISingleBars)actualSection.ReinforcementGroups[0]).Positions[0].Y.Value, 8);
      Assert.Equal(((ISingleBars)flattened.ReinforcementGroups[0]).Positions[0].Z.Value, ((ISingleBars)actualSection.ReinforcementGroups[0]).Positions[0].Z.Value, 8);
      Assert.Equal(((ISingleBars)flattened.ReinforcementGroups[0]).BarBundle.CountPerBundle, ((ISingleBars)actualSection.ReinforcementGroups[0]).BarBundle.CountPerBundle);
      Assert.Equal(((ISingleBars)flattened.ReinforcementGroups[0]).BarBundle.Diameter, ((ISingleBars)actualSection.ReinforcementGroups[0]).BarBundle.Diameter);

      Assert.Equal(flattened.Profile.ElasticModulus().Y.Value, actualSection.Profile.ElasticModulus().Y.Value, 10);
      Assert.Equal(flattened.Profile.ElasticModulus().Z.Value, actualSection.Profile.ElasticModulus().Z.Value, 10);
      Assert.Equal(flattened.Profile.SurfaceAreaPerUnitLength().Value, actualSection.Profile.SurfaceAreaPerUnitLength().Value, 10);
      Assert.Equal(flattened.Profile.LocalAxisSecondMomentOfArea().YY.Value, actualSection.Profile.LocalAxisSecondMomentOfArea().YY.Value, 10);
      Assert.Equal(flattened.Profile.LocalAxisSecondMomentOfArea().ZZ.Value, actualSection.Profile.LocalAxisSecondMomentOfArea().ZZ.Value, 10);
      Assert.Equal(flattened.Profile.LocalAxisSecondMomentOfArea().YZ.Value, actualSection.Profile.LocalAxisSecondMomentOfArea().YZ.Value, 10);
      Assert.Equal(flattened.Profile.PrincipalAxisSecondMomentOfArea().UU.Value, actualSection.Profile.PrincipalAxisSecondMomentOfArea().UU.Value, 10);
      Assert.Equal(flattened.Profile.PrincipalAxisSecondMomentOfArea().VV.Value, actualSection.Profile.PrincipalAxisSecondMomentOfArea().VV.Value, 10);
    }

    private string CreateJson(IDesignCode designCode, ISection section) {
      var json = new JsonConverter(designCode);
      return json.SectionToJson(section);
    }
  }
}
