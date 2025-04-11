using Oasys.AdSec.StandardMaterials;

namespace AdSecCoreTests.Builder {
  public class MaterialHelperTests {

    [Fact]
    public void TestARebarName() {
      var result = MaterialHelper.FindPath(Reinforcement.Steel.IS456.Edition_2000.S415);
      Assert.Contains("Reinforcement+Steel+IS456+Edition_2000.S415", result);
    }

    [Fact]
    public void TestAConcreteName() {
      var result = MaterialHelper.FindPath(Concrete.IS456.Edition_2000.M10);
      Assert.Contains("Concrete+IS456+Edition_2000.M10", result);
    }

    [Fact]
    public void NullThrowsError() {
      Assert.Throws<ArgumentNullException>(() => MaterialHelper.FindPath(null));
    }

    [Fact]
    public void ShouldReturnNullForMissingValues() {
      Assert.Null(MaterialHelper.FindPath("MissingMaterial"));
    }
  }
}
