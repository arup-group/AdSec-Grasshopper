using Rhino.UI;

namespace AdSecGH.Helpers {
  internal class GhFileHelper {
    private GhFileHelper() { }
    internal static string SaveFilePath() {
      var saveDialog = new SaveFileDialog {
        Filter = "AdSec File (*.ads)|*.ads|All files (*.*)|*.*",
      };
      return saveDialog.ShowSaveDialog() ? saveDialog.FileName : string.Empty;
    }
  }
}
