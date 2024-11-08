using AdSecGH.Components;

using AdSecGHCore.Constants;

using Oasys.AdSec;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests {
  public class FlattenRebarTests {
    private readonly FlattenRebarComponent component;
    private readonly double rebarSize = 2;
    private readonly IReinforcement ReinforcementMaterial = Reinforcement.Steel.IS456.Edition_2000.S415;
    private readonly IConcrete SectionMaterial = Concrete.IS456.Edition_2000.M10;
    private readonly double SectionSize = 40;
    private readonly IPoint zero = IPoint.Create(Length.Zero, Length.Zero);

    public FlattenRebarTests() {
      ContextUnits.Instance.SetDefaultUnits();
      component = new FlattenRebarComponent();
      var section = CreateSectionWithOneRebar();
      component.Section.Value = section;
      component.Compute();
    }

    private ISection CreateSectionWithOneRebar() {
      var section = CreateSquareSection(SectionSize);
      var singleBars = SingleBar(rebarSize);
      section.ReinforcementGroups.Add(singleBars);
      return section;
    }

    private ISection CreateSquareSection(double centimeters) {
      return ISection.Create(SquareProfile(centimeters), SectionMaterial);
    }

    private ISingleBars SingleBar(double centimeters) {
      var size = Length.FromCentimeters(centimeters);
      var singleBar = ISingleBars.Create(IBarBundle.Create(ReinforcementMaterial, size, 2));
      singleBar.Positions.Add(zero);
      return singleBar;
    }

    private static IRectangleProfile SquareProfile(double centimeters) {
      var size = Length.FromCentimeters(centimeters);
      return IRectangleProfile.Create(size, size);
    }

    [Fact]
    public void ShouldAddOutputsAsManyDiametersAsReinforcementGroups() {
      Assert.Single(component.Diameter.Value);
    }

    [Fact]
    public void ShouldRespectTheCurrentUnitsBeingSet() {
      ContextUnits.Instance.LengthUnitGeometry = LengthUnit.Millimeter;
      component.Compute();
      Assert.Equal(rebarSize * 10, component.Diameter.Value[0]);
    }
  }
}
