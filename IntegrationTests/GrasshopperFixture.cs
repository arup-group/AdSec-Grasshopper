﻿using System;
using System.IO;
using System.Linq;

using Oasys.Geometry.Paths2D;

using OasysGH.Units;

using Xunit;

namespace IntegrationTests {
  public class GrasshopperFixture : IDisposable {
    public Rhino.Runtime.InProcess.RhinoCore Core {
      get {
        if (null == _Core) {
          InitializeCore();
        }
        return _Core as Rhino.Runtime.InProcess.RhinoCore;
      }
    }
    public Grasshopper.Kernel.GH_DocumentIO DocIO {
      get {
        if (null == _docIo) {
          InitializeDocIO();
        }
        return _docIo as Grasshopper.Kernel.GH_DocumentIO;
      }
    }
    public Grasshopper.Plugin.GH_RhinoScriptInterface GHPlugin {
      get {
        if (null == _GHPlugin) {
          InitializeGrasshopperPlugin();
        }
        return _GHPlugin as Grasshopper.Plugin.GH_RhinoScriptInterface;
      }
    }
    private object _docIo = null;
    private static string _linkFileName = "IntegrationTests.ghlink";
    private static string _linkFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Grasshopper", "Libraries");
    private object _Core = null;
    private object _GHPlugin = null;
    private bool _isDisposed;

    static GrasshopperFixture() {
      // This MUST be included in a static constructor to ensure that no Rhino DLLs
      // are loaded before the resolver is set up. Avoid creating other static functions
      // and members which may reference Rhino assemblies, as that may cause those
      // assemblies to be loaded before this is called.
      RhinoInside.Resolver.Initialize();
    }

    public GrasshopperFixture() {
      AddPluginToGH();

      InitializeCore();

      // setup headless units
      Utility.SetupUnitsDuringLoad();
    }

    public void AddPluginToGH() {
      Directory.CreateDirectory(_linkFilePath);
      StreamWriter writer = File.CreateText(Path.Combine(_linkFilePath, _linkFileName));
      writer.WriteLine(Environment.CurrentDirectory);
      string gsaGhPath = Path.Combine(FindSolutionRoot(Environment.CurrentDirectory), "GSA-GH");
      writer.WriteLine(gsaGhPath);
      writer.Close();
    }

    public static string FindSolutionRoot(string startPath) {
      var dir = new DirectoryInfo(startPath);

      while (dir != null && !dir.GetFiles("*.sln").Any()) {
        dir = dir.Parent;
      }

      return dir?.FullName ?? throw new Exception("Solution root not found.");
    }

    public void Dispose() {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
      File.Delete(Path.Combine(_linkFilePath, _linkFileName));
    }

    protected virtual void Dispose(bool disposing) {
      if (_isDisposed) {
        return;
      }
      if (disposing) {
        _docIo = null;
        GHPlugin.CloseAllDocuments();
        _GHPlugin = null;
        Core.Dispose();
      }

      // TODO: free unmanaged resources (unmanaged objects) and override finalizer
      // TODO: set large fields to null
      _isDisposed = true;
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~GrasshopperFixture()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }
    private void InitializeCore() {
      _Core = new Rhino.Runtime.InProcess.RhinoCore();
    }

    private void InitializeDocIO() {
      // we do this in a seperate function to absolutely ensure that the core is initialized before we load the GH plugin,
      // which will happen automatically when we enter the function containing GH references
      if (null == _GHPlugin) {
        InitializeGrasshopperPlugin();
      }
      InitializeDocIO2();
    }

    private void InitializeDocIO2() {
      var docIO = new Grasshopper.Kernel.GH_DocumentIO();
      _docIo = docIO;
    }

    private void InitializeGrasshopperPlugin() {
      if (null == _Core) {
        InitializeCore();
      }
      // we do this in a seperate function to absolutely ensure that the core is initialized before we load the GH plugin,
      // which will happen automatically when we enter the function containing GH references
      InitializeGrasshopperPlugin2();
    }

    private void InitializeGrasshopperPlugin2() {
      _GHPlugin = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
      var ghp = _GHPlugin as Grasshopper.Plugin.GH_RhinoScriptInterface;
      ghp.RunHeadless();
    }
  }

  [CollectionDefinition("GrasshopperFixture collection")]
  public class GrasshopperCollection : ICollectionFixture<GrasshopperFixture> {
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
  }
}
