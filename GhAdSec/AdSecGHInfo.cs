using Grasshopper.Kernel;
using System;
using System.IO;
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
            // ### Create Ribbon Category name and icon ###
            Grasshopper.Instances.ComponentServer.AddCategorySymbolName("AdSec", 'A');
            Grasshopper.Instances.ComponentServer.AddCategoryIcon("AdSec", AdSecGH.Properties.Resources.AdSecLogo);

            // create main menu dropdown
            AdSecGH.Helpers.Loader menuLoad = new Helpers.Loader();
            menuLoad.LoadingAdSecMenuAndReferences();

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
