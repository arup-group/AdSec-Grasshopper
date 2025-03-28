using System;
using System.Diagnostics;
using System.Drawing;
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

using OasysUnits;

namespace AdSecGH.Components {
  public class SaveModel : GH_OasysDropDownComponent {
    private string _jsonString;
    private string _fileName;
    private bool canOpen;

    public SaveModel() : base("Save AdSec", "Save",
      "Saves your AdSec Section with loads from this parametric nightmare", CategoryName.Name(),
      SubCategoryName.Cat0()) {
      Hidden = true; // sets the initial state of the component to hidden
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
        System.IO.File.WriteAllText(_fileName, _jsonString);
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

    private void WriteFilePathToPanel() {
      //add panel input with string
      //delete existing inputs if any
      while (Params.Input[3].Sources.Count > 0) {
        Instances.ActiveCanvas.Document.RemoveObject(Params.Input[3].Sources[0], false);
      }

      //instantiate  new panel
      var panel = new GH_Panel();
      panel.CreateAttributes();

      panel.Attributes.Pivot
        = new PointF(Attributes.DocObject.Attributes.Bounds.Left - panel.Attributes.Bounds.Width - 40,
          Attributes.DocObject.Attributes.Bounds.Bottom - panel.Attributes.Bounds.Height);

      //populate value list with our own data
      panel.UserText = _fileName;

      //Until now, the panel is a hypothetical object.
      // This command makes it 'real' and adds it to the canvas.
      Instances.ActiveCanvas.Document.AddObject(panel, false);

      //Connect the new slider to this component
      Params.Input[3].AddSource(panel);
      Params.OnParametersChanged();
      ExpireSolution(true);
    }

    public void SaveFile() {
      if (string.IsNullOrEmpty(_fileName)) {
        SaveAsFile();
      } else {
        // write to file
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

    protected override void SolveInternal(IGH_DataAccess da) {
      var sections = this.GetAdSecSections(da, 0);
      if (!sections.Any()) {
        return;
      }

      var loads = this.GetLoads(da, 1);

      _jsonString = AdSecFile.ModelJson(sections, loads);

      // filepath
      string pathString = "";
      if (da.GetData(3, ref pathString)) {
        _fileName = pathString;
        canOpen = false;
      }

      // input save bool
      bool save = false;
      if (da.GetData(2, ref save) && save) {
        // write to file
        SaveJson();
      }
    }

  }
}
