using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

using AdSecGH.Helpers;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using GH_IO.Serialization;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using Oasys.AdSec;
using Oasys.AdSec.IO.Graphics.Section;
using Oasys.Profiles;

using OasysGH;
using OasysGH.Components;
using OasysGH.UI;

using Rhino.UI;

namespace AdSecGH.Components {
  public class SaveSVG : GH_OasysDropDownComponent {
    private static string imageSVG;
    private bool _canOpen;
    private string _fileName;

    public SaveSVG() : base("Section SVG", "SVG", "Creates a SVG file from an AdSec Section", CategoryName.Name(),
      SubCategoryName.Cat0()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("baf1ad7d-efca-4851-a6a3-21a65471a041");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.SVG;

    // This region handles input and output parameters

    public override void CreateAttributes() {
      m_attributes = new ThreeButtonComponentAttributes(this, "Save", "Save As", "Open SVG", SaveFile, SaveAsFile,
        OpenSVGexe, true, "Save SVG file");
    }

    public void OpenSVGexe() {
      if (_fileName != null) {
        if (_fileName != "") {
          if (_canOpen) {
            Process.Start(_fileName);
          } else {
            File.WriteAllText(_fileName, imageSVG);
            _canOpen = true;
          }
        }
      }
    }

    public override bool Read(GH_IReader reader) {
      _fileName = reader.GetString("File");
      return base.Read(reader);
    }

    public void SaveAsFile() {
      var fdi = new SaveFileDialog {
        Filter = "SVG File (*.svg)|*.svg|All files (*.*)|*.*",
      };
      bool res = fdi.ShowSaveDialog();
      if (res) // == DialogResult.OK)
      {
        _fileName = fdi.FileName;
        // write to file
        File.WriteAllText(_fileName, imageSVG);

        _canOpen = true;

        //add panel input with string
        //delete existing inputs if any
        while (Params.Input[2].Sources.Count > 0) {
          Instances.ActiveCanvas.Document.RemoveObject(Params.Input[2].Sources[0], false);
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
        Params.Input[2].AddSource(panel);
        Params.OnParametersChanged();
        ExpireSolution(true);
      }
    }

    public void SaveFile() {
      if (string.IsNullOrEmpty(_fileName)) {
        SaveAsFile();
      } else {
        // write to file
        File.WriteAllText(_fileName, imageSVG);
        _canOpen = true;
      }
    }

    public override void SetSelected(int i, int j) { }

    public override void VariableParameterMaintenance() {
      Params.Input[0].Optional = _fileName != null; //filename can have input from user input
      Params.Input[0]
       .ClearRuntimeMessages(); // this needs to be called to avoid having a runtime warning message after changed to optional
    }

    public override bool Write(GH_IWriter writer) {
      writer.SetString("File", _fileName);
      return base.Write(writer);
    }

    protected override void InitialiseDropdowns() { }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to save", GH_ParamAccess.item);
      pManager.AddBooleanParameter("Save?", "Save", "[Optional] Input 'True' to save or use button",
        GH_ParamAccess.item, false);
      pManager.AddTextParameter("File and Path", "File", "[Optional] Filename and path", GH_ParamAccess.item);
      pManager[1].Optional = true;
      pManager[2].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddTextParameter("SVG string", "SVG", "Text string representing the SVG file", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      var section = AdSecInput.AdSecSection(this, DA, 0);
      if (section == null) {
        return;
      }

      // create a flattened section
      ISection flat = null;
      if (section.DesignCode != null) {
        var adSec = IAdSec.Create(section.DesignCode);
        flat = adSec.Flatten(section.Section);
      } else {
        var prof = IPerimeterProfile.Create(section.Section.Profile);
        flat = ISection.Create(prof, section.Section.Material);
      }

      // construct image converter
      var sectionImageBuilder = new SectionImageBuilder(flat);

      // create svg string
      imageSVG = sectionImageBuilder.Svg();

      // replace colours:
      string concrete = "#CDCDCD";
      string steel = "#0061A0";
      string rebar = "#2D2D2D";
      string link = "#969696";

      imageSVG = imageSVG.Replace("#84D0FF", steel);
      imageSVG = imageSVG.Replace("#ABABAB", rebar);
      imageSVG = imageSVG.Replace("#CDCDCD", link);
      imageSVG = imageSVG.Replace("#8EFB8E", concrete);

      // filepath
      string pathString = "";
      if (DA.GetData(2, ref pathString)) {
        if (_fileName != pathString) {
          _fileName = pathString;
          _canOpen = false;
        }
      }

      // input save bool
      bool save = false;
      if (DA.GetData(1, ref save)) {
        if (save) {
          // write to file
          File.WriteAllText(_fileName, imageSVG);
          _canOpen = true;
        }
      }

      DA.SetData(0, imageSVG);
    }
  }
}
