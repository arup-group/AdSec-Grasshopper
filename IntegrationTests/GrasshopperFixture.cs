using System;
using System.IO;
using Xunit;

namespace ComposGHTests {
  public class GrasshopperFixture : IDisposable {
    public Rhino.Runtime.InProcess.RhinoCore Core {
      get {
        if (null == this._Core)
          InitializeCore();
        return this._Core as Rhino.Runtime.InProcess.RhinoCore;
      }
    }
    public Grasshopper.Kernel.GH_DocumentIO DocIO {
      get {
        if (null == this._DocIO)
          this.InitializeDocIO();
        return this._DocIO as Grasshopper.Kernel.GH_DocumentIO;
      }
    }
    public Grasshopper.Plugin.GH_RhinoScriptInterface GHPlugin {
      get {
        if (null == this._GHPlugin)
          InitializeGrasshopperPlugin();
        return this._GHPlugin as Grasshopper.Plugin.GH_RhinoScriptInterface;
      }
    }
    private object _Doc { get; set; }
    private object _DocIO { get; set; }
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
      this.AddPluginToGH();

      this.InitializeCore();

      // setup headless units
      OasysGH.Units.Utility.SetupUnitsDuringLoad(true);
    }

    public void AddPluginToGH() {
      Directory.CreateDirectory(_linkFilePath);
      StreamWriter writer = File.CreateText(Path.Combine(_linkFilePath, _linkFileName));
      writer.Write(Environment.CurrentDirectory);
      writer.Close();
    }

    public void Dispose() {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      this.Dispose(disposing: true);
      GC.SuppressFinalize(this);
      File.Delete(Path.Combine(_linkFilePath, _linkFileName));
    }

    protected virtual void Dispose(bool disposing) {
      if (_isDisposed) return;
      if (disposing) {
        this._Doc = null;
        this._DocIO = null;
        GHPlugin.CloseAllDocuments();
        this._GHPlugin = null;
        this.Core.Dispose();
      }

      // TODO: free unmanaged resources (unmanaged objects) and override finalizer
      // TODO: set large fields to null
      this._isDisposed = true;
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~GrasshopperFixture()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }
    private void InitializeCore() {
      this._Core = new Rhino.Runtime.InProcess.RhinoCore();
    }

    private void InitializeDocIO() {
      // we do this in a seperate function to absolutely ensure that the core is initialized before we load the GH plugin,
      // which will happen automatically when we enter the function containing GH references
      if (null == this._GHPlugin) InitializeGrasshopperPlugin();
      InitializeDocIO2();
    }

    private void InitializeDocIO2() {
      var docIO = new Grasshopper.Kernel.GH_DocumentIO();
      this._DocIO = docIO;
    }

    private void InitializeGrasshopperPlugin() {
      if (null == this._Core)
        this.InitializeCore();
      // we do this in a seperate function to absolutely ensure that the core is initialized before we load the GH plugin,
      // which will happen automatically when we enter the function containing GH references
      InitializeGrasshopperPlugin2();
    }

    private void InitializeGrasshopperPlugin2() {
      this._GHPlugin = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
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
