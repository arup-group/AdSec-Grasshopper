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
using AdSecGH.Helpers;

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
      // ### Search for plugin path ###

      // initially look in %appdata% folder where package manager will store the plugin
      string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      path = Path.Combine(path, "McNeel", "Rhinoceros", "Packages", Rhino.RhinoApp.ExeVersion + ".0", "AdSec");

      if (!File.Exists(Path.Combine(path, "AdSec.gha"))) // if no plugin file is found there continue search
      {
        // look in all the other Grasshopper assembly (plugin) folders
        foreach (GH_AssemblyFolderInfo pluginFolder in Grasshopper.Folders.AssemblyFolders)
        {
          if (File.Exists(Path.Combine(pluginFolder.Folder, "AdSec.gha"))) // if the folder contains the plugin
          {
            path = pluginFolder.Folder;
            break;
          }
        }
      }
      PluginPath = Path.GetDirectoryName(path);

      // ### Set system environment variables to allow user rights to read above dll ###
      const string name = "PATH";
      string pathvar = Environment.GetEnvironmentVariable(name);
      var value = pathvar + ";" + PluginPath;
      var target = EnvironmentVariableTarget.Process;
      Environment.SetEnvironmentVariable(name, value, target);


      // ### Reference AdSecAPI and SQLite dlls ###
      try
      {
        AdSecAPI = Assembly.LoadFile(PluginPath + "\\AdSec_API.dll");
        //Assembly assTuple = Assembly.LoadFile(PluginPath + "\\System.ValueTuple.dll");
        //Assembly assSQL = Assembly.LoadFile(PluginPath + "\\System.Data.SQLite.dll");
      }
      catch (Exception ex)
      {

        string message = ex.Message
            + Environment.NewLine + Environment.NewLine +
            "Error loading the file AdSec_API.dll from path " + PluginPath + " - check if the file exist."
            + Environment.NewLine + "The plugin cannot be loaded.";
        Exception exception = new Exception(message);
        GH_LoadingException gH_LoadingException = new GH_LoadingException("AdSec: AdSec_API.dll loading", exception);
        Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);
        return GH_LoadingInstruction.Abort;
      }


      // ### Trigger a license check ###
      try
      {
        IAdSec ad = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
      }
      catch (Exception ex)
      {
        string message = ex.Message;
        Exception exception = new Exception(message);
        GH_LoadingException gH_LoadingException = new GH_LoadingException("AdSec: License", exception);
        Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);

        return GH_LoadingInstruction.Abort;
      }


      // ### Create Ribbon Category name and icon ###
      Grasshopper.Instances.ComponentServer.AddCategorySymbolName("AdSec", 'A');
      Grasshopper.Instances.ComponentServer.AddCategoryIcon("AdSec", Properties.Resources.AdSecLogo);


      // ### Queue up Main menu loader ###
      Helpers.Loader menuLoad = new Helpers.Loader();
      menuLoad.CreateMainMenuItem();

      // ### Setup units ###
      Units.SetupUnits();

      PostHog.PluginLoaded();

      return GH_LoadingInstruction.Proceed;
    }
    public static Assembly AdSecAPI;
    public static string PluginPath;
  }

  public class AdSecGHInfo : GH_AssemblyInfo
  {
    internal static Guid GUID = new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9");
    internal const string Company = "Oasys";
    internal const string Copyright = "Copyright © Oasys 1985 - 2022";
    internal const string Contact = "https://www.oasys-software.com/";
    internal const string Vers = "0.9.8";
    internal static bool isBeta = true;
    internal const string ProductName = "AdSec";
    internal const string PluginName = "AdSecGH";
    internal static string Disclaimer = PluginName + " is pre-release and under active development, including further testing to be undertaken. It is provided \"as-is\" and you bear the risk of using it. Future versions may contain breaking changes. Any files, results, or other types of output information created using " + PluginName + " should not be relied upon without thorough and independent checking. ";
    internal const string TermsConditions = "Oasys terms and conditions apply. See https://www.oasys-software.com/terms-conditions for details. ";
    public override string Name
    {
      get
      {
        return ProductName;
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
        return "Official Oasys AdSec Grasshopper Plugin" + Environment.NewLine
          + (isBeta ? Disclaimer : "")
        + Environment.NewLine + "The plugin requires an AdSec 10 license to load. "
        + Environment.NewLine + "Contact oasys@arup.com to request a free trial version. "
        + System.Environment.NewLine + TermsConditions
        + System.Environment.NewLine + Copyright;

      }
    }
    public override Guid Id
    {
      get
      {
        return GUID;
      }
    }

    public override string AuthorName
    {
      get
      {
        //Return a string identifying you or your company.
        return Company;
      }
    }
    public override string AuthorContact
    {
      get
      {
        //Return a string representing your preferred contact details.
        return Contact;
      }
    }
    public override string Version
    {
      get
      {
        if (isBeta)
          return Vers + "-beta";
        else
          return Vers;
      }
    }
  }
}
