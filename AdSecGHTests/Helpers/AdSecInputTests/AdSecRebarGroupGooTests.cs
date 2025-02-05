using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class AdSecRebarGroupGooTests {
    private AdSecRebarGroupGoo _groupGoo;

    public AdSecRebarGroupGooTests() {
      _groupGoo = null;
    }

    [Fact]
    public void TryCastToAdSecRebarGroupGooReturnsFalseWhenCantConvert() {
      var objwrap = new GH_ObjectWrapper();
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroupGoo(objwrap, ref _groupGoo);

      Assert.False(castSuccessful);
      Assert.Null(_groupGoo);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupGooReturnsFalseWhenCantConvertFromNull() {
      GH_ObjectWrapper objwrap = null;
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroupGoo(objwrap, ref _groupGoo);

      Assert.False(castSuccessful);
      Assert.Null(_groupGoo);
    }

    [Fact]
    public void TryCastToAdSecRebarGroupGooReturnsCorrectDataFromAdSecRebarGroupGoo() {
      var input = new AdSecRebarGroupGoo(new AdSecRebarGroup());

      var objwrap = new GH_ObjectWrapper(input);
      bool castSuccessful = AdSecInput.TryCastToAdSecRebarGroupGoo(objwrap, ref _groupGoo);

      Assert.True(castSuccessful);
      Assert.NotNull(_groupGoo);
    }
  }
}
