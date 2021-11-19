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
        bool AppendToExistingMenu = false;
        public Loader() { }

        public void CreateMainMenuItem()
        {
            loadTimer = new System.Timers.Timer(500);
            loadTimer.Start();
            loadTimer.Elapsed += TryAddMenuItem;
            trimTimer = new System.Timers.Timer(500);
            trimTimer.Elapsed += TrimMenuItem;
        }
        private void TrimMenuItem(object sender, ElapsedEventArgs e)
        {
            var mainMenu = Grasshopper.Instances.DocumentEditor.MainMenuStrip;
            if (AppendToExistingMenu)
            {
                // return if menu has not yet been created
                if (mainMenu == null || mainMenu.Items.Count == 0)
                    return;
                // return if trim loop has not yet been run by GSA plugin
                int n = 0;
                for (int i = 0; i < mainMenu.Items.Count; i++)
                {
                    if (mainMenu.Items[i].ToString() == "Oasys")
                        n++;
                }
                if (n > 1) // if more than one Oasys menu exist we let GSA plugin handle removing that
                    return;
                else if (n == 1) // if there is just one Oasys menu we append to that one
                {
                    for (int i = 0; i < mainMenu.Items.Count; i++)
                    {
                        if (mainMenu.Items[i].ToString() == "Oasys")
                        {
                            ToolStripMenuItem oasysMenu = mainMenu.Items[i] as ToolStripMenuItem;
                            if (oasysMenu.DropDown.Items.Count == 2)
                            {
                                // add separator first
                                oasysMenu.DropDown.Items.Add(GH_DocumentObject.Menu_AppendSeparator(oasysMenu.DropDown));
                                // append AdSec menu items
                                AddMenuItem(oasysMenu, sender, e);
                            }
                        }
                    }
                }
                else
                    return;
            }
            else
            {
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
            }
            
            trimTimer.Stop();

            if (AppendToExistingMenu)
            {
                for (int i = 0; i < mainMenu.Items.Count; i++)
                {
                    if (mainMenu.Items[i].ToString() == "Oasys")
                    {
                        ToolStripMenuItem oasysMenu = mainMenu.Items[i] as ToolStripMenuItem;
                        while (oasysMenu.DropDown.Items.Count > 5)
                            oasysMenu.DropDown.Items.RemoveAt(5);
                    }
                }
            }
        }
        private void TryAddMenuItem(object sender, ElapsedEventArgs e)
        {
            if (Grasshopper.Instances.DocumentEditor == null) return;

            if (MenuHasBeenAdded)
            {
                loadTimer.Stop();
                trimTimer.Start();
                return;
            }

            // check if GSA plugin is installed, then we want to append to existing menu
            GH_AssemblyInfo gsaPlugin = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("a3b08c32-f7de-4b00-b415-f8b466f05e9f"));
            if (gsaPlugin != null)
            {
                AppendToExistingMenu = true;
                loadTimer.Stop();
                trimTimer.Start();
                return;
            }
            else
            {
                ToolStripMenuItem oasysMenu = AddMenuItem(new ToolStripMenuItem("Oasys"), sender, e);
                
                // get main menu
                var mainMenu = Grasshopper.Instances.DocumentEditor.MainMenuStrip;
                
                try
                {
                    mainMenu.Items.Insert(mainMenu.Items.Count - 2, oasysMenu);
                    MenuHasBeenAdded = true;
                    loadTimer.Stop();
                    trimTimer.Start();
                }
                catch (Exception)
                {
                }
            }
        }

        private ToolStripMenuItem AddMenuItem(ToolStripMenuItem oasysMenu, object sender, ElapsedEventArgs e)
        {
            // add units
            oasysMenu.DropDown.Items.Add("AdSec Units", Properties.Resources.Units, (s, a) =>
            {
                AdSecGH.UI.UnitSettingsBox unitBox = new UI.UnitSettingsBox();
                unitBox.Show();
            });
            // add info
            oasysMenu.DropDown.Items.Add("AdSec Info", Properties.Resources.AdSecInfo, (s, a) =>
            {
                AdSecGH.UI.AboutAdSecBox aboutBox = new UI.AboutAdSecBox();
                aboutBox.Show();
            });

            return oasysMenu;
        }
    }
}