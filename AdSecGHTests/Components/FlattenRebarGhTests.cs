using AdSecCore.Builders;

using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

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

    private readonly IDesignCode designCode = IS456.Edition_2000;

    public FlattenRebarTests() {
      component = new FlattenRebar();
      var secSection = new AdSecSection(Section, designCode, "", "", Plane.WorldXY);
      adSecSectionGoo = new AdSecSectionGoo(secSection);
      component.SetParamAt(0, adSecSectionGoo);
    }

    private ISection Section => new SectionBuilder().WithWidth(40).CreateSquareSection().Build();

    [Fact]
    public void ShouldPassDataFromGhInputToSection() {
      Assert.True(component.SetParamAt(0, adSecSectionGoo));
      ComponentTesting.ComputeOutputs(component);
      component.CollectData();
      var runtimeMessages = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      var messages = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.NotNull(component.BusinessComponent.Section.Value);
    }

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
