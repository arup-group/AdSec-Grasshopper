using AdSecGH.Parameters;

using Grasshopper.Kernel.Parameters;

using Oasys.GH.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Helpers {

  [Collection("GrasshopperFixture collection")]
  public class AdvancedDataConvertorTest {
    private readonly AllParameters allParameters;
    private readonly AllParametersComponent component;

    public AdvancedDataConvertorTest() {
      allParameters = new AllParameters();
      component = new AllParametersComponent();
      allParameters = component.BusinessComponent;
      Recompute();
    }

    private void Recompute() {
      component.SetDefaultValues();
      ComponentTesting.ComputeOutputs(component);
    }

    [Fact]
    public void ShouldBeAbleToCreateIt() {
      Assert.NotNull(component);
    }

    [Fact]
    public void ShouldHaveSectionParameter() {
      Assert.NotNull(component.Params.GetInputParam("Section") as AdSecSectionParameter);
    }

    [Fact]
    public void ShouldHavePointParameter() {
      Assert.NotNull(component.Params.GetInputParam("Points") as Param_GenericObject);
    }

    [Fact]
    public void ShouldHaveMaterialsParameter() {
      Assert.NotNull(component.Params.GetInputParam("Materials") as AdSecMaterialParameter);
    }

    [Fact]
    public void ShouldHaveSamePointOutput() {
      allParameters.Points.Default = new[] {
        new AdSecPointGoo(Point3d.Origin),
      };
      Recompute();
      Assert.NotNull(component.Params.GetInputParam("Points").GetValue(0, 0));
    }

    [Fact]
    public void ShouldHaveSameSectionOutput() {
      allParameters.Section.Default = new AdSecSectionGoo();
      Recompute();
      Assert.NotNull(component.Params.GetInputParam("Section").GetValue(0, 0));
    }

    [Fact]
    public void ShouldHaveSameMaterialOutput() {
      allParameters.Material.Default = new[] {
        new AdSecMaterialGoo(new AdSecMaterial()),
      };
      Recompute();

      Assert.NotNull(component.Params.GetInputParam("Materials").GetValue(0, 0));
    }
  }
}
