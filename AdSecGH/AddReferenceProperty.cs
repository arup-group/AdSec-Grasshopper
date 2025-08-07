using System;
using System.IO;
using System.Reflection;
using System.Text;

using AdSecCore.Constants;

using AdSecGH.Graphics.Menu;
using AdSecGH.Properties;

using Grasshopper;
using Grasshopper.Kernel;

using OasysGH.Helpers;

using Rhino;

using Utility = OasysGH.Utility;

namespace AdSecGH {
  public class AddReferencePriority : GH_AssemblyPriority {
#pragma warning disable S2223 // Non-constant static fields should not be visible
#pragma warning disable S1104 // Fields should not have public accessibility
    public static Assembly AdSecAPI;
#pragma warning restore S1104 // Fields should not have public accessibility
#pragma warning restore S2223 // Non-constant static fields should not be visible
    private static string _pluginPath;

    public static string PluginPath {
      get {
        if (_pluginPath != null) {
          return _pluginPath;
        }

        _pluginPath = TryFindPluginPath("AdSec.gha");
        return _pluginPath;
      }
    }

    /// <summary>
    ///   This method finds the location of the AdSec plugin and add's the path to the system environment to load referenced
    ///   dll files.
    ///   Method also tries to load the adsec_api.dll file and provides grasshopper loading error messages if it fails.
    ///   If loading of the dll files fails then the method will abort loading the adsec plugin.
    /// </summary>
    /// <returns></returns>
    public override GH_LoadingInstruction PriorityLoad() {
      if (TryFindPluginPath("AdSec.gha") == "") {
        return GH_LoadingInstruction.Abort;
      }

      // ### Set system environment variables to allow user rights to read above dll ###
      const string name = "PATH";
      string pathvar = Environment.GetEnvironmentVariable(name);
      string value = $"{pathvar};{PluginPath}";
      var target = EnvironmentVariableTarget.Process;
      Environment.SetEnvironmentVariable(name, value, target);

      // ### Reference AdSecAPI and SQLite dlls ###
      try {
#pragma warning disable S2696 // Instance members should not write to "static" fields
#pragma warning disable S3885 // "Assembly.Load" should be used - we must use LoadFile to load the dll from the plugin folder, without that our tests will not work
        AdSecAPI = Assembly.LoadFile($"{PluginPath}\\AdSec_API.dll");
        AdSecFileHelper.LoadMode = AdSecDllLoader.LoadMode.Custom;
        AdSecFileHelper.Custom = AdSecAPI;
#pragma warning restore S3885 // "Assembly.Load" should be used
#pragma warning restore S2696 // Instance members should not write to "static" fields
      } catch (Exception ex) {
        string message
          = $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Error loading the file AdSec_API.dll from path {PluginPath} - check if the file exist.{Environment.NewLine}The plugin cannot be loaded.";
        var exception = new Exception(message);
        var gH_LoadingException = new GH_LoadingException("AdSec: AdSec_API.dll loading", exception);
        Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);
        PostHog.PluginLoaded(PluginInfo.Instance, message);
        return GH_LoadingInstruction.Abort;
      }

      // ### Queue up Main menu loader ###
      Instances.CanvasCreated += MenuLoad.OnStartup;

      // ### Create Ribbon Category name and icon ###
      Instances.ComponentServer.AddCategorySymbolName("AdSec", 'A');
      Instances.ComponentServer.AddCategoryIcon("AdSec", Resources.AdSecLogo);

      // ### Setup OasysGH and shared Units ###
      Utility.InitialiseMainMenuUnitsAndDependentPluginsCheck();

      PostHog.PluginLoaded(PluginInfo.Instance);

      return GH_LoadingInstruction.Proceed;
    }

    private static string TryFindPluginPath(string keyword) {
      // ### Search for plugin path ###

      // initially look in %appdata% folder where package manager will store the plugin
      string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      path = Path.Combine(path, "McNeel", "Rhinoceros", "Packages", $"{RhinoApp.ExeVersion}.0",
        AdSecGHInfo.ProductName);

      if (File.Exists(Path.Combine(path, keyword))) // if no plugin file is found there continue search
      {
        return Path.GetDirectoryName(path);
      }

      // search grasshopper libraries folder
      string sDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Grasshopper",
        "Libraries");

      string[] files = Directory.GetFiles(sDir, keyword, SearchOption.AllDirectories);
      if (files.Length > 0) {
        path = files[0].Replace(keyword, string.Empty);
      }

      if (File.Exists(Path.Combine(path, keyword))) // if no plugin file is found there continue search
      {
        return Path.GetDirectoryName(path);
      }

      // look in all the other Grasshopper assembly (plugin) folders
      foreach (var pluginFolder in Folders.AssemblyFolders) {
        files = Directory.GetFiles(pluginFolder.Folder, keyword, SearchOption.AllDirectories);
        if (files.Length == 0) {
          continue;
        }

        path = files[0].Replace(keyword, string.Empty);
        return Path.GetDirectoryName(path);
      }

      string message
        = $"Error loading the file {keyword} from any Grasshopper plugin folders - check if the file exist.{Environment.NewLine}The plugin cannot be loaded.{Environment.NewLine}Folders (including subfolder) that was searched:{Environment.NewLine}{sDir}";
      var stringBuilder = new StringBuilder(message);
      foreach (var pluginFolder in Folders.AssemblyFolders) {
        stringBuilder.AppendLine(pluginFolder.Folder);
      }

      message = stringBuilder.ToString();
      var exception = new Exception(message);
      var gH_LoadingException
        = new GH_LoadingException($"{AdSecGHInfo.ProductName}: {keyword} loading failed", exception);
      Instances.ComponentServer.LoadingExceptions.Add(gH_LoadingException);
      PostHog.PluginLoaded(PluginInfo.Instance, message);
      return "";
    }
  }
}
