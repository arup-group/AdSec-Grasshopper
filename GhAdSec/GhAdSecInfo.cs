using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace GhAdSec
{
    public class AddReferencePriority : GH_AssemblyPriority
    {
        /// <summary>
        /// This method finds the user's GSA installation folder and loads GsaAPI.dll so that this plugin does not need to ship with an additional dll file
        /// 
        /// Method also provides access rights to Grasshopper to read dynamically linked dll files in the GSA installation folder.
        /// </summary>
        /// <returns></returns>
        public override GH_LoadingInstruction PriorityLoad()
        {
            // ## Get plugin assembly file location
            string pluginPath = Assembly.GetExecutingAssembly().Location; // full path+name
            pluginPath = pluginPath.Replace("GhAdSec.gha", "");
            
            // ### Set system environment variables to allow user rights to read above dll ###
            const string name = "PATH";
            string pathvar = System.Environment.GetEnvironmentVariable(name);
            var value = pathvar + ";" + pluginPath;
            var target = EnvironmentVariableTarget.Process;
            System.Environment.SetEnvironmentVariable(name, value, target);

            // ### Reference AdSec API dlls from .gha assembly path ###
            Assembly ass1 = Assembly.LoadFile(pluginPath + "\\AdSec_API.dll");

            // ### Create Ribbon Category name and icon ###
            Grasshopper.Instances.ComponentServer.AddCategorySymbolName("AdSec", 'A');
            //Grasshopper.Instances.ComponentServer.AddCategoryIcon("GSA", GhSA.Properties.Resources.GsaGhLogo);

            return GH_LoadingInstruction.Proceed;
        }
    }
   
    public class GhAdSecInfo : GH_AssemblyInfo
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
                return "AdSec Plugin";
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
    }
}
