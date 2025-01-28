using System.Collections.Generic;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.StandardMaterials;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionsTests {
    private List<AdSecSection> _sections;
    private List<int> invalidIds;

    public AdSecSectionsTests() {
      _sections = new List<AdSecSection>();
      invalidIds = new List<int>();
    }

    [Fact]
    public void TryCastToAdSecSectionsReturnsFalseWhenWhenNull() {
      bool castSuccessful = AdSecInput.TryCastToAdSecSections(null, _sections, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_sections);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecSectionsReturnsFalseWhenWhenEmptySections() {
      var objectWrappers = new List<GH_ObjectWrapper>();
      bool castSuccessful = AdSecInput.TryCastToAdSecSections(objectWrappers, _sections, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_sections);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecSectionsReturnsEmptyWhenNullItemSections() {
      var objectWrapper = new GH_ObjectWrapper(null);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objectWrapper,
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSections(objectWrappers, _sections, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_sections);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
    }

    [Fact]
    public void TryCastToAdSecSectionsReturnsFalseWhenSecondItemIncorrect() {
      var objectWrappers = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(CreateAdSecSection()),
        new GH_ObjectWrapper(null),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSections(objectWrappers, _sections, invalidIds);

      Assert.False(castSuccessful);
      Assert.Single(_sections);
      Assert.Single(invalidIds);
      Assert.Equal(1, invalidIds[0]);
    }

    [Fact]
    public void TryCastToAdSecSectionReturnsCorrectDataFromAdSecSection() {
      var input = CreateAdSecSection();

      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(input),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSections(objwrap, _sections, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_sections);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecSectionReturnsCorrectDataFromAdSecSectionGoo() {
      var input = CreateAdSecSection();

      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(new AdSecSectionGoo(input)),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSections(objwrap, _sections, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_sections);
      Assert.Empty(invalidIds);
    }

    private static AdSecSection CreateAdSecSection() {
      var length = new Length(1, LengthUnit.Meter);
      var thickness = new Length(0.2, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(thickness, length),
        new WebConstant(thickness)));
      var section = ISection.Create(profile, Concrete.ACI318.Edition_2002.Metric.MPa_20);
      var input = new AdSecSection(section, new AdSecDesignCode().DesignCode, "", "", Plane.WorldXY);
      return input;
    }
  }
}
