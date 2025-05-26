using AdSecCore.Builders;
using AdSecCore.Functions;

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
  public class AnalyseTests {
    private readonly AdSecSectionGoo adSecSectionGoo;
    private static Analyse component;
    private readonly IDesignCode designCode = IS456.Edition_2000;
    private readonly ISection Section;

    public AnalyseTests() {
      if (component == null) {
        component = new Analyse();

        var singleBars = new BuilderSingleBar().WithSize(2).AtPosition(Geometry.Zero()).Build();
        Section = new SectionBuilder().WithWidth(40).CreateSquareSection().WithReinforcementGroup(singleBars).Build();

        var secSection = new AdSecSection(Section, designCode, "", "", Plane.WorldXY);
        adSecSectionGoo = new AdSecSectionGoo(secSection);
        component.SetInputParamAt(0, adSecSectionGoo);

        ComponentTesting.ComputeOutputs(component);
      }
    }

    [Fact]
    public void ShouldReuseThePlaneFromTheInput() {
      var adSecFailureSurfaceGoo = component.GetValue<AdSecFailureSurfaceGoo>(1);
      Assert.Equal(OasysPlane.PlaneXY, adSecFailureSurfaceGoo.FailureSurface.LocalPlane);
    }

    [Fact]
    public void ShouldHaveAWarning() {
      component.BusinessComponent.WarningMessages.Add("Test");
      ComponentTesting.ComputeOutputs(component);
      Assert.Single(component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
    }

    [Fact]
    public void ShouldHaveOneInput() {
      Assert.Single(component.Params.Input);
    }

    [Fact]
    public void ShouldHaveTwoOutputs() {
      Assert.Equal(2, component.Params.Output.Count);
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }
  }
}
