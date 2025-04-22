using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Xunit;

namespace AdSecGHTests.Components.Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateStandardMaterialTests {
    private readonly CreateStandardMaterial _component;

    public CreateStandardMaterialTests() {
      _component = new CreateStandardMaterial();
    }

    private void SetConcrete() {
      _component.SetSelected(0, 0);
    }
    private void SetRebar() {
      _component.SetSelected(0, 1);
    }

    private void SetEuropeanCode() {
      _component.SetSelected(1, 4);
    }

    private void SetNationalAnnex(int nationalAnnex) {
      _component.SetSelected(2, nationalAnnex);
    }

    [Fact]
    public void NumberOfConcreteDesignCodeIsCorrect() {
      SetConcrete();
      Assert.Equal(12, _component.DropDownItems[1].Count);
    }

    [Fact]
    public void NumberOfEuroCodeNationalAnnexAreCorrect() {
      SetConcrete();
      Assert.Equal(14, _component.DropDownItems[2].Count);
    }

    [Fact]
    public void NumberOfEuroCodeEditionsAreCorrect() {
      SetConcrete();
      Assert.Equal(2, _component.DropDownItems[3].Count);
    }

    [Fact]
    public void NumberOfRebarDesignCodeIsCorrect() {
      SetRebar();
      Assert.Equal(12, _component.DropDownItems[1].Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    public void EN1992ConcreteMaterialsAreExpected(int nationalAnnex) {
      SetConcrete();
      SetEuropeanCode();
      SetNationalAnnex(nationalAnnex);
      var concreteGrades = _component.DropDownItems[_component.DropDownItems.Count - 1];
      Assert.Equal(15, concreteGrades.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    public void EN1992RebarMaterialsAreExpected(int nationalAnnex) {
      SetRebar();
      SetEuropeanCode();
      SetNationalAnnex(nationalAnnex);
      var rebarGrades = _component.DropDownItems[_component.DropDownItems.Count - 1];
      Assert.Equal(3, rebarGrades.Count);
    }

    [Theory]
    [InlineData(0, 12)]
    [InlineData(1, 12)]
    [InlineData(2, 9)]
    [InlineData(3, 7)]
    [InlineData(5, 0)]
    public void ShouldReturnNullForMissingValues(int materialType, int expectedSize) {
      var designCodes = ReflectionHelper.StandardCodes((AdSecMaterial.AdSecMaterialType)materialType);
      Assert.Equal(expectedSize, designCodes.Count);
    }
  }
}
