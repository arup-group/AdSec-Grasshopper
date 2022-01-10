using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

namespace AdSecGH.Helpers
{
    public class Loader
    {
        System.Timers.Timer menuLoadTimer;
        System.Timers.Timer menuTrimTimer;
        static bool MenuHasBeenAdded = false;
        bool AppendToExistingMenu = false;
        public Loader() { }

        internal void LoadingAdSecMenuAndReferences()
        {
            menuLoadTimer = new System.Timers.Timer(500);
            menuLoadTimer.Start();
            menuLoadTimer.Elapsed += TryAddMenuItem;
            menuTrimTimer = new System.Timers.Timer(500);
            menuTrimTimer.Elapsed += TrimMenuItem;
        }

        private void TryAddMenuItem(object sender, ElapsedEventArgs e)
        {
            if (Grasshopper.Instances.DocumentEditor == null) return;

            if (MenuHasBeenAdded)
            {
                menuLoadTimer.Stop();
                menuTrimTimer.Start();
                return;
            }

            // check if GSA plugin is installed, then we want to append to existing menu
            GH_AssemblyInfo gsaPlugin = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("a3b08c32-f7de-4b00-b415-f8b466f05e9f"));
            if (gsaPlugin != null && ((int)gsaPlugin.Version[0] < 1 & (int)gsaPlugin.Version[2] < 4))
            {
                AppendToExistingMenu = true;
                menuLoadTimer.Stop();
                menuTrimTimer.Start();
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
                    menuLoadTimer.Stop();
                    menuTrimTimer.Start();
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
            
            menuTrimTimer.Stop();

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

            LoadDllReferences();
        }
        

        private void LoadDllReferences()
        {
            GH_AssemblyInfo adsecPlugin = Grasshopper.Instances.ComponentServer.FindAssembly(new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9"));
            string codeBase = adsecPlugin.Location;

            // ## Get plugin assembly file location
            //string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            AddReferencePriority.PluginPath = Path.GetDirectoryName(path);

            AddReferencePriority.PluginPath = AddReferencePriority.PluginPath.Replace("AdSec.gha", "");

            // ### Set system environment variables to allow user rights to read above dll ###
            const string name = "PATH";
            string pathvar = System.Environment.GetEnvironmentVariable(name);
            var value = pathvar + ";" + AddReferencePriority.PluginPath;
            var target = EnvironmentVariableTarget.Process;
            System.Environment.SetEnvironmentVariable(name, value, target);

            // ### Try Reference AdSecAPI and SQLite dlls ###
            try
            {
                // ### Reference AdSec API dlls from .gha assembly path ###
                AddReferencePriority.AdSecAPI = Assembly.LoadFile(AddReferencePriority.PluginPath + "\\AdSec_API.dll");
            }
            catch (Exception ex)
            {
                // check other plugins?
                string loadedPlugins = "";
                ReadOnlyCollection<GH_AssemblyInfo> plugins = Grasshopper.Instances.ComponentServer.Libraries;
                foreach (GH_AssemblyInfo plugin in plugins)
                {
                    if (!plugin.IsCoreLibrary)
                    {
                        if (!plugin.Name.StartsWith("Kangaroo"))
                        {
                            loadedPlugins = loadedPlugins + "-" + plugin.Name + System.Environment.NewLine;
                        }
                    }
                }
                string message = ex.Message
                    + System.Environment.NewLine + System.Environment.NewLine +
                    "Error loading the file AdSec_API.dll from path " + AddReferencePriority.PluginPath + " - check if the file exist."
                    + System.Environment.NewLine + System.Environment.NewLine +
                    "This may be due to clash with other referenced dll files by one of these plugins that's already been loaded: "
                    + System.Environment.NewLine + loadedPlugins
                    + System.Environment.NewLine + "You may try disable the above plugins to solve the issue."
                    + System.Environment.NewLine + "The plugin cannot be loaded.";
                Exception exception = new Exception(message);
                Grasshopper.Kernel.GH_LoadingException gH_LoadingException = new GH_LoadingException("AdSec: AdSec_API.dll loading", exception);
                Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);
                //return GH_LoadingInstruction.Abort;
            }

            // try create solution for license check
            try
            {
                IAdSec ad = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
            }
            catch (Exception ex)
            {
                ReadOnlyCollection<GH_AssemblyInfo> plugins = Grasshopper.Instances.ComponentServer.Libraries;
                Guid gsa_guid = new Guid("a3b08c32-f7de-4b00-b415-f8b466f05e9f");
                string msg = "";
                foreach (GH_AssemblyInfo assemblyInfo in plugins)
                {
                    if (assemblyInfo.Id == gsa_guid)
                    {
                        msg = System.Environment.NewLine + "Known license retrieval error; please uninstall GSA plugin and try loading AdSec plugin again";
                    }
                }

                string message = ex.Message + msg;
                Exception exception = new Exception(message);
                Grasshopper.Kernel.GH_LoadingException gH_LoadingException = new GH_LoadingException("AdSec", exception);
                Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);

                //return GH_LoadingInstruction.Abort;
            }

            try
            {
                // ### Reference System.ValueTuple.dll from .gha assembly path ###
                Assembly assTuple = Assembly.LoadFile(AddReferencePriority.PluginPath + "\\System.ValueTuple.dll");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}