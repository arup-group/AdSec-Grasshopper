using AdSecGH.Components;

using OasysGH.Units;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class FlattenRebarGhTests {
    private readonly FlattenRebarGh func;

    public FlattenRebarGhTests() {
      func = new FlattenRebarGh();
    }

    [Fact]
    public void ShouldPassAttributeValuesFromSection() {
      Assert.Equal(func.Section.Name, func.AdSecSection.Name);
      Assert.Equal(func.Section.NickName, func.AdSecSection.NickName);
      Assert.Equal(func.Section.Description, func.AdSecSection.Description);
    }

    [Fact]
    public void ShouldPassAttributeValuesFromPoint() {
      Assert.Contains(func.Position.Name, func.AdSecPoint.Name);
      Assert.Equal(func.Position.NickName, func.AdSecPoint.NickName);
      Assert.Equal(func.Position.Description, func.AdSecPoint.Description);
    }

    [Fact]
    public void ShouldSetUnitsToPosition() {
      Assert.Contains($"[{DefaultUnits.LengthUnitGeometry.GetUnit()}]", func.AdSecPoint.Name);
    }

    [Fact]
    public void ShouldSetUnitsToDiameter() {
      Assert.Contains($"[{DefaultUnits.LengthUnitGeometry.GetUnit()}]", func.Diameter.Name);
    }
  }
}
