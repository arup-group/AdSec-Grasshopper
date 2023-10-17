
using System.Collections.Generic;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.Interop;
using Oasys.Taxonomy.Geometry;
using Oasys.Taxonomy.Profiles;
using OasysUnits;
using OasysUnits.Units;
using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionTests {
    [Fact]
    public void FlattenSectionTest() {

      var failurePoint = IStressStrainPoint.Create(new Pressure(1, PressureUnit.DynePerSquareCentimeter), new Strain(1, StrainUnit.MicroStrain));
      IStressStrainCurve tension = ILinearStressStrainCurve.Create(failurePoint);
      IStressStrainCurve compression = ILinearStressStrainCurve.Create(failurePoint);
      var strength = ITensionCompressionCurve.Create(tension, compression);
      var serviceability = ITensionCompressionCurve.Create(tension, compression);
      var concreteCrackCalculationParameters = IConcreteCrackCalculationParameters.Create(new Pressure(1, PressureUnit.DynePerSquareCentimeter), new Pressure(-1, PressureUnit.DynePerSquareCentimeter), new Pressure(1, PressureUnit.DynePerSquareCentimeter));
      IMaterial material = IConcrete.Create(strength, serviceability, concreteCrackCalculationParameters);

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
      var profile = new PerimeterProfile(polygon, new List<IPolygon>());
      Oasys.Profiles.IPerimeterProfile adsecProfile = AdSecProfiles.CreatePerimeterProfile(profile);
      var section = ISection.Create(adsecProfile, material);

      var adSec = IAdSec.Create(designCode);
      ISection flattened = adSec.Flatten(section);

      string foo = flattened.Profile.Description();

      Assert.Equal(section.Profile.ElasticModulus().Y.Value, flattened.Profile.ElasticModulus().Y.Value, 10);
      Assert.Equal(section.Profile.ElasticModulus().Z.Value, flattened.Profile.ElasticModulus().Z.Value, 10);
      Assert.Equal(section.Profile.SurfaceAreaPerUnitLength().Value, flattened.Profile.SurfaceAreaPerUnitLength().Value, 10);
      Assert.Equal(section.Profile.LocalAxisSecondMomentOfArea().YY.Value, flattened.Profile.LocalAxisSecondMomentOfArea().YY.Value, 10);
      Assert.Equal(section.Profile.LocalAxisSecondMomentOfArea().ZZ.Value, flattened.Profile.LocalAxisSecondMomentOfArea().ZZ.Value, 10);
      Assert.Equal(section.Profile.LocalAxisSecondMomentOfArea().YZ.Value, flattened.Profile.LocalAxisSecondMomentOfArea().YZ.Value, 10);
      Assert.Equal(section.Profile.PrincipalAxisSecondMomentOfArea().UU.Value, flattened.Profile.PrincipalAxisSecondMomentOfArea().UU.Value, 10);
      Assert.Equal(section.Profile.PrincipalAxisSecondMomentOfArea().VV.Value, flattened.Profile.PrincipalAxisSecondMomentOfArea().VV.Value, 10);
    }
  }
}
