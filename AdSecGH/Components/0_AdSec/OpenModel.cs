using System;
using System.Drawing;
using System.Linq;

using AdSecGH.Properties;

using AdSecGHCore.Functions;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using Oasys.GH.Helpers;

using OasysGH;
using OasysGH.UI;

using Rhino.UI;

namespace AdSecGH.Components {
  interface IShowDialog {
    string FileName { get; set; }
    bool ShowOpenDialog();
  }

  public class DialogGh : IShowDialog {

    public string FileName { get; set; }
    internal readonly OpenFileDialog openFileDialog;

    public DialogGh() {
      openFileDialog = new OpenFileDialog {
        Filter = "AdSec Files(*.ads)|*.ads|All files (*.*)|*.*",
        Title = "Open AdSec Model",
      };
    }

    public bool ShowOpenDialog() {
      return openFileDialog.ShowOpenDialog();
    }
  }

  public class OpenModel : DropdownAdapter<OpenModelFunction> {

    internal IShowDialog OpenFileDialog { get; set; } = new DialogGh();
    public override Guid ComponentGuid => new Guid("42135d0f-bf55-40c0-8f6f-5dc2ad5f7741");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.OpenAdSec;
    private GH_Panel _panel;

    public override void CreateAttributes() {
      m_attributes = new ButtonComponentAttributes(this, "Open", () => OpenFile(), "Open AdSec file");
    }

    public void OpenFile(string file = null) {
      string fileName = file ?? ShowOpenFileDialog();
      if (string.IsNullOrEmpty(fileName)) {
        return;
      }

      if (_panel == null || OnPingDocument().FindObject(_panel.InstanceGuid, false) == null) {
        CreatePanel();
      }

      UpdatePanelForExistingSources(fileName);

      // Connect the new panel to the component
      ConnectNewPanelToComponent(_panel);

      ExpireSolution(true);
    }

    private string ShowOpenFileDialog() {
      bool showOpenDialog = OpenFileDialog.ShowOpenDialog();
      return showOpenDialog ? OpenFileDialog.FileName : null;
    }

    private void CreatePanel() {
      _panel = new GH_Panel();
      _panel.CreateAttributes();
      SetPanelLocation(_panel);

      // Ensure the panel is real and added to the canvas
      OnPingDocument().AddObject(_panel, false);
    }

    private void SetPanelLocation(IGH_ActiveObject panel) {
      panel.Attributes.Pivot = new PointF(
        Attributes.DocObject.Attributes.Bounds.Left - panel.Attributes.Bounds.Width - 30,
        Params.Input[0].Attributes.Pivot.Y - (panel.Attributes.Bounds.Height / 2));
    }

    private void UpdatePanelForExistingSources(string fileName) {
      _panel.UserText = fileName;
      _panel.ExpireSolution(true);
    }

    private void ConnectNewPanelToComponent(GH_Panel panel) {
      Params.Input[0].AddSource(panel);

      (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
      Params.OnParametersChanged();
    }
  }
}
