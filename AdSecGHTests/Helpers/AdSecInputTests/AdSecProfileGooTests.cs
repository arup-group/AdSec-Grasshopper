using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.Taxonomy.Profiles;

using OasysGH.Parameters;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests_ProfileGoo {
    private AdSecProfileGoo _profileGoo;

    public AdSecInputTests_ProfileGoo() {
      _profileGoo = null;
    }

    [Fact]
    public void TryCastToProfileGooReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecProfileGoo(objwrap, ref _profileGoo);

      Assert.False(castSuccessful);
      Assert.Null(_profileGoo);
    }

    [Fact]
    public void TryCastToProfileGooReturnsProfileGooFromAdSecProfile() {
      var length = new Length(1, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(length, length),
        new WebConstant(length)));
      var profileGoo = new AdSecProfileGoo(profile, Plane.WorldXY);
      var objwrap = new GH_ObjectWrapper(profileGoo);
      bool castSuccessful = AdSecInput.TryCastToAdSecProfileGoo(objwrap, ref _profileGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_profileGoo);
      Assert.True(_profileGoo.IsValid);
    }

    [Fact]
    public void TryCastToProfileGooReturnsProfileGooFromOasysProfile() {
      var length = new Length(1, LengthUnit.Meter);
      var profile = new AngleProfile(length, new Flange(length, length), new WebConstant(length));
      var profileGoo = new OasysProfileGoo(profile);
      var objwrap = new GH_ObjectWrapper(profileGoo);
      bool castSuccessful = AdSecInput.TryCastToAdSecProfileGoo(objwrap, ref _profileGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_profileGoo);
      Assert.True(_profileGoo.IsValid);
    }
  }
}
