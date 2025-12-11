using System;
using System.Linq;

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
    [InlineData(13, "GEO P(m) M(0|0) L(0.042426406871193|-0.03) L(0.028284271247462|-0.02) ")] //Perimeter
    [InlineData(14, "STD RHS(m) 0.11 0.11 0.11 0.11")] //Rectangle Hollow
    [InlineData(15, "STD R(m) 0.11 0.11")] //Rectangle
    [InlineData(16, "STD RE(m) 0.11 0.11 0.11 0.11 2")] //Recto Ellipse
    [InlineData(17, "STD RC(m) 0.11 0.11")] //Recto Circle
    [InlineData(18, "STD SPW(m) 0.11 0.11 11")] //Secant Pile
    [InlineData(19, "STD SHT(m) 0.11 0.11 0.11 0.11 0.22 0.22")] //Sheet Pile
    [InlineData(20, "STD TR(m) 0.11 0.11 0.11")] //Trapezoid
    [InlineData(21, "STD T(m) 0.11 0.11 0.11 0.11")] //T Section
    public void SolveInternalComputeValidData(int profileTypeIndex, string expectedDesc) {
      _component.SetSelected(0, profileTypeIndex);
      string[] splittedCode = expectedDesc.Split(' ');
      SetValidInputs($"{splittedCode[0]} {splittedCode[1]}"); //take first two parts of the string as code

      var result = (AdSecProfileGoo)ComponentTestHelper.GetOutput(_component);

      Assert.NotNull(result);
      Assert.Equal(expectedDesc, result.Value.Profile.Description());
      Assert.Equal(0, result.Value.LocalPlane.Origin.X);
      Assert.Equal(0, result.Value.LocalPlane.Origin.Y);
      Assert.Equal(0, result.Value.LocalPlane.Origin.Z);
      Assert.Equal(0, result.Value.LocalPlane.XAxis.X);
      Assert.Equal(1, result.Value.LocalPlane.XAxis.Y);
      Assert.Equal(0, result.Value.LocalPlane.XAxis.Z);
      Assert.Equal(0, result.Value.LocalPlane.YAxis.X);
      Assert.Equal(0, result.Value.LocalPlane.YAxis.Y);
      Assert.Equal(1, result.Value.LocalPlane.YAxis.Z);
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
    public void ClearMessagesBeforeComputing(int profileTypeIndex, string expectedDesc) {
      _component.SetSelected(0, profileTypeIndex);
      // Set invalid inputs to trigger errors
      ComponentTestHelper.SetInput(_component, "!");
      ComponentTestHelper.SetInput(_component, -11, 1);
      ComponentTestHelper.SetInput(_component, Plane.Unset, _component.Params.Input.Count - 1);
      ComponentTestHelper.ComputeData(_component);
      try {
        var result = (AdSecProfileGoo)ComponentTestHelper.GetOutput(_component);
        Assert.Fail($"We expect to have incorrect data, so it shouldn't have any result! But we got: {result.Value}");
      } catch {
        // no result found so we can continue
      }

      //change to something else to "reset" component
      int tempIndex = profileTypeIndex == 0 ? 1 : 0;
      _component.SetSelected(0, tempIndex);

      // Set inputs to valid values
      _component.SetSelected(0, profileTypeIndex);
      string[] splittedCode = expectedDesc.Split(' ');
      SetValidInputs($"{splittedCode[0]} {splittedCode[1]}"); //take first two parts of the string as code

      Assert.False(ComponentHasMessages(), "Component should NOT have any error or warning message.");
    }

    private bool ComponentHasMessages() {
      var ghParam0 = _component.Params.Input[0];
      var ghParam1 = _component.Params.Input[1];

      bool componentHasMessages = ghParam0.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any()
        || ghParam0.RuntimeMessages(GH_RuntimeMessageLevel.Warning).Any()
        || ghParam1.RuntimeMessages(GH_RuntimeMessageLevel.Error).Any()
        || ghParam1.RuntimeMessages(GH_RuntimeMessageLevel.Warning).Any();
      return componentHasMessages;
    }

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

      ComponentTestHelper.SetInput(_component, Plane.WorldYZ, _component.Params.Input.Count - 1);

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
      ComponentTestHelper.SetInput(_component, 22, 4);
      ComponentTestHelper.SetInput(_component, 22, 5);
    }

    private void SetInputsForPerimiter() {
      int input = _component.Params.Input.FindIndex(x => x.Name == "Boundary");
      if (input < 0) {
        Assert.Fail("Cannot find boundary input");
      }

      ComponentTestHelper.SetInput(_component,
        Surface.CreateExtrusionToPoint(new LineCurve(new Line(0, 0, 0, 2, 2, 2)), new Point3d(3, 3, 3)), input);
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
