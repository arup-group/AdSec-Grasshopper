using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Components._3_Rebar;
using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;
using Oasys.Taxonomy.Profiles;

using OasysGH.Parameters;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components._2_Profile {
  [Collection("GrasshopperFixture collection")]
  public class EditProfileTests {

    private readonly EditProfile _component;
    private readonly AdSecProfileGoo _profileGoo = CreateProfileGoo();
    public EditProfileTests() {
      _component = new EditProfile();
      InitializeComponent(_component);
    }

    private void InitializeComponent(EditProfile component) {
      component.SetInputParamAt(0, _profileGoo);
      ComponentTestHelper.ComputeData(_component);
    }

    private static AdSecProfileGoo CreateProfileGoo() {
      var profile = ProfileDesign.From(new SectionDesign() {
        Section = new SectionBuilder().WithHeight(1).WithWidth(1).Build()
      });
      return new AdSecProfileGoo(profile);
    }

    [Fact]
    public void ShouldParseOasysTaxonomy() {
      var component = new EditProfile();
      var length = new Length(1, LengthUnit.Meter);
      var profile = new OasysProfileGoo(new RectangleProfile(length, length));
      component.SetInputParamAt(0, profile);
      var result = (AdSecProfileGoo)ComponentTestHelper.GetOutput(component);
      Assert.NotNull(result);
    }

    [Fact]
    public void ShouldHaveBusinessComponent() {
      Assert.NotNull(_component.BusinessComponent);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.EditProfile));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveFourInputs() {
      Assert.Equal(4, _component.Params.Input.Count);
    }

    [Fact]
    public void ShouldUpdateNameWhenChangingDropdownToRad() {
      _component.SetSelected(0, 1);
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("°", _component.Params.Input[1].Name);
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
    public void ShouldBeAbleToGetPlaneWithNoInputs() {
      Assert.NotNull(_component.GetValue<AdSecProfileGoo>());
    }

    [Fact]
    public void RectangleModelShouldUseSavedMode() {
      _component.SetSelected(0, 1);
      var doc = new GH_DocumentIO();
      doc.Document = new GH_Document();
      doc.Document.AddObject(_component, false);
      var randomPath = CreateRebarGroupSaveLoadTests.GetRandomName();
      doc.SaveQuiet(randomPath);
      doc.Open(randomPath);
      doc.Document.NewSolution(true);
      var component = (EditProfile)doc.Document.FindComponent(_component.InstanceGuid);
      InitializeComponent(component);
      Assert.Equal(AngleUnit.Degree, component.BusinessComponent.LocalAngleUnit);
    }

    [Fact]
    public void ShouldNotModifyUpstreamObject() {
      _component.SetInputParamAt(1, 2);
      ComponentTestHelper.ComputeData(_component);
      var extractedProfileGoo = (AdSecProfileGoo)ComponentTestHelper.GetOutput(_component);
      Assert.NotEqual(_profileGoo.Rotation, extractedProfileGoo.Rotation);
    }
  }
}
