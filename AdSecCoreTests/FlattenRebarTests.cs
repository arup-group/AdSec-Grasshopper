using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGHCore.Constants;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests {

  public class FlattenRebarTests {
    private readonly FlattenRebarFunction operation;
    private readonly double rebarSize = 2;
    private readonly double SectionSize = 40;
    private readonly ISingleBars singleBars;

    public FlattenRebarTests() {
      ContextUnits.Instance.SetDefaultUnits();
      operation = new FlattenRebarFunction();
      singleBars = new BuilderSingleBar().WithSize(rebarSize).CreateSingleBar().AtPosition(Geometry.Zero()).Build();
      var section = new SectionBuilder().WithWidth(SectionSize).CreateSquareSection().WithReinforcementGroup(singleBars)
       .Build();
      operation.Section.Value = new SectionDesign {
        Section = section,
        DesignCode = IS456.Edition_2000
      };
      operation.Compute();
    }

    [Fact]
    public void ShouldHaveOneInput() {
      Assert.Single(operation.GetAllInputAttributes());
    }

    [Fact]
    public void ShouldHaveFiveOutputs() {
      Assert.Equal(5, operation.GetAllOutputAttributes().Length);
    }

    [Fact]
    public void ShouldAddOutputsAsManyDiametersAsRebars() {
      Assert.Single(operation.Diameter.Value);
    }

    [Fact]
    public void ShouldAddOutputsAsManyPositionsAsRebars() {
      Assert.Single(operation.Position.Value);
    }

    [Fact]
    public void ShouldAddOutputsAsManyBundleCountsAsRebars() {
      Assert.Single(operation.BundleCount.Value);
    }

    [Fact]
    public void ShouldAddOutputsAsManyPreLoadsAsRebars() {
      singleBars.Preload = IPreForce.Create(Force.FromKilonewtons(1));
      operation.Compute();
      Assert.Single(operation.PreLoad.Value);
      Assert.NotEqual(0, operation.PreLoad.Value[0]);
    }

    [Fact]
    public void ShouldThrowForOtherPreloadTypes() {
      Assert.Throws<NotSupportedException>(() => FlattenRebarFunction.GetPreLoad(new DummyPreload()));
    }

    [Fact]
    public void ShouldBeAbleToGetPreLoadsWithStrain() {
      singleBars.Preload = IPreStrain.Create(Strain.FromPercent(1));
      operation.Compute();
      Assert.Single(operation.PreLoad.Value);
      Assert.NotEqual(0, operation.PreLoad.Value[0]);
    }

    [Fact]
    public void ShouldBeAbleToGetPreLoadsWithPressure() {
      singleBars.Preload = IPreStress.Create(Pressure.FromMegapascals(1));
      operation.Compute();
      Assert.Single(operation.PreLoad.Value);
      Assert.NotEqual(0, operation.PreLoad.Value[0]);
    }

    [Fact]
    public void ShouldAddOutputsAsManyMaterialsAsRebars() {
      Assert.Single(operation.Material.Value);
    }

    [Fact]
    public void ShouldGetTheMaterialNameReadableForUsers() {
      Assert.Equal("Reinforcement", operation.Material.Value[0]);
    }

    [Fact]
    public void ShouldRespectTheCurrentUnitsBeingSet() {
      ContextUnits.Instance.LengthUnitGeometry = LengthUnit.Millimeter;
      operation.Compute();
      Assert.Equal(rebarSize * 10, operation.Diameter.Value[0]);
    }

    private class DummyPreload : IPreload { }
  }
}
