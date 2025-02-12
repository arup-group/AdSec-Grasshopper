using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests_PointGoo {
    private AdSecPointGoo _pointGoo = null;

    public AdSecInputTests_PointGoo() {
      _pointGoo = null;
    }

    [Fact]
    public void TryCastToAdSecPointGooReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecPointGoo(objwrap, ref _pointGoo);

      Assert.False(castSuccessful);
      Assert.Null(_pointGoo);
    }

    [Fact]
    public void TryCastToAdSecPointGooReturnsPointGoo() {
      var length = new Length(1, LengthUnit.Meter);
      var adSecPointGoo = new AdSecPointGoo(length, length) { };
      var objwrap = new GH_ObjectWrapper(adSecPointGoo);
      bool castSuccessful = AdSecInput.TryCastToAdSecPointGoo(objwrap, ref _pointGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_pointGoo);
      Assert.True(_pointGoo.IsValid);
      Assert.Equal(0, _pointGoo.Value.X);
      Assert.Equal(length.As(DefaultUnits.LengthUnitGeometry), _pointGoo.Value.Y);
      Assert.Equal(length.As(DefaultUnits.LengthUnitGeometry), _pointGoo.Value.Z);
    }

    [Fact]
    public void TryCastToAdSecPointReturnsPointGoo() {
      var objwrap = new GH_ObjectWrapper(new GH_Point());
      bool castSuccessful = AdSecInput.TryCastToAdSecPointGoo(objwrap, ref _pointGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_pointGoo);
      Assert.True(_pointGoo.IsValid);
      Assert.Equal(0, _pointGoo.Value.X);
      Assert.Equal(0, _pointGoo.Value.Y);
      Assert.Equal(0, _pointGoo.Value.Z);
    }
  }
}
