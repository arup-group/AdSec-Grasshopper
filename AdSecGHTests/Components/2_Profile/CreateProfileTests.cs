using System;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Rhino.Geometry;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateProfileTests {
    private readonly CreateProfile _component;
    private const string _catalogueMode = "Catalogue";
    private const string _otherMode = "Other";

    public CreateProfileTests() {
      _component = new CreateProfile();
      _component.CreateAttributes();
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
    public void ShouldSetMode1WhenOtherMode() {
      var dummyCreateProfile = new DummyCreateProfile();
      dummyCreateProfile.SetModeByName(_otherMode);
      dummyCreateProfile.Mode1Click();

      Assert.Equal(_catalogueMode, dummyCreateProfile.GetModeString());
    }

    [Fact]
    public void ShouldSetMode1WhenCatalogueMode() {
      var dummyCreateProfile = new DummyCreateProfile();
      dummyCreateProfile.SetModeByName(_catalogueMode);
      dummyCreateProfile.Mode1Click();

      Assert.Equal(_catalogueMode, dummyCreateProfile.GetModeString());
    }

    [Fact]
    public void ShouldSetMode2WhenCatalogueMode() {
      var dummyCreateProfile = new DummyCreateProfile();
      dummyCreateProfile.SetModeByName(_catalogueMode);
      dummyCreateProfile.Mode2Click();

      Assert.Equal(_otherMode, dummyCreateProfile.GetModeString());
    }

    [Fact]
    public void ShouldSetMode2WhenOtherMode() {
      var dummyCreateProfile = new DummyCreateProfile();
      dummyCreateProfile.SetModeByName(_otherMode);
      dummyCreateProfile.Mode2Click();

      Assert.Equal(_otherMode, dummyCreateProfile.GetModeString());
    }

    [Theory]
    [InlineData(0, "STD A(m) 0.11 0.11 0.11 0.11")] //angle
    [InlineData(1, "CAT 2CHB2B 2CHB2B381x102x110")] //catalogue
    [InlineData(2, "STD CH(m) 0.11 0.11 0.11 0.11")] //channel
    [InlineData(3, "STD CHS(m) 0.11 0.11")] //circle hollow
    [InlineData(4, "STD C(m) 0.11")] //circle
    [InlineData(5, "STD X(m) 0.11 0.11 0.11 0.11")] //Cruciform Symmetrical
    [InlineData(6, "STD OVAL(m) 0.11 0.11 0.11")] //Ellipse Hollow
    [InlineData(7, "STD E(m) 0.11 0.11 2")] //Ellipse
    [InlineData(8, "STD GC(m) 0.11 0.11 0.11 0.11")] //General C
    [InlineData(9, "STD GZ(m) 0.11 0.11 0.11 0.11 0.11 0.11")] //General Z
    [InlineData(10, "STD GI(m) 0.11 0.11 0.11 0.11 0.11 0.11")] //I Beam Asymmetrical
    [InlineData(11, "STD CB(m) 0.11 0.11 0.11 0.11 0.11 0.121")] //I Beam Cellular
    [InlineData(12, "STD I(m) 0.11 0.11 0.11 0.11")] //I Beam Symmetrical
    [InlineData(13,
      "GEO P(m) M(0.021213203435596|0.015) L(-0.021213203435596|-0.015) L(-0.0070710678118655|-0.005) ")] //Perimeter
    [InlineData(14, "STD RHS(m) 0.11 0.11 0.11 0.11")] //Rectangle Hollow
    [InlineData(15, "STD R(m) 0.11 0.11")] //Rectangle
    [InlineData(16, "STD RE(m) 0.11 0.11 0.11 0.11 2")] //Recto Ellipse
    [InlineData(17, "STD RC(m) 0.11 0.11")] //Recto Circle
    [InlineData(18, "STD SPW(m) 0.11 0.11 11")] //Secant Pile
    [InlineData(19, "STD SHT(m) 0.11 0.11 0.11 0.11 0.22 0.22")] //Sheet Pile
    [InlineData(20, "STD TR(m) 0.11 0.11 0.11")] //Trapezoid
    [InlineData(21, "STD T(m) 0.11 0.11 0.11 0.11")] //T Section
    public void SolveInternalShouldUpdateOutputsFor(int profileTypeIndex, string expectedDesc) {
      _component.SetSelected(0, profileTypeIndex);
      string[] splittedCode = expectedDesc.Split(' ');
      SetValidInputs($"{splittedCode[0]} {splittedCode[1]}"); //take first two parts of the string as code

      var result = (AdSecProfileGoo)ComponentTestHelper.GetOutput(_component);

      Assert.NotNull(result);
      Assert.Equal(expectedDesc, result.Value.Profile.Description());
      Assert.Equal(0, result.Value.LocalPlane.Origin.X);
      Assert.Equal(0, result.Value.LocalPlane.Origin.Y);
      Assert.Equal(0, result.Value.LocalPlane.Origin.Z);
    }

    //[Theory]
    //[InlineData(1)] //catalogue
    //[InlineData(3)] //circle hollow
    //public void ShouldClearMessagesBeforeComputing(int profileTypeIndex) {
    //  _component.SetSelected(0, profileTypeIndex);
    //  // Set invalid inputs to trigger errors
    //  ComponentTestHelper.SetInput(_component, -1);
    //  ComponentTestHelper.SetInput(_component, -1, 1);
    //  ComponentTestHelper.ComputeData(_component);
    //  Assert.NotEmpty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));

    //  // Set inputs to valid values
    //  SetValidInputs(false);

    //  Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    //}

    private void SetValidInputs(string code) {
      switch (code) {
        case "CAT 2CHB2B":
          SetInputsForCatalogue();
          break;
        case "GEO P(m)":
          SetInputsForPerimiter();
          break;
        case "STD SHT(m)":
          SetInputsForSheetPile();
          break;

        default:
          SetInputIfNotLocalPlane();
          break;
      }

      ComponentTestHelper.ComputeData(_component);
    }

    private void SetInputIfNotLocalPlane(int maxIndex = 0) {
      int inputCount = maxIndex > 0 ? maxIndex : _component.Params.Input.Count;

      for (int i = 0; i < inputCount; i++) {
        var input = _component.Params.Input[i];

        if (!input.Name.Contains("LocalPlane")) {
          ComponentTestHelper.SetInput(_component, 11, i);
        }
      }
    }

    private void SetInputsForSheetPile() {
      SetInputIfNotLocalPlane(4);
      const int inputIndex = 22;
      ComponentTestHelper.SetInput(_component, inputIndex, 4);
      ComponentTestHelper.SetInput(_component, inputIndex, 5);
    }

    private void SetInputsForPerimiter() {
      ComponentTestHelper.SetInput(_component,
        Surface.CreateExtrusionToPoint(new LineCurve(new Line(0, 0, 0, 2, 2, 2)), new Point3d(3, 3, 3)));
    }

    private void SetInputsForCatalogue() {
      ComponentTestHelper.SetInput(_component, 11);
      ComponentTestHelper.SetInput(_component, true, 1);
    }

    internal class DummyCreateProfile : CreateProfile {
      public void Mode1Click() {
        base.Mode1Clicked();
      }

      public void Mode2Click() {
        base.Mode2Clicked();
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
