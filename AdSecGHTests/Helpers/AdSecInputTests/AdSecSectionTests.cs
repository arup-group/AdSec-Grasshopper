using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionTests {
    private AdSecSection _section;

    public AdSecSectionTests() {
      _section = null;
    }

    [Fact]
    public void TryCastToAdSecSectionReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecSection(objwrap, ref _section);

      Assert.False(castSuccessful);
      Assert.Null(_section);
    }

    [Fact]
    public void TryCastToAdSecSectionReturnsCorrectDataFromAdSecSectionGoo() {
      var length = new Length(1, LengthUnit.Meter);
      var thickness = new Length(0.2, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(thickness, length),
        new WebConstant(thickness)));
      var section = ISection.Create(profile, Concrete.ACI318.Edition_2002.Metric.MPa_20);
      var input = new AdSecSectionGoo(
        new AdSecSection(section, new AdSecDesignCode().DesignCode, "", "", Plane.WorldXY));

      var objwrap = new GH_ObjectWrapper(input);
      bool castSuccessful = AdSecInput.TryCastToAdSecSection(objwrap, ref _section);

      Assert.True(castSuccessful);
      Assert.NotNull(_section);
    }

    [Fact]
    public void TryCastToAdSecSectionReturnsCorrectDataFromAdSecSubComponentGoo() {
      var length = new Length(1, LengthUnit.Meter);
      var thickness = new Length(0.2, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(thickness, length),
        new WebConstant(thickness)));
      var section = ISection.Create(profile, Concrete.ACI318.Edition_2002.Metric.MPa_20);
      var input = new AdSecSubComponentGoo(section, new Plane(), IPoint.Create(length, length),
        new AdSecDesignCode().DesignCode, "", "");

      var objwrap = new GH_ObjectWrapper(input);
      bool castSuccessful = AdSecInput.TryCastToAdSecSection(objwrap, ref _section);

      Assert.True(castSuccessful);
      Assert.NotNull(_section);
    }
  }
}
