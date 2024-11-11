using AdSecGH.Components;
using AdSecGH.Parameters;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class FlattenRebarGhTests {
    private readonly FlattenRebarGhComponent component;
    private bool valueChanged;

    public FlattenRebarGhTests() {
      component = new FlattenRebarGhComponent();
    }

    [Fact]
    public void WhenAdSecSectionChangesSectionChanges() {
      component.Section.OnValueChanged += section => valueChanged = true;
      component.AdSecSection.Value = new AdSecSectionGoo();
      Assert.True(valueChanged);
    }
  }
}
