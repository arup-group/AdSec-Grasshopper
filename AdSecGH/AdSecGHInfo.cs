using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Grasshopper.Kernel;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using OasysGH;
using OasysGH.Helpers;

namespace AdSecGH {
  public class AddReferencePriority : GH_AssemblyPriority {
    public static string PluginPath {
      get {
        if (_pluginPath == null) {
          _pluginPath = TryFindPluginPath("AdSec.gha");
        }
        return _pluginPath;
      }
    }
    public static Assembly AdSecAPI;
    private static string _pluginPath;

    /// <summary>
    /// This method finds the location of the AdSec plugin and add's the path to the system environment to load referenced dll files.
    /// Method also tries to load the adsec_api.dll file and provides grasshopper loading error messages if it fails.
    /// If loading of the dll files fails then the method will abort loading the adsec plugin.
    /// </summary>
    /// <returns></returns>
    public override GH_LoadingInstruction PriorityLoad() {
      if (TryFindPluginPath("AdSec.gha") == "") {
        return GH_LoadingInstruction.Abort;
      }

      // ### Set system environment variables to allow user rights to read above dll ###
      const string name = "PATH";
      string pathvar = Environment.GetEnvironmentVariable(name);
      string value = pathvar + ";" + PluginPath;
      EnvironmentVariableTarget target = EnvironmentVariableTarget.Process;
      Environment.SetEnvironmentVariable(name, value, target);

      // ### Reference AdSecAPI and SQLite dlls ###
      try {
        AdSecAPI = Assembly.LoadFile(PluginPath + "\\AdSec_API.dll");
        //Assembly assTuple = Assembly.LoadFile(PluginPath + "\\System.ValueTuple.dll");
        //Assembly assSQL = Assembly.LoadFile(PluginPath + "\\System.Data.SQLite.dll");
      } catch (Exception ex) {
        string message = ex.Message
            + Environment.NewLine + Environment.NewLine +
            "Error loading the file AdSec_API.dll from path " + PluginPath + " - check if the file exist."
            + Environment.NewLine + "The plugin cannot be loaded.";
        var exception = new Exception(message);
        var gH_LoadingException = new GH_LoadingException("AdSec: AdSec_API.dll loading", exception);
        Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);
        PostHog.PluginLoaded(PluginInfo.Instance, message);
        return GH_LoadingInstruction.Abort;
      }

      // ### Trigger a license check ###
      try {
        var ad = IAdSec.Create(EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014);
      } catch (Exception ex) {
        string message = ex.Message;
        var exception = new Exception(message);
        var gH_LoadingException = new GH_LoadingException("AdSec: License", exception);
        Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);
        PostHog.PluginLoaded(PluginInfo.Instance, message);
        return GH_LoadingInstruction.Abort;
      }

      // ### Queue up Main menu loader ###
      Grasshopper.Instances.CanvasCreated += Graphics.Menu.MenuLoad.OnStartup;

      // ### Create Ribbon Category name and icon ###
      Grasshopper.Instances.ComponentServer.AddCategorySymbolName("AdSec", 'A');
      Grasshopper.Instances.ComponentServer.AddCategoryIcon("AdSec", Properties.Resources.AdSecLogo);

      // ### Setup OasysGH and shared Units ###
      Utility.InitialiseMainMenuAndDefaultUnits();

      PostHog.PluginLoaded(AdSecGH.PluginInfo.Instance);

