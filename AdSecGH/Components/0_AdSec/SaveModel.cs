using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

using AdSecGH.Helpers;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using GH_IO.Serialization;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using OasysGH;
using OasysGH.Components;
using OasysGH.UI;

namespace AdSecGH.Components {
  public class SaveModel : GH_OasysDropDownComponent {
    private string _jsonString;
    private string _fileName;
    private bool canOpen;

    public SaveModel() : base("Save AdSec", "Save",
      "Saves your AdSec Section with loads from this parametric nightmare", CategoryName.Name(),
      SubCategoryName.Cat0()) {
      Hidden = true; // sets the initial state of the component to hidden
      Params.Input[0].ObjectChanged += (sender, args) => { ResetFields(); };
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("6bba517c-3ec1-45da-a520-ea117f7f901a");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.SaveAdSec;

    public override void CreateAttributes() {
      if (!_isInitialised) {
        InitialiseDropdowns();
      }

      m_attributes = new ThreeButtonComponentAttributes(this, "Save", "Save As", "Open AdSec", SaveFile, SaveAsFile,
        () => OpenAdSecExe(), true, "Save AdSec file");
    }

    public Process OpenAdSecExe() {
      return canOpen ? Process.Start(_fileName) : null;
    }

    public override bool Read(GH_IReader reader) {
      _fileName = reader.GetString("File");
      return base.Read(reader);
    }

    private void SaveJson() {
      try {
        File.WriteAllText(_fileName, _jsonString);
        canOpen = true;
      } catch (Exception e) {
        this.AddRuntimeError(e.Message);
        canOpen = false;
      }
    }

    public void SaveAsFile() {
      _fileName = AdSecFile.SaveFilePath();
      SaveJson();
      if (canOpen) {
        WriteFilePathToPanel();
      }
    }

    public void WriteFilePathToPanel(IGrasshopperDocumentContext context = null) {
      const int pathIndex = 3;
      RemoveSourcesFrom(pathIndex);
      AddNewPanelForInput(pathIndex, _fileName, context);
    }

    private void AddNewPanelForInput(int pathIndex, string text, IGrasshopperDocumentContext context = null) {
      var panel = CreatePanel(text);

      var canvaContext = context ?? new GrasshopperDocumentContext();
      canvaContext.AddObject(panel, false);

      Params.Input[pathIndex].AddSource(panel);
      Params.OnParametersChanged();
      ExpireSolution(true);
    }

    public GH_Panel CreatePanel(string text) {
      var panel = new GH_Panel();
      panel.CreateAttributes();

      const int offset = 40;
      var attributesBounds = Attributes.DocObject.Attributes.Bounds;
      var panelBounds = panel.Attributes.Bounds;

      panel.Attributes.Pivot = new PointF(attributesBounds.Left - panelBounds.Width - offset,
        attributesBounds.Bottom - panelBounds.Height);

      panel.UserText = text;

      return panel;
    }

    private void RemoveSourcesFrom(int index) {
      Params.Input[index].Sources?.Clear();
    }

    public void SaveFile() {
      if (string.IsNullOrEmpty(_fileName)) {
        SaveAsFile();
      } else {
        SaveJson();
      }
    }

    public override void SetSelected(int i, int j) { }

    public override bool Write(GH_IWriter writer) {
      writer.SetString("File", _fileName);
      return base.Write(writer);
    }

    protected override void InitialiseDropdowns() { _isInitialised = true; }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to save", GH_ParamAccess.list);
      pManager.AddGenericParameter("Loads", "Lds",
        "[Optional] List of AdSec Loads (consistent Load or Deformation type)", GH_ParamAccess.tree);
      pManager.AddBooleanParameter("Save?", "Save", "[Optional] Input 'True' to save or use button",
        GH_ParamAccess.item, false);
      pManager.AddTextParameter("File and Path", "File", "[Optional] Filename and path", GH_ParamAccess.item);
      pManager[1].Optional = true;
      pManager[2].Optional = true;
      pManager[3].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) { }

    protected override void SolveInternal(IGH_DataAccess DA) {
      var sections = this.GetAdSecSections(DA, 0);
      if (!sections.Any()) {
        return;
      }

      var loads = this.GetLoads(DA, 1);

      _jsonString = AdSecFile.ModelJson(sections, loads);

      // filepath
      string pathString = "";
      if (DA.GetData(3, ref pathString)) {
        if (!pathString.Equals(_fileName)) {
          canOpen = false;
        }

        _fileName = pathString;
      } else {
        canOpen = false;
        _fileName = string.Empty;
      }

      // input save bool
      bool save = false;
      if (DA.GetData(2, ref save) && save) {
        SaveJson();
      }
    }

    private void ResetFields() {
      _fileName = null;
      canOpen = false;
      _jsonString = null;
    }

    public interface IGrasshopperDocumentContext {
      void AddObject(IGH_DocumentObject obj, bool recordUndo = false);
    }

    public class GrasshopperDocumentContext : IGrasshopperDocumentContext {
      public void AddObject(IGH_DocumentObject obj, bool recordUndo = false) {
        Instances.ActiveCanvas.Document.AddObject(obj, recordUndo);
      }
    }
  }
}
