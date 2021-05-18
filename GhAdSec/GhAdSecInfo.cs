using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Reflection;

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
            // ### Reference GSA API and SQLite dlls ###
            // set folder to latest GSA version.
            //Assembly ass1 = Assembly.LoadFile(@"C:\Users\Kristjan.Nielsen\source\repos\GhAdSec\GhAdSec\bin\x64\Debug\AdSec_API.dll");
            //Assembly ass2 = Assembly.LoadFile(Util.Gsa.InstallationFolderPath.GetPath + "\\System.Data.SQLite.dll");
            //Assembly ass3 = Assembly.LoadFile(Util.Gsa.InstallationFolderPath.GetPath + "\\libiomp5md.dll");

            // ### Set system environment variables to allow user rights to read above dll ###
            //const string name = "PATH";
            //string pathvar = System.Environment.GetEnvironmentVariable(name);
            //var value = pathvar + ";" + Util.Gsa.InstallationFolderPath.GetPath + "\\";
            //var target = EnvironmentVariableTarget.Process;
            //System.Environment.SetEnvironmentVariable(name, value, target);

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
