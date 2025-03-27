using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using AdSecGHCore;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.Reinforcement.Preloads;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

namespace AdSecCoreTests.Functions {
  public class EditSectionFunctionTests {

    private readonly EditSectionFunction function;

    public EditSectionFunctionTests() {
      function = new EditSectionFunction();
      function.Section.Value = SampleData.GetSectionDesign();
    }

    [Fact]
    public void ShouldPassProfile() {
      function.Compute();
      Assert.NotNull(function.ProfileOut.Value);
    }

    [Fact]
    public void ShouldPassMaterial() {
      function.Compute();
      Assert.NotNull(function.MaterialOut.Value);
    }

    [Fact]
    public void ShouldPassDesignCode() {
      function.Compute();
      Assert.NotNull(function.DesignCodeOut.Value);
    }

    [Fact]
    public void ShouldReturnRebarGroup() {
      function.Compute();
      Assert.NotNull(function.RebarGroupOut.Value);
    }

    [Fact]
    public void ShouldPassSubComponent() {
      function.Compute();
      Assert.NotNull(function.SubComponentOut.Value);
    }

    [Fact]
    public void ShouldNOTCalculateGeometry() {
      function.Compute();
      Assert.Null(function.Geometry.Value);
    }

    [Fact]
    public void ShouldHaveSixInputs() {
      function.Compute();
      Assert.Equal(6, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveSevenOutputs() {
      function.Compute();
      Assert.Equal(7, function.GetAllOutputAttributes().Length);
    }

    [Fact]
    public void ShouldUpdateProfile() {
      var newProfile = ProfileBuilder.GetIBeam();
      function.Profile.Value = new ProfileDesign {
        Profile = newProfile,
      };
      function.Compute();
      Assert.Equal(newProfile, function.ProfileOut.Value.Profile);
      var profile = (IIBeamSymmetricalProfile)function.SectionOut.Value.Section.Profile;
      Assert.True(Equal(newProfile, profile));
    }

    [Fact]
    public void ShouldUpdateMaterial() {
      var newMat = Steel.AS4100.Edition_1998.AS1163_C250;
      function.Material.Value = new MaterialDesign {
        Material = newMat,
        DesignCode = function.Section.Value.DesignCode,
      };
      function.Compute();
      Assert.Equal(newMat, function.MaterialOut.Value.Material);
      Assert.Equal(newMat, function.SectionOut.Value.Section.Material);
      Assert.True(Equal(newMat, function.SectionOut.Value.Section.Material));
    }

    [Fact]
    public void ShouldUpdateDesignCode() {
      var newCode = AS3600.Edition_2001;
      function.DesignCode.Value = new DesignCode {
        IDesignCode = newCode,
      };
      function.Compute();
      Assert.Equal(newCode, function.DesignCodeOut.Value.IDesignCode);
      Assert.Equal(newCode, function.MaterialOut.Value.DesignCode.IDesignCode);
    }

    [Fact]
    public void ShouldUpdateRebarGroup() {
      var adSecRebarGroup = new AdSecRebarGroup {
        Group = new BuilderLineGroup().Build(),
      };
      function.RebarGroup.Value = new[] {
        adSecRebarGroup,
      };
      function.Compute();
      Assert.Equal(adSecRebarGroup, function.RebarGroupOut.Value[0]);
      Assert.True(Equal((ILineGroup)adSecRebarGroup.Group,
        (ILineGroup)function.SectionOut.Value.Section.ReinforcementGroups[0]));
    }

    [Fact(Skip = "Not Equality method finished")]
    public void ShouldUpdateSubComponent() {
      var subComponent = SampleData.GetSubComponentZero();
      function.SubComponent.Value = new[] {
        subComponent,
      };
      function.Compute();
      Assert.Equal(subComponent, function.SubComponentOut.Value[0]);
      Assert.True(Equal(subComponent.ISubComponent, function.SectionOut.Value.Section.SubComponents[0]));
    }

    [Fact]
    public void ShouldHaveAllButFirstParametersOptional() {
      Assert.True(AllButFirstOptional());
    }

    public static bool Equal(IIBeamSymmetricalProfile flanges, IIBeamSymmetricalProfile flanges2) {
      return Equal(flanges.BottomFlange, flanges2.BottomFlange) && Equal(flanges.TopFlange, flanges2.TopFlange)
        && Equal(flanges.Web, flanges2.Web) && Equals(flanges.Depth, flanges2.Depth)
        && Equals(flanges.Rotation, flanges2.Rotation) && Equals(flanges.IsReflectedY, flanges2.IsReflectedY)
        && Equals(flanges.IsReflectedZ, flanges2.IsReflectedZ);
    }

    public static bool Equal(IWebConstant web, IWebConstant web2) {
      return Equals(web.Thickness, web2.Thickness) && Equals(web.BottomThickness, web2.BottomThickness)
        && Equals(web.TopThickness, web2.TopThickness);
    }

    public static bool Equal(IFlange flange, IFlange flange2) {
      return Equals(flange.Thickness, flange2.Thickness) && Equals(flange.Width, flange2.Width);
    }

    public static bool Equal(ISubComponent subComponent, ISubComponent subComponent2) {
      return Equals(subComponent.Offset, subComponent2.Offset) && Equal(subComponent.Section, subComponent2.Section);
    }

    public static bool Equal(ISection section, ISection section2) {
      return Equals(section.Cover, section2.Cover) && Equal(section.Material, section2.Material)
        && Equals(section.Profile, section2.Profile)
        && Equal((ILineGroup)section.ReinforcementGroups[0], (ILineGroup)section2.ReinforcementGroups[0])
        && Equal(section.SubComponents[0], section2.SubComponents[0]);
    }

    public static bool Equal(ILayer layer, ILayer layer2) {
      return Equals(layer.BarBundle.CountPerBundle, layer2.BarBundle.CountPerBundle)
        && Equals(layer.BarBundle.Diameter, layer2.BarBundle.Diameter)
        && Equal(layer.BarBundle.Material, layer2.BarBundle.Material);
    }

    public static bool Equal(ILineGroup group, ILineGroup group2) {
      bool equals = Equal(group.Layer, group2.Layer) && Equal(group.FirstBarPosition, group2.FirstBarPosition)
        && Equal(group.LastBarPosition, group2.LastBarPosition) && Equal(group.Preload, group2.Preload);

      return equals;
    }

    public static bool Equal(IPreload preload, IPreload preload2) {
      if (preload is IPreForce force) {
        return Equal(force, (IPreForce)preload2);
      }

      if (preload is IPreStress stress) {
        return Equal(stress, (IPreStress)preload2);
      }

      if (preload is IPreStrain strain) {
        return Equal(strain, (IPreStrain)preload2);
      }

      return false;
    }

    public static bool Equal(IPreForce preload, IPreForce preload2) {
      return Equals(preload.Force, preload2.Force);
    }

    public static bool Equal(IPreStrain preload, IPreStrain preload2) {
      return Equals(preload.Strain, preload2.Strain);
    }

    public static bool Equal(IPreStress preload, IPreStress preload2) {
      return Equals(preload.Stress, preload2.Stress);
    }

    public static bool Equal(IPoint point, IPoint point2) {
      return Equals(point.Y, point2.Y) && Equals(point.Z, point2.Z);
    }

    public static bool Equal(IMaterial material, IMaterial material2) {
      var serviceability1 = material.Serviceability;
      var serviceability2 = material2.Serviceability;

      return Equals(serviceability1.Tension.FailureStrain, serviceability2.Tension.FailureStrain)
        && Equals(serviceability1.Compression.FailureStrain, serviceability2.Compression.FailureStrain)
        && Equals(material.Strength.Compression.FailureStrain, material2.Strength.Compression.FailureStrain);
    }

    private static bool AllButFirstOptional() {
      var function = new EditSectionFunction();
      var inputAttributes = function.GetAllInputAttributes();
      for (int i = 1; i < inputAttributes.Length; i++) {
        var attribute = inputAttributes[i];
        if (!attribute.Optional) {
          return false;
        }
      }

      return true;
    }
  }
}
