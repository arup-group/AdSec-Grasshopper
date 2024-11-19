using AdSecCore.Builders;
using AdSecCore.Helpers;

using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.GH.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class FlattenRebarGhTests {
    private readonly FlattenRebarGhComponent component;
    private readonly IDesignCode designCode = IS456.Edition_2000;
    private bool valueChanged;

    public FlattenRebarGhTests() {
      component = new FlattenRebarGhComponent();
    }

    private ISection Section => new SectionBuilder().WithWidth(40).CreateSquareSection().Build();

    [Fact]
    public void WhenAdSecSectionChangesSectionChanges() {
      valueChanged = false;
      component.Section.OnValueChanged += section => valueChanged = true;
      component.AdSecSection.Value = new AdSecSectionGoo();
      Assert.True(valueChanged);
    }

    [Fact(Skip
      = "Cannot compare like so, since the sections are cloned, perhaps we have some code on GSA.GH that can help")]
    public void WhenAdSecSectionChangesSectionGetsTheSameValue() {
      var secSection = new AdSecSection(Section, designCode, "", "", Plane.WorldXY);
      var adSecSectionGoo = new AdSecSectionGoo(secSection);
      component.AdSecSection.Value = adSecSectionGoo;
      Assert.True(Equals(Section, component.Section.Value));
    }
  }

  [Collection("GrasshopperFixture collection")]
  public class FlattenRebarTests {
    private readonly AdSecSectionGoo adSecSectionGoo;
    private readonly FlattenRebar component;
    private readonly ISection Section;

    private readonly IDesignCode designCode = IS456.Edition_2000;

    public FlattenRebarTests() {
      component = new FlattenRebar();

      var singleBars = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Zero())
       .Build();
      Section = new SectionBuilder().WithWidth(40).CreateSquareSection().WithReinforcementGroup(singleBars).Build();

      var secSection = new AdSecSection(Section, designCode, "", "", Plane.WorldXY);
      adSecSectionGoo = new AdSecSectionGoo(secSection);
      component.SetInputParamAt(0, adSecSectionGoo);

      ComponentTesting.ComputeOutputs(component);
      //component.CollectData();
    }

    [Fact]
    public void ShouldPassDataFromGhInputToSection() {
      Assert.NotNull(component.BusinessComponent.Section.Value);
    }

    [Fact]
    public void ShouldPassDataToDiameterOutput() {
      var outputParamAt = component.GetOutputParamAt(1);
      var diameter = outputParamAt.GetValue(0,0) as GH_Number;
      Assert.NotNull(diameter);
      Assert.Equal(0.02, diameter.Value);
    }

    // [Fact]
    // public void ShouldPassDataToMaterialOutput() {
    //   Assert.Equal("Reinforcement", component.BusinessComponent.Material.Value[0]);
    // }
    //
    // [Fact]
    // public void ShouldPassDataToASDOutput() {
    //   Assert.Equal(1, component.BusinessComponent.BundleCount.Value[0]);
    // }

    [Fact]
    public void ShouldHaveNoWarning() {
      var runtimeMessages = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Empty(runtimeMessages);
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }
  }
}