      return GH_LoadingInstruction.Proceed;
    }

    private static string TryFindPluginPath(string keyword) {
      // ### Search for plugin path ###

      // initially look in %appdata% folder where package manager will store the plugin
      string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      path = Path.Combine(path, "McNeel", "Rhinoceros", "Packages", Rhino.RhinoApp.ExeVersion + ".0", AdSecGHInfo.ProductName);

      if (!File.Exists(Path.Combine(path, keyword))) // if no plugin file is found there continue search
      {
        // search grasshopper libraries folder
        string sDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Grasshopper",
          "Libraries");

        string[] files = Directory.GetFiles(sDir, keyword, SearchOption.AllDirectories);
        if (files.Length > 0) {
          path = files[0].Replace(keyword, string.Empty);
        }

        if (!File.Exists(Path.Combine(path, keyword))) // if no plugin file is found there continue search
        {
          // look in all the other Grasshopper assembly (plugin) folders
          foreach (GH_AssemblyFolderInfo pluginFolder in Grasshopper.Folders.AssemblyFolders) {
            files = Directory.GetFiles(pluginFolder.Folder, keyword, SearchOption.AllDirectories);
            if (files.Length > 0) {
              path = files[0].Replace(keyword, string.Empty);
              return Path.GetDirectoryName(path);
            }
          }
          string message =
            "Error loading the file " + keyword + " from any Grasshopper plugin folders - check if the file exist."
            + Environment.NewLine + "The plugin cannot be loaded."
            + Environment.NewLine + "Folders (including subfolder) that was searched:"
            + Environment.NewLine + sDir;
          foreach (GH_AssemblyFolderInfo pluginFolder in Grasshopper.Folders.AssemblyFolders) {
            message += Environment.NewLine + pluginFolder.Folder;
          }

          var exception = new Exception(message);
          var gH_LoadingException = new GH_LoadingException(AdSecGHInfo.ProductName + ": " + keyword + " loading failed", exception);
          Grasshopper.Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);
          PostHog.PluginLoaded(AdSecGH.PluginInfo.Instance, message);
          return "";
        }
      }
      return Path.GetDirectoryName(path);
    }
  }

  public class AdSecGHInfo : GH_AssemblyInfo {
    public override Bitmap AssemblyIcon => Icon;
    public override string AuthorContact =>
        //Return a string representing your preferred contact details.
        Contact;
    public override string AuthorName =>
        //Return a string identifying you or your company.
        Company;
    public override string Description =>
        //Return a short string describing the purpose of this GHA library.
        "Official Oasys AdSec Grasshopper Plugin" + Environment.NewLine
          + (isBeta ? Disclaimer : "")
        + Environment.NewLine + "The plugin requires an AdSec 10 license to load. "
        + Environment.NewLine + "Contact oasys@arup.com to request a free trial version. "
        + System.Environment.NewLine + TermsConditions
        + System.Environment.NewLine + Copyright;
    public override Bitmap Icon =>
        //Return a 24x24 pixel bitmap to represent this GHA library.
        null;
    public string IconUrl => "https://raw.githubusercontent.com/arup-group/GSA-Grasshopper/main/Documentation/GettingStartedGuide/Icons/GsaGhLogo.jpg";
    public override Guid Id => GUID;
    public override string Name => ProductName;
    public override string Version {
      get {
        if (isBeta) {
          return Vers + "-beta";
        } else {
          return Vers;
        }
      }
    }
    internal const string Company = "Oasys";
    internal const string Contact = "https://www.oasys-software.com/";
    internal const string Copyright = "Copyright © Oasys 1985 - 2022";
    internal const string PluginName = "AdSecGH";
    internal const string ProductName = "AdSec";
    internal const string TermsConditions = "Oasys terms and conditions apply. See https://www.oasys-software.com/terms-conditions for details. ";
    internal const string Vers = "0.9.17";
    internal static string Disclaimer = PluginName + " is pre-release and under active development, including further testing to be undertaken. It is provided \"as-is\" and you bear the risk of using it. Future versions may contain breaking changes. Any files, results, or other types of output information created using " + PluginName + " should not be relied upon without thorough and independent checking. ";
    internal static Guid GUID = new Guid("f815c29a-e1eb-4ca6-9e56-0554777ff9c9");
    internal static bool isBeta = true;
  }

  internal sealed class PluginInfo {
    public static OasysPluginInfo Instance => lazy.Value;
    private static readonly Lazy<OasysPluginInfo> lazy =
            new Lazy<OasysPluginInfo>(() => new OasysPluginInfo(
      AdSecGHInfo.ProductName,
      AdSecGHInfo.PluginName,
      AdSecGHInfo.Vers,
      AdSecGHInfo.isBeta,
      "phc_alOp3OccDM3D18xJTWDoW44Y1cJvbEScm5LJSX8qnhs"
      ));

    private PluginInfo() {
    }
  }
}
