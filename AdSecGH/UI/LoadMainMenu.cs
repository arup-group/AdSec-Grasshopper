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
#pragma warning disable S2445 // Blocks should be synchronized on read-only fields
#pragma warning disable S3998 // Threads should not lock on objects with weak identity
        lock (oasysMenu) {
          oasysMenu.DropDown.Items.Add(new ToolStripSeparator());
          PopulateSub(oasysMenu);
        }
#pragma warning restore S3998 // Threads should not lock on objects with weak identity
#pragma warning restore S2445 // Blocks should be synchronized on read-only fields
      }

      Instances.CanvasCreated -= OnStartup;
    }

    private static void PopulateSub(ToolStripMenuItem menuItem) {
      // add documentation
      //menuItem.DropDown.Items.Add("AdSecGH Documentation", Properties.Resources.Documentation, (s, a) =>
      //{
      //  Process.Start(new ProcessStartInfo
      //  {
      //    FileName = "https://docs.oasys-software.com/structural/gsa/explanations/AdSecGH-introduction.html?source=grasshopper",
      //    UseShellExecute = true
      //  });
      //});
      // add example files
      //menuItem.DropDown.Items.Add("AdSecGH Example files", Properties.Resources.ExampleFiles, (s, a) =>
      //{
      //  Process.Start(new ProcessStartInfo
      //  {
      //    FileName = "https://github.com/arup-group/AdSec-Grasshopper/tree/main/ExampleFiles",
      //    UseShellExecute = true
      //  });
      //});
      // add info
      menuItem.DropDown.Items.Add("AdSecGH Info", Resources.AdSecInfo, (s, a) => {
        var aboutBox = new AboutBox();
        aboutBox.ShowDialog();
      });
    }
  }
}
