using AdSecGH.Components;
using AdSecGH.Helpers;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class IGHParamExtensionsTests {
    private GH_Component owner;

    public IGHParamExtensionsTests() {
      owner = new CreatePoint(); // whatever component
    }

    [Fact]
    public void ConvertErrorShouldAddErrorIntoParam() {
      owner.Params.Input[0].ConvertError("test");

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(actualResult);
      Assert.Contains("test", actualResult[0]);
    }

    [Fact]
    public void FailedToCollectDataWarningShouldAddWarningIntoParam() {
      owner.Params.Input[0].FailedToCollectDataWarning();

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(actualResult);
      Assert.Contains("failed", actualResult[0]);
    }
  }
}
