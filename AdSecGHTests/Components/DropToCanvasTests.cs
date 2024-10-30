using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class DropToCanvasTests {
    [Fact]
    public void TestAllComponents() {
      var instances = ModFactory.CreateInstancesWithCreateInterface();
      foreach (var instance in instances) {
        instance.ExpireSolution(true);
        instance.CollectData();
        Assert.True(instance.RuntimeMessages(GH_RuntimeMessageLevel.Error).Count == 0);
      }
    }
  }
}
