using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using Oasys.GH.Helpers;

using Rhino.UI;

using Xunit;

namespace AdSecGHTests.Components {

  [Collection("GrasshopperFixture collection")]
  public class DialogGhTests {
    private readonly DialogGh _dialog;

    public DialogGhTests() {
      _dialog = new DialogGh();
    }

    [Fact]
    public void ShouldReturnFileNameFromDialog() {
      var dialog = new OpenFileDialog();
      dialog.FileName = "test.adsec";
      var _dialog = new DialogGh(dialog);
      Assert.Equal("test.adsec", _dialog.FileName);
    }

    [Fact]
    public void ShouldOnlyWorkWithAdsFiles() {
      Assert.Equal("AdSec Files(*.ads)|*.ads|All files (*.*)|*.*", _dialog._openFileDialog.Filter);
    }

    [Fact]
    public void ShouldHaveATitle() {
      Assert.Equal("Open AdSec Model", _dialog._openFileDialog.Title);
    }
  }

  [Collection("GrasshopperFixture collection")]
  public class OpenModelTests {
    private readonly OpenModel _component;
    private readonly GH_DocumentIO doc;
    private static readonly string _filePath = "path/to/file.adsec";

    public OpenModelTests() {
      doc = new GH_DocumentIO {
        Document = new GH_Document(),
      };
      _component = new OpenModel();
    }

    private class TestDialog : IShowDialog {
      public string FileName { get; set; }

      public bool ShowOpenDialog() {
        FileName = _filePath;
        return true;
      }
    }

    [Fact]
    public void ShouldOpenDialogWhenNull() {
      _component.OpenFileDialog = new TestDialog();
      doc.Document.AddObject(_component, true);
      _component.OpenFile();
      AssertPanel(_filePath);
    }

    [Fact]
    public void ShouldNotCreatePanelWhenEmpty() {
      doc.Document.AddObject(_component, true);
      _component.OpenFile("");
      Assert.Single(doc.Document.Objects);
      Assert.Empty(_component.Params.Input[0].Sources);
    }

    [Fact]
    public void ShouldCretePanelWhenPathIsSpecified() {
      doc.Document.AddObject(_component, true);
      string pathToFileAdsec = "path/to/file.adsec";
      _component.OpenFile(pathToFileAdsec);
      AssertPanel(pathToFileAdsec);
    }

    private void AssertPanel(string path) {
      Assert.Equal(2, doc.Document.Objects.Count);
      Assert.Single(_component.Params.Input[0].Sources);
      var ghParam = _component.Params.Input[0].Sources[0];
      Assert.IsType<GH_Panel>(ghParam);
      var panel = (GH_Panel)ghParam;
      Assert.Equal(path, panel.UserText);
    }

    [Fact]
    public void ShouldUpdateAndNotDestroyIt() {
      doc.Document.AddObject(_component, true);
      _component.OpenFile(_filePath);
      var ghParam = _component.Params.Input[0].Sources[0];
      var panel = (GH_Panel)ghParam;
      var guid = panel.InstanceGuid;
      AssertPanel(_filePath);
      string pathToFile2AdSec = "path/to/file2.adsec";
      _component.OpenFile(pathToFile2AdSec);
      var ghParam2 = _component.Params.Input[0].Sources[0];
      var panel2 = (GH_Panel)ghParam2;
      AssertPanel(pathToFile2AdSec);
      Assert.Equal(guid, panel2.InstanceGuid);
    }

    [Fact]
    public void ShouldCreateANewPanelIfPreviousDestroyed() {
      doc.Document.AddObject(_component, true);
      _component.OpenFile(_filePath);
      var ghParam = _component.Params.Input[0].Sources[0];
      var panel = (GH_Panel)ghParam;
      var myObject = doc.Document.FindObject(panel.InstanceGuid, false);
      doc.Document.RemoveObject(myObject, true);
      _component.OpenFile(_filePath);
      AssertPanel(_filePath);
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
}
