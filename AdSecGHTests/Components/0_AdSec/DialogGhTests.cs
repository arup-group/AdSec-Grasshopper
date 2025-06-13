using AdSecGH.Components;

using Rhino.UI;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class DialogGhTests {
    private readonly DialogGh _dialog;
    private readonly OpenFileDialog _openfileDialog;

    public DialogGhTests() {
      _openfileDialog = new OpenFileDialog {
        FileName = "test.adsec",
      };
      _dialog = new DialogGh(_openfileDialog);
    }

    [Fact]
    public void ShouldReturnFileNameFromDialog() {
      Assert.Equal("test.adsec", _dialog.FileName);
    }

    [Fact]
    public void ShouldOnlyWorkWithAdsFiles() {
      Assert.Equal("AdSec Files(*.ads)|*.ads|All files (*.*)|*.*", _openfileDialog.Filter);
    }

    [Fact]
    public void ShouldHaveATitle() {
      Assert.Equal("Open AdSec Model", _openfileDialog.Title);
    }
  }
}
