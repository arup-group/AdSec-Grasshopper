using AdSecCore.Builders;

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
  public class AnalyseTests {

    private readonly Analyse component;
    private readonly IDesignCode designCode = IS456.Edition_2000;
    private readonly ISection Section;
    private readonly AdSecSectionGoo adSecSectionGoo;

    public AnalyseTests() {
      component = new Analyse();

      var singleBars = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Zero())
       .Build();
      Section = new SectionBuilder().WithWidth(40).CreateSquareSection().WithReinforcementGroup(singleBars).Build();

      var secSection = new AdSecSection(Section, designCode, "", "", Plane.WorldXY);
      adSecSectionGoo = new AdSecSectionGoo(secSection);
      component.SetInputParamAt(0, adSecSectionGoo);

      ComponentTesting.ComputeOutputs(component);
    }
    
    [Fact]
    public void ShouldHaveOneInput() {
      Assert.Single(component.Params.Input);
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }
  }

  [Collection("GrasshopperFixture collection")]
  public class FlattenRebarGhTests {
    private readonly IDesignCode designCode = IS456.Edition_2000;
    private readonly FlattenRebarGh func;

    private readonly ISection Section = new SectionBuilder().WithWidth(40).CreateSquareSection().Build();
    private bool valueChanged;

    public FlattenRebarGhTests() {
      func = new FlattenRebarGh();
    }

    [Fact]
    public void WhenAdSecSectionChangesSectionChanges() {
      valueChanged = false;
      func.Section.OnValueChanged += section => valueChanged = true;
      func.AdSecSection.Value = new AdSecSectionGoo();
      Assert.True(valueChanged);
    }

    [Fact(Skip
      = "Cannot compare like so, since the sections are cloned, perhaps we have some code on GSA.GH that can help")]
    public void WhenAdSecSectionChangesSectionGetsTheSameValue() {
      var secSection = new AdSecSection(Section, designCode, "", "", Plane.WorldXY);
      var adSecSectionGoo = new AdSecSectionGoo(secSection);
      func.AdSecSection.Value = adSecSectionGoo;
      Assert.True(Equals(Section, func.Section.Value));
    }
  }

  [Collection("GrasshopperFixture collection")]
  public class FlattenRebarTests {
    private readonly AdSecSectionGoo adSecSectionGoo;
    private readonly FlattenRebar component;

    private readonly IDesignCode designCode = IS456.Edition_2000;
    private readonly ISection Section;

    public FlattenRebarTests() {
      component = new FlattenRebar();

      var singleBars = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Zero())
       .Build();
      Section = new SectionBuilder().WithWidth(40).CreateSquareSection().WithReinforcementGroup(singleBars).Build();

      var secSection = new AdSecSection(Section, designCode, "", "", Plane.WorldXY);
      adSecSectionGoo = new AdSecSectionGoo(secSection);
      component.SetInputParamAt(0, adSecSectionGoo);

      ComponentTesting.ComputeOutputs(component);
    }

    [Fact]
    public void ShouldPassDataFromGhInputToSection() {
      Assert.NotNull(component.BusinessComponent.Section.Value);
    }

    [Fact]
    public void ShouldPassDataFromPositionToAdSecPoint() {
      Assert.NotNull(component.BusinessComponent.AdSecPoint);
    }

    [Fact]
    public void ShouldPassDataToPositionOutput() {
      var position = component.GetOutputParamAt(0).GetValue<AdSecPointGoo>(0, 0);
      Assert.NotNull(position);
      Assert.Equal(0, position.AdSecPoint.Y.Value);
      Assert.Equal(0, position.AdSecPoint.Z.Value);
    }

    [Fact]
    public void ShouldPassDataToDiameterOutput() {
      var diameter = component.GetOutputParamAt(1).GetValue<GH_Number>(0, 0);
      Assert.NotNull(diameter);
      Assert.Equal(0.02, diameter.Value);
    }

    [Fact]
    public void ShouldPassDataToBundleCountOutput() {
      var bundleCount = component.GetOutputParamAt(2).GetValue<GH_Integer>(0, 0);
      Assert.NotNull(bundleCount);
      Assert.Equal(1, bundleCount.Value);
    }

    [Fact]
    public void ShouldPassDataToPreLoadOutput() {
      var preLoad = component.GetOutputParamAt(3).GetValue<GH_Number>(0, 0);
      Assert.NotNull(preLoad);
      Assert.Equal(0, preLoad.Value);
    }

    [Fact]
    public void ShouldPassDataToMaterialOutput() {
      var material = component.GetOutputParamAt(4).GetValue<GH_String>(0, 0);
      Assert.NotNull(material);
      Assert.Equal("Reinforcement", material.Value);
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
