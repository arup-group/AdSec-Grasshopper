using System;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateProfileTests {

    private readonly CreateProfile _component;

    public CreateProfileTests() {
      _component = new CreateProfile();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.CreateProfile));
    }

    [Fact]
    public void ShouldHaveBusinessComponent() {
      Assert.NotNull(_component.BusinessComponent);
    }

    [Fact]
    public void ShouldHaveThreeInputs() {
      Assert.Equal(3, _component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveNoWarnings() {
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldUnregisterAllInputs() {
      var dummyCreateProfile = new DummyCreateProfile();
      dummyCreateProfile.DummyUnregisterAllInputs();

      Assert.Empty(dummyCreateProfile.Params.Input);
    }

    [Fact]
    public void ShouldUnregisterPlaneInput() {
      var dummyCreateProfile = new DummyCreateProfile();
      var planeInput = dummyCreateProfile.Params.Input[2];
      Assert.Equal(3, dummyCreateProfile.Params.Input.Count);

      dummyCreateProfile.DummyUnregisterPlaneInput();
      var inputsAfterRemoval = dummyCreateProfile.Params.Input;
      Assert.Equal(2, inputsAfterRemoval.Count);
      Assert.DoesNotContain(planeInput, inputsAfterRemoval);
    }

    [Fact]
    public void ShouldSetMode1() {
      var dummyCreateProfile = new DummyCreateProfile();
      dummyCreateProfile.Mode1Click();

      Assert.Equal("Catalogue", dummyCreateProfile.GetModeString());
    }

    [Fact]
    public void ShouldSetMode2WhenCatalogueMode() {
      var dummyCreateProfile = new DummyCreateProfile();
      dummyCreateProfile.SetModeByName("Catalogue");
      dummyCreateProfile.Mode2Click();

      Assert.Equal("Other", dummyCreateProfile.GetModeString());
    }

    [Fact]
    public void ShouldSetMode2WhenOtherMode() {
      var dummyCreateProfile = new DummyCreateProfile();
      dummyCreateProfile.SetModeByName("Other");
      dummyCreateProfile.Mode2Click();

      Assert.Equal("Other", dummyCreateProfile.GetModeString());
    }

    internal class DummyCreateProfile : CreateProfile {
      public void Mode1Click() {
        base.Mode1Clicked();
      }

      public void Mode2Click() {
        base.Mode2Clicked();
      }

      public void DummyUnregisterAllInputs() {
        UnregisterAllInputs();
      }

      public void DummyUnregisterPlaneInput() {
        UnregisterPlaneInput();
      }

      public string GetModeString() {
        return _mode.ToString();
      }

      public void SetModeByName(string name) {
        if (Enum.TryParse(name, out FoldMode result)) {
          _mode = result;
        }
      }
    }
  }
}
