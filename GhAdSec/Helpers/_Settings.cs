using Grasshopper.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace AdSecGH.Helpers
{
    public class SettingsMenu : IGH_SettingFrontend
    {
        public SettingsMenu() : base ()
        {
        }

        public string Category => "AdSec";

        public string Name => "AdSecName";

        public IEnumerable<string> Keywords => throw new NotImplementedException();

        public Control SettingsUI()
        {
            throw new NotImplementedException();
        }
    }
}
