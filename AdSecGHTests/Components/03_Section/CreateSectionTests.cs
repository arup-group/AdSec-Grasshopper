using System.Collections.Generic;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;
using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateSectionTests {
    readonly CreateSection component;
    private IDesignCode DesignCode = IS456.Edition_2000;
    private IConcrete SectionMat = Concrete.IS456.Edition_2000.M10;
    private ISteel iBeamMat = Steel.AS4100.Edition_1998.AS1163_C250;

    public CreateSectionTests() {
      component = new CreateSection();
      var profile = new ProfileBuilder().WidthDepth(1).WithWidth(2).Build();
      var profileDesign = new ProfileDesign() { Profile = profile, LocalPlane = OasysPlane.PlaneYZ };
      component.SetInputParamAt(0, new AdSecProfileGoo(profileDesign));
      var adSecMaterial = new AdSecMaterialGoo(new MaterialDesign() { Material = SectionMat, DesignCode = DesignCode });
      component.SetInputParamAt(1, adSecMaterial);
    }

    [Fact]
    public void ShouldNotHaveAnyErrors() {
      ComponentTesting.ComputeOutputs(component);
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveOutputWithoutOptional() {
      ComponentTesting.ComputeOutputs(component);
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void ShouldHaveOutputWithOptional() {
      var iBeamSymmetricalProfile = ProfileBuilder.GetIBeam();
      var section = new SectionBuilder().SetProfile(iBeamSymmetricalProfile).WithMaterial(iBeamMat).Build();
      var subComponent = new SubComponent() {
        SectionDesign = new SectionDesign() {
          Section = section,
          DesignCode = DesignCode,
          MaterialName = "AS1163_C250",
          CodeName = "AS4100",
          LocalPlane = OasysPlane.PlaneYZ
        },
        ISubComponent = ISubComponent.Create(section, Geometry.Zero())
      };
      var subComponentGoo = new AdSecSubComponentGoo(subComponent);
      component.SetInputParamAt(3, subComponentGoo);
      ComponentTesting.ComputeOutputs(component);
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
