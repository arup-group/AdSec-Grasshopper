using AdSecCore.Builders;
using AdSecCore.Helpers;

using AdSecGH.Components;

using AdSecGHCore.Constants;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests {

  public class FlattenRebarTests {
    private readonly FlattenRebarComponent component;
    private readonly double rebarSize = 2;
    private readonly double SectionSize = 40;
    private readonly ISingleBars singleBars;

    public FlattenRebarTests() {
      ContextUnits.Instance.SetDefaultUnits();
      component = new FlattenRebarComponent();
      singleBars = new BuilderReinforcementGroup().WithSize(rebarSize).CreateSingleBar().AtPosition(Geometry.Zero())
       .Build() as ISingleBars;
      var section = new SectionBuilder().WithWidth(SectionSize).CreateSquareSection().WithReinforcementGroup(singleBars)
       .Build();
      component.Section.Value = section;
      component.Compute();
    }

    [Fact]
    public void ShouldAddOutputsAsManyDiametersAsRebars() {
      Assert.Single(component.Diameter.Value);
    }

    [Fact]
    public void ShouldAddOutputsAsManyPositionsAsRebars() {
      Assert.Single(component.Position.Value);
    }

    [Fact]
    public void ShouldAddOutputsAsManyBundleCountsAsRebars() {
      Assert.Single(component.BundleCount.Value);
    }

    [Fact]
    public void ShouldAddOutputsAsManyPreLoadsAsRebars() {
      singleBars.Preload = IPreForce.Create(Force.FromKilonewtons(1));
      component.Compute();
      Assert.Single(component.PreLoad.Value);
      Assert.NotEqual(0, component.PreLoad.Value[0]);
    }

    [Fact]
    public void ShouldBeAbleToGetPreLoadsWithStrain() {
      singleBars.Preload = IPreStrain.Create(Strain.FromPercent(1));
      component.Compute();
      Assert.Single(component.PreLoad.Value);
      Assert.NotEqual(0, component.PreLoad.Value[0]);
    }

    [Fact]
    public void ShouldBeAbleToGetPreLoadsWithPressure() {
      singleBars.Preload = IPreStress.Create(Pressure.FromMegapascals(1));
      component.Compute();
      Assert.Single(component.PreLoad.Value);
      Assert.NotEqual(0, component.PreLoad.Value[0]);
    }

    [Fact]
    public void ShouldAddOutputsAsManyMaterialsAsRebars() {
      Assert.Single(component.Material.Value);
    }

    [Fact]
    public void ShouldGetTheMaterialNameReadableForUsers() {
      Assert.Equal("Reinforcement", component.Material.Value[0]);
    }

    [Fact]
    public void ShouldRespectTheCurrentUnitsBeingSet() {
      ContextUnits.Instance.LengthUnitGeometry = LengthUnit.Millimeter;
      component.Compute();
      Assert.Equal(rebarSize * 10, component.Diameter.Value[0]);
    }
  }
}
