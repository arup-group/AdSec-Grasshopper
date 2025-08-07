using AdSecCore.Constants;

namespace AdSecCoreTests.Constants {
  public class AdSecFileHelperTests {

    [Fact]
    public void ShouldLoadAssemblyUsingLoadFrom() {
      var loader = new AdSecDllLoader(AdSecDllLoader.LoadMode.LoadFrom);
      Assert.NotNull(loader.AdSecAPI());
    }

    [Fact]
    public void ShouldLoadAssemblyUsingLoad() {
      Assert.Throws<FileNotFoundException>(() => {
        var loader = new AdSecDllLoader(AdSecDllLoader.LoadMode.Load);
        Assert.NotNull(loader.AdSecAPI());
      });
    }

    [Fact]
    public void ShouldLoadAssemblyUsingCustom() {
      var loader = new AdSecDllLoader(AdSecDllLoader.LoadMode.Custom) {
        Custom = null,
      };
      Assert.Null(loader.AdSecAPI());
    }
  }
}
