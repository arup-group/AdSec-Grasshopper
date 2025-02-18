using System.Collections.Generic;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSubComponentsTests {
    private Oasys.Collections.IList<ISubComponent> _subComponents;
    private List<int> invalidIds;

    public AdSecSubComponentsTests() {
      _subComponents = Oasys.Collections.IList<ISubComponent>.Create();
      invalidIds = new List<int>();
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsFalseWhenInvalidIdsListIsNullInitialised() {
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(null, _subComponents, null);

      Assert.False(castSuccessful);
      Assert.Empty(_subComponents);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsFalseWhenNull() {
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(null, _subComponents, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_subComponents);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsFalseWhenEmptySections() {
      var objectWrappers = new List<GH_ObjectWrapper>();
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(objectWrappers, _subComponents, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_subComponents);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsFalseWhenValueIsNull() {
      ISubComponent subComponent = null;
      var objectWrappers = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(subComponent),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(objectWrappers, _subComponents, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_subComponents);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsEmptyWhenNullItemSections() {
      var objectWrapper = new GH_ObjectWrapper(null);
      var objectWrappers = new List<GH_ObjectWrapper>() {
        objectWrapper,
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(objectWrappers, _subComponents, invalidIds);

      Assert.False(castSuccessful);
      Assert.Empty(_subComponents);
      Assert.Single(invalidIds);
      Assert.Equal(0, invalidIds[0]);
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsFalseWhenSecondItemIncorrect() {
      var layer = GetAdSecSectionGoo();
      var objectWrappers = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(layer),
        new GH_ObjectWrapper(null),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(objectWrappers, _subComponents, invalidIds);

      Assert.False(castSuccessful);
      Assert.Single(_subComponents);
      Assert.Single(invalidIds);
      Assert.Equal(1, invalidIds[0]);
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsCorrectDataFromISubComponent() {
      var point = IPoint.Create(new Length(1, LengthUnit.Meter), new Length(2, LengthUnit.Meter));
      var subComponent = ISubComponent.Create(GetAdSecSectionGoo().Value.Section, point);

      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(subComponent),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(objwrap, _subComponents, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_subComponents);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsCorrectDataFromAdSecSubComponentGoo() {
      var point = IPoint.Create(new Length(1, LengthUnit.Meter), new Length(2, LengthUnit.Meter));
      var subComponent = ISubComponent.Create(GetAdSecSectionGoo().Value.Section, point);
      var input = new AdSecSubComponentGoo(subComponent, Plane.WorldXY, new AdSecDesignCode().DesignCode, "test",
        string.Empty);

      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(input),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(objwrap, _subComponents, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_subComponents);
      Assert.Empty(invalidIds);
    }

    [Fact]
    public void TryCastToAdSecSubComponentsReturnsCorrectDataFromAdSecSectionGoo() {
      var length = new Length(1, LengthUnit.Meter);
      var thickness = new Length(0.2, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(thickness, length),
        new WebConstant(thickness)));
      var section = ISection.Create(profile, Concrete.ACI318.Edition_2002.Metric.MPa_20);
      var designCode = new AdSecDesignCode(ACI318.Edition_2002.Metric, "test").DesignCode;
      var adSecSection = new AdSecSection(section, designCode, "", "", Plane.WorldXY);
      var input = new AdSecSectionGoo(adSecSection);

      var objwrap = new List<GH_ObjectWrapper>() {
        new GH_ObjectWrapper(input),
      };
      bool castSuccessful = AdSecInput.TryCastToAdSecSubComponents(objwrap, _subComponents, invalidIds);

      Assert.True(castSuccessful);
      Assert.NotNull(_subComponents);
      Assert.Empty(invalidIds);
    }

    private static AdSecSectionGoo GetAdSecSectionGoo() {
      var length = new Length(1, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(length, length),
        new WebConstant(length)));
      var material = Steel.ASTM.A242_46;
      var section = ISection.Create(profile, material);
      var designCode = new AdSecDesignCode().DesignCode;
      return new AdSecSectionGoo(new AdSecSection(section, designCode, "", "", Plane.WorldXY));
    }
  }
}
