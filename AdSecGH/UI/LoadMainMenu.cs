using System.Threading;
using System.Windows.Forms;

using AdSecGH.Properties;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;

namespace AdSecGH.Graphics.Menu {
  public static class MenuLoad {
    internal static void OnStartup(GH_Canvas canvas) {
      var oasysMenu = new ToolStripMenuItem("Oasys") {
        Name = "Oasys",
      };

      PopulateSub(oasysMenu);

      GH_DocumentEditor editor = null;

      while (editor == null) {
        editor = Instances.DocumentEditor;
        Thread.Sleep(321);
      }

      if (!editor.MainMenuStrip.Items.ContainsKey("Oasys")) {
        editor.MainMenuStrip.Items.Add(oasysMenu);
      } else {
#pragma warning disable S3998 // Threads should not lock on objects with weak identity
        oasysMenu = (ToolStripMenuItem)editor.MainMenuStrip.Items["Oasys"];
        lock (oasysMenu) {
          oasysMenu.DropDown.Items.Add(new ToolStripSeparator());
          PopulateSub(oasysMenu);
        }
#pragma warning restore S3998 // Threads should not lock on objects with weak identity
      }

      Instances.CanvasCreated -= OnStartup;
    }

    private static void PopulateSub(ToolStripDropDownItem menuItem) {
      // add info
      menuItem.DropDown.Items.Add("AdSecGH Info", Resources.AdSecInfo, (s, a) => {
        var aboutBox = new AboutBox();
        aboutBox.ShowDialog();
      });
    }
  }
}
