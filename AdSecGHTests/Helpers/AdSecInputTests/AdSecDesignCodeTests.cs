using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecInputTests_DesignCode {
    private AdSecDesignCode _designCode;

    public AdSecInputTests_DesignCode() {
      _designCode = null;
    }

    [Fact]
    public void TryCastToDesignCodeReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToDesignCode(objwrap, ref _designCode);

      Assert.False(castSuccessful);
      Assert.Null(_designCode);
    }

    [Fact]
    public void TryCastToDesignCodeReturnsDesignCode() {
      var designCode = new AdSecDesignCode() {
        DesignCodeName = "test",
      };
      var objwrap = new GH_ObjectWrapper(new AdSecDesignCodeGoo(designCode));
      bool castSuccessful = AdSecInput.TryCastToDesignCode(objwrap, ref _designCode);

      Assert.True(castSuccessful);
      Assert.NotNull(_designCode);
      Assert.False(_designCode.IsValid);
      Assert.Equal("test", _designCode.DesignCodeName);
    }
  }
}
