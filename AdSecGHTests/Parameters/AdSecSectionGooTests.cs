using AdSecGH.Parameters;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionGooTests {
    [Fact]
    public void IsBakeCapable_WithNullValue_ReturnsFalse() {
      Assert.False(new AdSecSectionGoo().IsBakeCapable);
    }

    [Fact]
    public void IsBakeCapable_WithValidValue_ReturnsTrue() {
      var section = SampleData.GetSectionDesign();
      var sectionGoo = new AdSecSectionGoo(new AdSecSection(section));
      Assert.True(sectionGoo.IsBakeCapable);
    }
  }
}
