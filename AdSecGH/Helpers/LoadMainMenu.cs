using System.Threading;
using System.Windows.Forms;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;

namespace AdSecGH.UI.Menu
{
  public class MenuLoad
  {
    private static ToolStripMenuItem oasysMenu;
    internal static void OnStartup(GH_Canvas canvas)
    {
      oasysMenu = new ToolStripMenuItem("Oasys");
      oasysMenu.Name = "Oasys";
      
      PopulateSub(oasysMenu);

      GH_DocumentEditor editor = null;

      while (editor == null)
      {
        editor = Grasshopper.Instances.DocumentEditor;
        Thread.Sleep(750);
      }

      if (!editor.MainMenuStrip.Items.ContainsKey("Oasys"))
        editor.MainMenuStrip.Items.Add(oasysMenu);
      else
      {
        oasysMenu = (ToolStripMenuItem)editor.MainMenuStrip.Items["Oasys"];
        lock (oasysMenu)
        {
          oasysMenu.DropDown.Items.Add(new ToolStripSeparator());
          PopulateSub(oasysMenu);
        }
      }

      Grasshopper.Instances.CanvasCreated -= OnStartup;
    }

    private static void PopulateSub(ToolStripMenuItem menutItem)
    {
      // add units
      menutItem.DropDown.Items.Add("AdSec Units", Properties.Resources.Units, (s, a) =>
      {
        UnitSettingsBox unitBox = new UnitSettingsBox();
        unitBox.Show();
      });
      // add info
      menutItem.DropDown.Items.Add("AdSec Info", Properties.Resources.AdSecInfo, (s, a) =>
      {
        AboutBox aboutBox = new AboutBox();
        aboutBox.Show();
      });
    }
  }
}
