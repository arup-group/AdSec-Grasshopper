using System;
using System.Reflection;
using System.Timers;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace AdSecGH.Helpers
{
    public class Loader
    {
        System.Timers.Timer menuLoadTimer;
        System.Timers.Timer menuTrimTimer;
        static bool MenuHasBeenAdded = false;
        bool AppendToExistingMenu = false;
        int n_existingMenus = 0;
        public Loader() { }

        internal void CreateMainMenuItem()
        {
            menuLoadTimer = new System.Timers.Timer(500);
            menuLoadTimer.Elapsed += TryAddMenuItem;
            menuTrimTimer = new System.Timers.Timer(500);
            menuTrimTimer.Elapsed += TrimMenuItem;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assemblies = currentDomain.GetAssemblies();
            foreach (Assembly ass in currentDomain.GetAssemblies())
            {
                if (ass.FullName.StartsWith("GsaGH"))
                {
                    AppendToExistingMenu = true;
                    n_existingMenus += 2;
                }
                if (ass.FullName.StartsWith("ComposGH"))
                {
                    AppendToExistingMenu = true;
                    if (n_existingMenus > 0)
                        n_existingMenus += 3;
                    else
                        n_existingMenus += 2;
                }
            }

            if (!AppendToExistingMenu)
                menuLoadTimer.Start();
            else
                menuTrimTimer.Start();
        }

        private void TryAddMenuItem(object sender, ElapsedEventArgs e)
        {
            if (Grasshopper.Instances.DocumentEditor == null) return; // return if document editor hasn't yet loaded
            if (Grasshopper.Instances.ActiveCanvas == null) return; // return if canvas has not yet been created
            if (Grasshopper.Instances.ComponentServer.
                FindAssembly(AdSecGHInfo.GUID) == null) return; // return if this plugin has not yet been loaded

            if (MenuHasBeenAdded)
            {
                menuLoadTimer.Stop();
                menuTrimTimer.Start();
                return;
            }

            if (!AppendToExistingMenu)
            {
                ToolStripMenuItem oasysMenu = AddMenuItem(new ToolStripMenuItem("Oasys"), sender, e);

                // get main menu
                var mainMenu = Grasshopper.Instances.DocumentEditor.MainMenuStrip;

                try
                {
                    mainMenu.Items.Insert(mainMenu.Items.Count - 2, oasysMenu);
                    MenuHasBeenAdded = true;

                }
                catch (Exception)
                {
                }
            }

            menuLoadTimer.Stop();
            menuTrimTimer.Start();
        }

        private ToolStripMenuItem AddMenuItem(ToolStripMenuItem oasysMenu, object sender, ElapsedEventArgs e)
        {
            // add units
            oasysMenu.DropDown.Items.Add("AdSec Units", Properties.Resources.Units, (s, a) =>
            {
                UI.UnitSettingsBox unitBox = new UI.UnitSettingsBox();
                unitBox.Show();
            });
            // add info
            oasysMenu.DropDown.Items.Add("AdSec Info", Properties.Resources.AdSecInfo, (s, a) =>
            {
                UI.AboutBox aboutBox = new UI.AboutBox();
                aboutBox.Show();
            });

            return oasysMenu;
        }

        private void TrimMenuItem(object sender, ElapsedEventArgs e)
        {
            if (Grasshopper.Instances.DocumentEditor == null) return;
            if (Grasshopper.Instances.ActiveCanvas == null) return;

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
                            if (oasysMenu.DropDown.Items.Count == n_existingMenus)
                            {
                                // add separator first
                                oasysMenu.DropDown.Items.Add(GH_DocumentObject.Menu_AppendSeparator(oasysMenu.DropDown));
                                // append Compos menu items
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

            menuTrimTimer.Stop();

            if (AppendToExistingMenu)
            {
                for (int i = 0; i < mainMenu.Items.Count; i++)
                {
                    if (mainMenu.Items[i].ToString() == "Oasys")
                    {
                        ToolStripMenuItem oasysMenu = mainMenu.Items[i] as ToolStripMenuItem;
                        while (oasysMenu.DropDown.Items.Count > n_existingMenus + 3)
                            oasysMenu.DropDown.Items.RemoveAt(n_existingMenus + 3);
                    }
                }
            }
        }
        //public class Loader
        //{
        //    System.Timers.Timer menuLoadTimer;
        //    System.Timers.Timer menuTrimTimer;
        //    static bool MenuHasBeenAdded = false;
        //    bool AppendToExistingMenu = false;
        //    public Loader() { }

        //    internal void CreateMainMenuItem()
        //    {
        //        menuLoadTimer = new System.Timers.Timer(500);
        //        menuLoadTimer.Elapsed += TryAddMenuItem;
        //        menuTrimTimer = new System.Timers.Timer(500);
        //        menuTrimTimer.Elapsed += TrimMenuItem;

        //        AppDomain currentDomain = AppDomain.CurrentDomain;
        //        Assembly[] assemblies = currentDomain.GetAssemblies();
        //        foreach (Assembly ass in currentDomain.GetAssemblies())
        //        {
        //            if (ass.FullName.StartsWith("GsaGH"))
        //                AppendToExistingMenu = true;
        //        }

        //        if (!AppendToExistingMenu)
        //            menuLoadTimer.Start();
        //        else
        //            menuTrimTimer.Start();
        //    }

        //    private void TryAddMenuItem(object sender, ElapsedEventArgs e)
        //    {
        //        if (Grasshopper.Instances.DocumentEditor == null) return;
        //        if (Grasshopper.Instances.ActiveCanvas == null) return;
        //        if (Grasshopper.Instances.ComponentServer.
        //            FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9")) == null) return;

        //        if (MenuHasBeenAdded)
        //        {
        //            menuLoadTimer.Stop();
        //            menuTrimTimer.Start();
        //            return;
        //        }

        //        if (!AppendToExistingMenu)
        //        {
        //            ToolStripMenuItem oasysMenu = AddMenuItem(new ToolStripMenuItem("Oasys"), sender, e);

        //            // get main menu
        //            var mainMenu = Grasshopper.Instances.DocumentEditor.MainMenuStrip;

        //            try
        //            {
        //                mainMenu.Items.Insert(mainMenu.Items.Count - 2, oasysMenu);
        //                MenuHasBeenAdded = true;

        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }

        //        menuLoadTimer.Stop();
        //        menuTrimTimer.Start();
        //    }

        //    private ToolStripMenuItem AddMenuItem(ToolStripMenuItem oasysMenu, object sender, ElapsedEventArgs e)
        //    {
        //        // add units
        //        oasysMenu.DropDown.Items.Add("AdSec Units", Properties.Resources.Units, (s, a) =>
        //        {
        //            UI.UnitSettingsBox unitBox = new UI.UnitSettingsBox();
        //            unitBox.Show();
        //        });
        //        // add info
        //        oasysMenu.DropDown.Items.Add("AdSec Info", Properties.Resources.AdSecInfo, (s, a) =>
        //        {
        //            UI.AboutAdSecBox aboutBox = new UI.AboutAdSecBox();
        //            aboutBox.Show();
        //        });

        //        return oasysMenu;
        //    }

        //    private void TrimMenuItem(object sender, ElapsedEventArgs e)
        //    {
        //        if (Grasshopper.Instances.DocumentEditor == null) return;
        //        if (Grasshopper.Instances.ActiveCanvas == null) return;

        //        var mainMenu = Grasshopper.Instances.DocumentEditor.MainMenuStrip;
        //        if (AppendToExistingMenu)
        //        {
        //            // return if menu has not yet been created
        //            if (mainMenu == null || mainMenu.Items.Count == 0)
        //                return;
        //            // return if trim loop has not yet been run by GSA plugin
        //            int n = 0;
        //            for (int i = 0; i < mainMenu.Items.Count; i++)
        //            {
        //                if (mainMenu.Items[i].ToString() == "Oasys")
        //                    n++;
        //            }
        //            if (n > 1) // if more than one Oasys menu exist we let GSA plugin handle removing that
        //                return;
        //            else if (n == 1) // if there is just one Oasys menu we append to that one
        //            {
        //                for (int i = 0; i < mainMenu.Items.Count; i++)
        //                {
        //                    if (mainMenu.Items[i].ToString() == "Oasys")
        //                    {
        //                        ToolStripMenuItem oasysMenu = mainMenu.Items[i] as ToolStripMenuItem;
        //                        if (oasysMenu.DropDown.Items.Count == 2)
        //                        {
        //                            // add separator first
        //                            oasysMenu.DropDown.Items.Add(GH_DocumentObject.Menu_AppendSeparator(oasysMenu.DropDown));
        //                            // append AdSec menu items
        //                            AddMenuItem(oasysMenu, sender, e);
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //                return;
        //        }
        //        else
        //        {
        //            bool removeNext = false;
        //            for (int i = 0; i < mainMenu.Items.Count; i++)
        //            {
        //                if (mainMenu.Items[i].ToString() == "Oasys")
        //                {
        //                    if (!removeNext)
        //                    {
        //                        removeNext = true;
        //                    }
        //                    else
        //                    {
        //                        mainMenu.Items.RemoveAt(i);
        //                        i--;
        //                    }
        //                }
        //            }
        //        }

        //        menuTrimTimer.Stop();

        //        if (AppendToExistingMenu)
        //        {
        //            for (int i = 0; i < mainMenu.Items.Count; i++)
        //            {
        //                if (mainMenu.Items[i].ToString() == "Oasys")
        //                {
        //                    ToolStripMenuItem oasysMenu = mainMenu.Items[i] as ToolStripMenuItem;
        //                    while (oasysMenu.DropDown.Items.Count > 5)
        //                        oasysMenu.DropDown.Items.RemoveAt(5);
        //                }
        //            }
        //        }
        //    }
    }
}