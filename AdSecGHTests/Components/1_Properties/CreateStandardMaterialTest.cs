using System.Collections.Generic;
using System.Linq;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec.DesignCode;
using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components.Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateStandardMaterialTests {
    private readonly CreateStandardMaterial _component;
    private readonly List<string> StandardDesignCodes = new List<string> { "IS456", "ACI318", "EN1992" };
    public CreateStandardMaterialTests() {
      AdSecUtility.LoadAdSecAPI();
      _component = new CreateStandardMaterial();
    }

    private void SetConcrete() {
      _component.SetSelected(0, 0);
    }
    private void SetRebar() {
      _component.SetSelected(0, 1);
    }

    private void SetSteel() {
      _component.SetSelected(0, 3);
    }

    private void SetEuropeanCode() {
      _component.SetSelected(1, 4);
    }

    private void SetEuropeanSteelCode() {
      _component.SetSelected(1, 1);
    }

    private void SetBritishSteelCode() {
      _component.SetSelected(1, 0);
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

    [Fact]
    public void SearchIsGivingExpectedResultForSpecificKey() {
      SetConcrete();
      ComponentTestHelper.SetInput(_component, "C32");
      var material = (AdSecMaterialGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal("C32_40", material.Material.GradeName);
    }

    [Fact]
    public void SearchIsGivingExpectedResultForAll() {
      SetConcrete();
      ComponentTestHelper.SetInput(_component, "all");
      var material = (AdSecMaterialGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal("C12_15", material.Material.GradeName);
    }

    [Fact]
    public void SearchIsGivingExpectedResultForMatchingCharacter() {
      SetRebar();
      ComponentTestHelper.SetInput(_component, "sb");
      var material = (AdSecMaterialGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal("S500B", material.Material.GradeName);
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

    [Fact]
    public void EN1992SteelMaterialsAreExpected() {
      SetSteel();
      SetEuropeanSteelCode();
      var steelGrades = _component.DropDownItems[_component.DropDownItems.Count - 1];
      Assert.Equal(4, steelGrades.Count);
    }

    [Fact]
    public void BritishSteelMaterialsAreExpected() {
      SetSteel();
      SetBritishSteelCode();
      var steelGrades = _component.DropDownItems[_component.DropDownItems.Count - 1];
      Assert.Equal(5, steelGrades.Count);
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

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.StandardMaterial));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void GetDesignCodeIsReportingCorrectGrade() {
      var designCode = new AdSecDesignCode() { DesignCode = IS456.Edition_2000 };
      var material = new AdSecMaterial() { DesignCode = designCode };
      var standardMaterial = CreateStandardMaterial.GetDesignCode(material);
      Assert.NotNull(standardMaterial.IDesignCode);
      Assert.Equal("IS456+Edition_2000", standardMaterial.DesignCodeName);
    }

    [Fact]
    public void GetInitialCodeSelectionIsPreviousOneWhenThereIsNoMatchForPreviousSelction() {
      var selectedCode = CreateStandardMaterial.GetInitialCodeSelection(StandardDesignCodes.Where(x => x != "EN1992"), "EN1992");
      Assert.Equal("IS456", selectedCode);
    }

    [Fact]
    public void GetInitialCodeSelectionIsPreviousOneWhenThereIsMatchForPreviousSelction() {
      var selectedCode = CreateStandardMaterial.GetInitialCodeSelection(StandardDesignCodes, "EN1992");
      Assert.Equal("EN1992", selectedCode);
    }


  }
}
