using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSolutionGooTests {
    private AdSecSolutionGoo _solutionGoo;

    public AdSecSolutionGooTests() {
      _solutionGoo = null;
    }

    [Fact]
    public void TryCastToAdSecSolutionGooReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecSolutionGoo(objwrap, ref _solutionGoo);

      Assert.False(castSuccessful);
      Assert.Null(_solutionGoo);
    }

    [Fact]
    public void TryCastToAdSecSolutionGooReturnsFalseWhenCantConvertFromNull() {
      GH_ObjectWrapper objwrap = null;
      bool castSuccessful = AdSecInput.TryCastToAdSecSolutionGoo(objwrap, ref _solutionGoo);

      Assert.False(castSuccessful);
      Assert.Null(_solutionGoo);
    }

    [Fact]
    public void TryCastToAdSecSolutionGooReturnsCorrectDataFromAdSecolutionGoo() {
      var length = new Length(1, LengthUnit.Meter);
      var thickness = new Length(0.2, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(thickness, length),
        new WebConstant(thickness)));
      var section = ISection.Create(profile, Concrete.ACI318.Edition_2002.Metric.MPa_20);
      var designCode = new AdSecDesignCode(ACI318.Edition_2002.Metric).DesignCode;
      var adSecSection = new AdSecSection(section, designCode, Plane.WorldXY);

      var adSec = IAdSec.Create(adSecSection.DesignCode);
      var solution = adSec.Analyse(adSecSection.Section);

      var input = new AdSecSolutionGoo(solution, adSecSection);

      var objwrap = new GH_ObjectWrapper(input);
      bool castSuccessful = AdSecInput.TryCastToAdSecSolutionGoo(objwrap, ref _solutionGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_solutionGoo);
    }
  }
}
