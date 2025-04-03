using System.Threading;
using System.Windows.Forms;

using AdSecGH.Properties;
using AdSecGH.UI;

using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;

namespace AdSecGH.Graphics.Menu {
  public class MenuLoad {
    private static ToolStripMenuItem oasysMenu;

    internal static void OnStartup(GH_Canvas canvas) {
      oasysMenu = new ToolStripMenuItem("Oasys") {
        Name = "Oasys"
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
        oasysMenu = (ToolStripMenuItem)editor.MainMenuStrip.Items["Oasys"];
        lock (oasysMenu) {
          oasysMenu.DropDown.Items.Add(new ToolStripSeparator());
          PopulateSub(oasysMenu);
        }
      }

      Instances.CanvasCreated -= OnStartup;
    }

    private static void PopulateSub(ToolStripMenuItem menuItem) {
      // add info
      menuItem.DropDown.Items.Add("AdSecGH Info", Resources.AdSecInfo, (s, a) => {
        var aboutBox = new AboutBox();
        aboutBox.ShowDialog();
      });
    }
  }
}
