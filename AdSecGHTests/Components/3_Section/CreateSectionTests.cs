using System.Linq;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysGH.Units;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateSectionTests {
    private readonly CreateSection component;
    private readonly IDesignCode DesignCode = IS456.Edition_2000;
    private readonly ISteel iBeamMat = Steel.AS4100.Edition_1998.AS1163_C250;
    private readonly IConcrete SectionMat = Concrete.IS456.Edition_2000.M10;

    public CreateSectionTests() {
      component = new CreateSection();
      var profile = new ProfileBuilder().WidthDepth(1).WithWidth(2).Build();
      var profileDesign = new ProfileDesign { Profile = profile, LocalPlane = OasysPlane.PlaneYZ, };
      component.SetInputParamAt(0, new AdSecProfileGoo(profileDesign));
      var adSecMaterial = new AdSecMaterialGoo(new MaterialDesign { Material = SectionMat, DesignCode = new DesignCode { IDesignCode = DesignCode, DesignCodeName = null, }, });
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
    public void ShouldHaveOutputWithReinforcementGroups() {
      var reinforcementGroupGoo = new AdSecRebarGroupGoo(new BuilderLineGroup().Build());
      component.SetInputParamAt(2, reinforcementGroupGoo);
      ComponentTesting.ComputeOutputs(component);
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void ShouldHaveOutputWithSubComponentThroughSection() {
      var sectionDesign = SampleData.GetSectionDesign(DesignCode, iBeamMat);
      var subComponentGoo = new AdSecSectionGoo(new AdSecSection(sectionDesign));
      component.SetInputParamAt(3, subComponentGoo);
      ComponentTesting.ComputeOutputs(component);
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void ShouldHaveOutputWithSubComponent() {
      var sectionDesign = SampleData.GetSectionDesign(DesignCode, iBeamMat);
      var subComponent = new SubComponent {
        SectionDesign = sectionDesign,
        ISubComponent = ISubComponent.Create(sectionDesign.Section, Geometry.Zero()),
      };
      var subComponentGoo = new AdSecSubComponentGoo(subComponent);
      component.SetInputParamAt(3, subComponentGoo);
      ComponentTesting.ComputeOutputs(component);
      Assert.NotNull(component.GetOutputParamAt(0));
    }

    [Fact]
    public void ShouldRemoveSubComponentIfDisconnected() {
      var sectionDesign = SampleData.GetSectionDesign(DesignCode, iBeamMat);
      var subComponent = new SubComponent {
        SectionDesign = sectionDesign,
        ISubComponent = ISubComponent.Create(sectionDesign.Section, Geometry.Zero()),
      };
      var subComponentGoo = new AdSecSubComponentGoo(subComponent);
      component.SetInputParamAt(3, subComponentGoo);
      var sectionOutWithSubComponent = (AdSecSectionGoo)ComponentTestHelper.GetOutput(component);
      Assert.Single(sectionOutWithSubComponent.Value.Section.SubComponents);
      ComponentTestHelper.DisconnectInput(component, 3);
      ComponentTesting.ComputeOutputs(component);
      var sectionOut = (AdSecSectionGoo)ComponentTestHelper.GetOutput(component);
      Assert.Empty(sectionOut.Value.Section.SubComponents);
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
      Assert.Equal(PluginInfo.Instance, component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(component.MatchesExpectedIcon(Resources.CreateSection));
    }

    [Fact]
    public void ShouldCalculateOffsetsWhenProfilePlaneAndRebarPlaneNotSame() {

      var profile = new ProfileBuilder().WidthDepth(400).WithWidth(300).Build();
      var globalPlane = Plane.WorldYZ;
      var Y = new Length(.15, DefaultUnits.LengthUnitGeometry);
      var Z = new Length(0.2, DefaultUnits.LengthUnitGeometry);
      globalPlane.Origin = new Point3d(0, -Y.Value, -Z.Value);

      var profileDesign = new ProfileDesign {
        Profile = profile,
        GlobalPlane = globalPlane.ToOasys(),
        LocalPlane = globalPlane.ToOasys()
      };

      var profileGoo = new AdSecProfileGoo(profileDesign);
      var component = new CreateSection();

      ComponentTestHelper.SetInput(component, profileGoo);
      var adSecMaterial = new AdSecMaterialGoo(new MaterialDesign {
        Material = SectionMat,
        DesignCode = new DesignCode { IDesignCode = DesignCode, DesignCodeName = string.Empty }
      });

      ComponentTestHelper.SetInput(component, adSecMaterial, 1);

      var barBuilder = new BuilderSingleBar();
      var singleBar = barBuilder.WithSize(16).AtPosition(IPoint.Create(Y, Z)).Build();

      ComponentTestHelper.SetInput(component, new AdSecRebarGroupGoo(singleBar), 2);

      AdSecSectionGoo section = (AdSecSectionGoo)ComponentTestHelper.GetOutput(component);

      Assert.NotNull(section);
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      section.Value.Section.ReinforcementGroups.ToList().ForEach(group => {
        var singleBars = group as ISingleBars;
        Assert.NotNull(singleBars);
        Assert.Single(singleBars.Positions);
        Assert.Equal(300, singleBars.Positions[0].Y.ToUnit(LengthUnit.Millimeter).Value, 1);
        Assert.Equal(400, singleBars.Positions[0].Z.ToUnit(LengthUnit.Millimeter).Value, 1);
      });
    }
  }
}
