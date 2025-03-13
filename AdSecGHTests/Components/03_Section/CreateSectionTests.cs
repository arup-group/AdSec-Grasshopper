using System.Collections.Generic;
using System.Linq;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;
using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateSectionTests {
    readonly CreateSection component;

    public CreateSectionTests() {
      component = new CreateSection();
      var profile = new ProfileBuilder().WidthDepth(1).WithWidth(2).Build();
      var profileDesign = new ProfileDesign() { Profile = profile, LocalPlane = OasysPlane.PlaneYZ };
      component.SetInputParamAt(0, new AdSecProfileGoo(profileDesign));
      var adSecMaterial = new AdSecMaterialGoo(new MaterialDesign() {
        Material = Concrete.IS456.Edition_2000.M10,
        DesignCode = IS456.Edition_2000
      });
      component.SetInputParamAt(1, adSecMaterial);
    }

    [Fact]
    public void ShouldNotHaveAnyErrors() {
      ComponentTesting.ComputeOutputs(component);
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveAnInputWithoutOptional() {
      ComponentTesting.ComputeOutputs(component);
      //var data = component.GetOutputParamAt(0) as AdSecSectionGoo;
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void ShouldHaveFourInputs() {
      Assert.Equal(4, component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(component.Params.Output);
    }
  }
}
