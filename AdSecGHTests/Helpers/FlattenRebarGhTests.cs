using AdSecGH.Components;

using OasysGH.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class FlattenRebarGhTests {
    private readonly FlattenRebarGhComponent component;

    public FlattenRebarGhTests() {
      component = new FlattenRebarGhComponent();
    }

    [Fact]
    public void ShouldPassAttributeValuesFromSection() {
      Assert.Equal(component.Section.Name, component.AdSecSection.Name);
      Assert.Equal(component.Section.NickName, component.AdSecSection.NickName);
      Assert.Equal(component.Section.Description, component.AdSecSection.Description);
    }

    [Fact]
    public void ShouldPassAttributeValuesFromPoint() {
      Assert.Contains(component.Position.Name, component.AdSecPoint.Name);
      Assert.Equal(component.Position.NickName, component.AdSecPoint.NickName);
      Assert.Equal(component.Position.Description, component.AdSecPoint.Description);
    }

    [Fact]
    public void ShouldSetUnitsToPosition() {
      Assert.Contains($"[{DefaultUnits.LengthUnitGeometry.GetUnit()}]", component.AdSecPoint.Name);
    }

    [Fact]
    public void ShouldSetUnitsToDiameter() {
      Assert.Contains($"[{DefaultUnits.LengthUnitGeometry.GetUnit()}]", component.Diameter.Name);
    }
  }
}
