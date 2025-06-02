using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using AdSecCore.Helpers;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;

using Oasys.AdSec.IO.Serialization;

using OasysGH;
using OasysGH.Components;
using OasysGH.UI;

using Rhino.Geometry;
using Rhino.UI;

namespace AdSecGH.Components {
  public class OpenModel : GH_OasysDropDownComponent {

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("42135d0f-bf55-40c0-8f6f-5dc2ad5f7741");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.OpenAdSec;
    private Guid _panelGuid = Guid.NewGuid();

    public OpenModel() : base("Open Model", "Open", "Open an existing AdSec .ads file", CategoryName.Name(),
      SubCategoryName.Cat0()) {
      Hidden = false; // sets the initial state of the component to hidden
    }

    public override void CreateAttributes() {
      m_attributes = new ButtonComponentAttributes(this, "Open", OpenFile, "Open AdSec file");
    }

    public void OpenFile() {
      string fileName = ShowOpenFileDialog();
      if (string.IsNullOrEmpty(fileName)) {
        return;
      }

      var panel = CreatePanel();
      SetPanelLocation(panel);

      UpdatePanelForExistingSources(panel, fileName);

      // Ensure the panel is real and added to the canvas
      Instances.ActiveCanvas.Document.AddObject(panel, false);

      // Connect the new panel to the component
      ConnectNewPanelToComponent(panel);

      ExpireSolution(true);
    }

    private static string ShowOpenFileDialog() {
      var openFileDialog = new OpenFileDialog {
        Filter = "AdSec Files(*.ads)|*.ads|All files (*.*)|*.*",
      };

      bool showOpenDialog = openFileDialog.ShowOpenDialog();
      return showOpenDialog ? openFileDialog.FileName : null;
    }

    private static GH_Panel CreatePanel() {
      var panel = new GH_Panel();
      panel.CreateAttributes();
      return panel;
    }

    private void SetPanelLocation(IGH_ActiveObject panel) {
      panel.Attributes.Pivot = new PointF(
        Attributes.DocObject.Attributes.Bounds.Left - panel.Attributes.Bounds.Width - 30,
        Params.Input[0].Attributes.Pivot.Y - (panel.Attributes.Bounds.Height / 2));
    }

    private void UpdatePanelForExistingSources(GH_Panel panel, string fileName) {
      foreach (var input in Params.Input[0].Sources.ToList()) // ToList() to avoid modifying collection while iterating
      {
        if (input.InstanceGuid == _panelGuid) {
          panel = input as GH_Panel;
          UpdatePanelWithFileName(panel, fileName);
        }

        Params.Input[0].RemoveSource(input);
      }

      if (panel == null) {
        return;
      }

      panel.UserText = fileName; // Ensure the panel has the correct file name
      _panelGuid = panel.InstanceGuid;
    }

    private void ConnectNewPanelToComponent(GH_Panel panel) {
      Params.Input[0].AddSource(panel);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
    }

    private static void UpdatePanelWithFileName(GH_Panel panel, string fileName) {
      if (panel == null) {
        return;
      }

      panel.UserText = fileName;
      panel.ExpireSolution(true);
    }

    public override void SetSelected(int i, int j) { }

    protected override void InitialiseDropdowns() { }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Filename and path", "File",
        $"AdSec file to open and work with.{Environment.NewLine}Input either path component, a text string with path and {Environment.NewLine}filename or an existing AdSec File created in Grasshopper.",
        GH_ParamAccess.item);
      pManager.AddPlaneParameter("LocalPlane", "Pln",
        "[Optional] Plane representing local " + "coordinate system, by default a YZ-plane is used",
        GH_ParamAccess.list, Plane.WorldYZ);
      pManager[1].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Sections", GH_ParamAccess.list);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      var objectWrapper = new GH_ObjectWrapper();
      DA.GetData(0, ref objectWrapper);
      GH_Convert.ToString(objectWrapper, out string fileName, GH_Conversion.Both);
      if (!fileName.EndsWith(".ads")) {
        fileName += ".ads";
      }

      string json = File.ReadAllText(fileName);
      var jsonParser = JsonParser.Deserialize(json);

      var planes = new List<Plane>();
      DA.GetDataList(1, planes);

      var sections = new List<AdSecSectionGoo>();
      var code = CodeHelper.GetDesignCode(json);
      if (code == null) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to read DesignCode. DesignCode set to Eurocode.");
        code = new AdSecDesignCode {
          DesignCode = FileHelper.Codes["EC2_04"],
          DesignCodeName = "EC2_04",
        };
      }

      for (int i = 0; i < jsonParser.Sections.Count; i++) {
        var section = jsonParser.Sections[i];
        var plane = i > planes.Count - 1 ? planes[planes.Count - 1] : planes[i];
        sections.Add(new AdSecSectionGoo(new AdSecSection(section, code.DesignCode, code.DesignCodeName, "", plane)));
      }

      if (sections.Count == 0) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File contains no valid sections");
      }

      foreach (var warning in jsonParser.Warnings) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning.Description);
      }

      DA.SetDataList(0, sections);
    }
  }
}
