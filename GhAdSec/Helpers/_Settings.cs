using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Oasys.AdSec;

namespace AdSecGH.Helpers
{
    public class Loader
    {
        System.Timers.Timer loadTimer;
        System.Timers.Timer trimTimer;
        static bool MenuHasBeenAdded = false;

        public Loader() { }

        public void CreateMainMenuItem()
        {
            loadTimer = new System.Timers.Timer(500);
            loadTimer.Start();
            loadTimer.Elapsed += AddMenuItem;
            trimTimer = new System.Timers.Timer(500);
            trimTimer.Elapsed += TrimMenuItem;
        }
        private void TrimMenuItem(object sender, ElapsedEventArgs e)
        {
            var mainMenu = Grasshopper.Instances.DocumentEditor.MainMenuStrip;
            bool removeNext = false;
            for (int i = 0; i < mainMenu.Items.Count; i++)
            {
                if (mainMenu.Items[i].ToString() == "Oasys")
                {
                    if (!removeNext)
                    {
                        removeNext = true;
                    }
                    else
                    {
                        mainMenu.Items.RemoveAt(i);
                        i--;
                    }
                }
            }
            trimTimer.Stop();
        }
        private void AddMenuItem(object sender, ElapsedEventArgs e)
        {
            if (Grasshopper.Instances.DocumentEditor == null) return;

            if (MenuHasBeenAdded)
            {
                loadTimer.Stop();
                trimTimer.Start();
                return;
            }

            // create menu
            var adSecMenu = new ToolStripMenuItem("Oasys");

            // add units
            adSecMenu.DropDown.Items.Add("AdSec Units", Properties.Resources.Units, (s, a) =>
            {
                AdSecGH.UI.UnitSettingsBox unitBox = new UI.UnitSettingsBox();
                unitBox.Show();
            });
            // add info
            adSecMenu.DropDown.Items.Add("AdSec Info", Properties.Resources.AdSecInfo, (s, a) =>
            {
                AdSecGH.UI.AboutAdSecBox aboutBox = new UI.AboutAdSecBox();
                aboutBox.Show();
            });

            //adSecMenu.DropDown.Items.Add(new ToolStripSeparator());

            //adSecMenu.DropDown.Items.Add("AdSec API Documentation", null, (s, a) =>
            //{
            //    Process.Start(@"https://arup-group.github.io/oasys-combined/adsec-api/api/");
            //});

            //adSecMenu.DropDown.Items.Add("Oasys website", null, (s, a) =>
            //{
            //    Process.Start(@"https://www.oasys-software.com/");
            //});

            var mainMenu = Grasshopper.Instances.DocumentEditor.MainMenuStrip;
            try
            {
                mainMenu.Items.Insert(mainMenu.Items.Count - 2, adSecMenu);
                MenuHasBeenAdded = true;
                loadTimer.Stop();
                trimTimer.Start();
            }
            catch (Exception)
            {
            }
        }
    }
}