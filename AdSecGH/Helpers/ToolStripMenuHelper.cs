using System.Windows.Forms;

namespace AdSecGH.Helpers {
  public static class ToolStripMenuHelper {
    public static ToolStripMenuItem CreateInvisibleMenuItem() {
      const string notAvailable = "Not available";
      var item = new ToolStripMenuItem {
        Text = notAvailable,
        Visible = false,
      };
      return item;
    }
  }
}
