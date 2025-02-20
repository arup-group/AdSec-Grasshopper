using System;
using System.Reflection;

using AdSecGH.Parameters;

using Grasshopper.Kernel.Types;

using Oasys.AdSec;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Parameters {
  [Collection("GrasshopperFixture collection")]
  public class AdSecParametersReflectionTests {

    public Type[] GoosWithNickname = {
      typeof(AdSecConcreteCrackCalculationParametersGoo),
      typeof(AdSecCrackGoo),
      typeof(AdSecDesignCodeGoo),
      typeof(AdSecLoadGoo),
      typeof(AdSecMaterialGoo),
    };

    public Type[] GoosWithoutNickname = {
      typeof(AdSecDeformationGoo),
      typeof(AdSecFailureSurfaceGoo),
      typeof(AdSecNMMCurveGoo),
      typeof(AdSecPointGoo),
      typeof(AdSecProfileFlangeGoo),
      typeof(AdSecProfileGoo),
      typeof(AdSecProfileWebGoo),
      typeof(AdSecRebarBundleGoo),
      typeof(AdSecRebarGroupGoo),
      typeof(AdSecRebarLayerGoo),
      typeof(AdSecSectionGoo),
      typeof(AdSecSolutionGoo),
      typeof(AdSecStressStrainCurveGoo),
      typeof(AdSecStressStrainPointGoo),
      typeof(AdSecSubComponentGoo),
    };

    public IGH_Goo[] InstanceOfGoos = {
      new AdSecLoadGoo(ILoad.Create(new Force(), new Moment(), new Moment()))
    };

    private static bool IsNullOrEmptyOrWhitespace(string value) {
      return string.IsNullOrWhiteSpace(value) || value == string.Empty;
    }

    [Fact]
    public void CheckInstancesDuplicate() {
      foreach (var goo in InstanceOfGoos) {
        var gooDuplicate = goo.Duplicate();
        Assert.NotNull(gooDuplicate);
        Assert.Equal(goo.ToString(), gooDuplicate.ToString());
      }
    }

    [Fact]
    public void NicknameProperty_ReturnsValidString() {
      foreach (var type in GoosWithNickname) {
        var property = type.GetProperty("NickName", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.False(IsNullOrEmptyOrWhitespace(value), $"Failed for {type}");
      }
    }

    [Fact]
    public void NameProperty_ReturnsValidString() {
      foreach (var type in GoosWithNickname) {
        var property = type.GetProperty("Name", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.False(string.IsNullOrEmpty(value), $"Failed for {type}");
      }
    }

    [Fact]
    public void DescriptionProperty_ReturnsValidString() {
      foreach (var type in GoosWithNickname) {
        var property = type.GetProperty("Description", BindingFlags.Static | BindingFlags.Public);
        string value = property?.GetValue(null) as string;

        Assert.False(string.IsNullOrEmpty(value), $"Failed for {type}");
      }
    }
  }
}
