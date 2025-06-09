using AdSecGH.Components;

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
}
