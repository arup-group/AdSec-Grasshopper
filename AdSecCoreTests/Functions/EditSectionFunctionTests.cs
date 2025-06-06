﻿using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using AdSecGHCore;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;

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
      var profile = function.SectionOut.Value.Section.Profile;
      Assert.True(Compare.Equal(newProfile, profile));
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
      Assert.True(Compare.Equal(newMat, function.SectionOut.Value.Section.Material));
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
      Assert.True(Compare.Equal((ILineGroup)adSecRebarGroup.Group,
        (ILineGroup)function.SectionOut.Value.Section.ReinforcementGroups[0]));
    }

    [Fact]
    public void ShouldUpdateSubComponent() {
      var subComponent = SampleData.GetSubComponentZero();
      function.SubComponent.Value = new[] {
        subComponent,
      };
      function.Compute();
      Assert.Equal(subComponent, function.SubComponentOut.Value[0]);
      Assert.True(Compare.Equal(subComponent.ISubComponent, function.SectionOut.Value.Section.SubComponents[0]));
    }

    [Fact]
    public void ShouldHaveAllButFirstParametersOptional() {
      Assert.True(AllButFirstOptional());
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
