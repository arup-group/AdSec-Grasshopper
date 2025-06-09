using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using Oasys.GH.Helpers;

using Xunit;

[Collection("GrasshopperFixture collection")]
public class OpenModelTests {
  private OpenModel _component;

  public OpenModelTests() {
    _component = new OpenModel();
  }

  [Fact]
  public void ShouldCretePanelWhenPathIsSpecified() {
    var doc = new GH_DocumentIO();
    doc.Document = new GH_Document();
    doc.Document.AddObject(_component, true);
    _component.OpenFile("path/to/file.adsec");
    Assert.Equal(2, doc.Document.Objects.Count);
    Assert.Single(_component.Params.Input[0].Sources);
    var ghParam = _component.Params.Input[0].Sources[0];
    Assert.IsType<GH_Panel>(ghParam);
    var panel = (GH_Panel)ghParam;
    Assert.Equal("path/to/file.adsec", panel.UserText);
  }

  [Fact]
  public void ShouldHavePluginInfoReferenced() {
    Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
  }

  [Fact]
  public void ShouldHaveIconReferenced() {
    Assert.True(_component.MatchesExpectedIcon(Resources.OpenAdSec));
  }

  [Fact]
  public void ShouldInitializeAttributes() {
    _component.Attributes = null;
    _component.CreateAttributes();
    Assert.NotNull(_component.Attributes);
  }

  [Fact]
  public void ShouldHaveTwoInputs() {
    Assert.Equal(2, _component.Params.Input.Count);
  }
}
