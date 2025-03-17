using System.Drawing;
using System.Reflection;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;
using Oasys.GH.Helpers;

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
    public void ShouldHaveOutputWithOptionalSubComponentAsNull() {
      component.SetInputParamAt(3, null);
      ComponentTesting.ComputeOutputs(component);
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void ShouldHaveOutputWithSubComponentThroughSection() {
      var iBeamSymmetricalProfile = ProfileBuilder.GetIBeam();
      var section = new SectionBuilder().SetProfile(iBeamSymmetricalProfile).WithMaterial(iBeamMat).Build();
      var sectionDesign = new SectionDesign() {
        Section = section,
        DesignCode = DesignCode,
        MaterialName = "AS1163_C250",
        CodeName = "AS4100",
      };
      var subComponentGoo = new AdSecSectionGoo(new AdSecSection(sectionDesign));
      component.SetInputParamAt(3, subComponentGoo);
      ComponentTesting.ComputeOutputs(component);
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void ShouldHaveOutputWithReinforcementGroups() {
      var reinforcementGroup = new BuilderLineGroup().Build();
      var reinforcementGroupGoo = new AdSecRebarGroupGoo(reinforcementGroup);
      component.SetInputParamAt(2, reinforcementGroupGoo);
      ComponentTesting.ComputeOutputs(component);
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void ShouldHaveOutputWithSubComponent() {
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

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(AdSecGH.PluginInfo.Instance, component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(MatchesExpectedIcon(component, Resources.CreateFlange));
    }

    public static bool MatchesExpectedIcon(GH_Component component, Bitmap expected) {
      var propertyInfo = component.GetType().GetProperty("Icon", BindingFlags.Instance | BindingFlags.NonPublic);
      var icon = (Bitmap)(propertyInfo?.GetValue(component, null));
      var expectedRawFormat = expected.RawFormat;
      return expectedRawFormat.Guid.Equals(icon?.RawFormat.Guid);
    }
  }
}
