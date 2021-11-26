using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

namespace AdSecGH
{
    public class AddReferencePriority : GH_AssemblyPriority
    {
        /// <summary>
        /// This method finds the location of the AdSec plugin and add's the path to the system environment to load referenced dll files.
        /// Method also tries to load the adsec_api.dll file and provides grasshopper loading error messages if it fails.
        /// If loading of the dll files fails then the method will abort loading the adsec plugin.
        /// </summary>
        /// <returns></returns>
        public override GH_LoadingInstruction PriorityLoad()
        {
            // ## Get plugin assembly file location
            PluginPath = Assembly.GetExecutingAssembly().Location; // full path+name
            PluginPath = PluginPath.Replace("AdSec.gha", "");
            
            // ### Set system environment variables to allow user rights to read above dll ###
            const string name = "PATH";
            string pathvar = System.Environment.GetEnvironmentVariable(name);
            var value = pathvar + ";" + PluginPath;
            var target = EnvironmentVariableTarget.Process;
            System.Environment.SetEnvironmentVariable(name, value, target);

            // ### Try Reference AdSecAPI and SQLite dlls ###
            // set folder to latest GSA version.
            try
            {
                // ### Reference AdSec API dlls from .gha assembly path ###
                AdSecAPI = Assembly.LoadFile(PluginPath + "\\AdSec_API.dll");
            }
            catch (Exception e)
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
                string message = e.Message
                    + System.Environment.NewLine + System.Environment.NewLine +
                    "This may be due to clash with other referenced dll files by one of these plugins that's already been loaded: "
                    + System.Environment.NewLine + loadedPlugins
                    + System.Environment.NewLine + "You may try disable the above plugins to solve the issue."
                    + System.Environment.NewLine + "The plugin cannot be loaded.";
                Exception exception = new Exception(message);
                Grasshopper.Kernel.GH_LoadingException gH_LoadingException = new GH_LoadingException("AdSec: AdSec_API.dll loading", exception);
                Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);
                return GH_LoadingInstruction.Abort;
            }

            // try create solution for license check
            try
            {
                IAdSec ad = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
            }
            catch (Exception e)
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

                string message = e.Message + msg;
                Exception exception = new Exception(message);
                Grasshopper.Kernel.GH_LoadingException gH_LoadingException = new GH_LoadingException("AdSec", exception);
                Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);

                return GH_LoadingInstruction.Abort;
            }

            try
            {
                // ### Reference System.ValueTuple.dll from .gha assembly path ###
                Assembly assTuple = Assembly.LoadFile(PluginPath + "\\System.ValueTuple.dll");
            }
            catch (Exception)
            {

                throw;
            }

            // ### Create Ribbon Category name and icon ###
            Grasshopper.Instances.ComponentServer.AddCategorySymbolName("AdSec", 'A');
            Grasshopper.Instances.ComponentServer.AddCategoryIcon("AdSec", AdSecGH.Properties.Resources.AdSecLogo);

            // create main menu dropdown
            AdSecGH.Helpers.Loader menuLoad = new Helpers.Loader();
            menuLoad.CreateMainMenuItem();

            // Setup units
            Units.SetupUnits();

            return GH_LoadingInstruction.Proceed;
        }
        public static Assembly AdSecAPI;
        public static string PluginPath;
    }
   
    public class AdSecGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "AdSec";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public string icon_url
        {
            get
            {
                return "https://raw.githubusercontent.com/arup-group/GSA-Grasshopper/master/Documentation/GettingStartedGuide/Icons/GsaGhLogo.jpg";
            }
        }
        public override Bitmap AssemblyIcon
        {
            get
            {
                return Icon;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Official Oasys AdSec Grasshopper Plugin" + System.Environment.NewLine
                + System.Environment.NewLine + "The plugin requires an AdSec 10 license to load."
                + System.Environment.NewLine
                + System.Environment.NewLine + "Contact oasys@arup.com to request a free trial version."
                + System.Environment.NewLine + System.Environment.NewLine + "Copyright © Oasys 1985 - 2021";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Oasys / Kristjan Nielsen";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "https://www.oasys-software.com/";
            }
        }
        public override string Version
        {
            get
            {
                return "0.0.7-beta";
            }
        }
    }
}
