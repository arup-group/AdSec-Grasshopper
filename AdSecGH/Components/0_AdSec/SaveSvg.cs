using System;
using System.Drawing;
using System.IO;
using AdSecGH.Helpers;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Oasys.AdSec;
using Oasys.AdSec.IO.Graphics.Section;
using Oasys.Profiles;
using OasysGH;
using OasysGH.Components;
using OasysGH.UI;

namespace AdSecGH.Components
{
    public class SaveSVG : GH_OasysDropDownComponent
  {
    static string imageSVG;

    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("baf1ad7d-efca-4851-a6a3-21a65471a041");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Properties.Resources.SVG;
    private string _fileName = null;
    private bool _canOpen = false;

    public SaveSVG() : base(
      "Section SVG",
      "SVG",
      "Creates a SVG file from an AdSec Section",
      CategoryName.Name(),
      SubCategoryName.Cat0())
    {
      this.Hidden = true; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    // This region handles input and output parameters

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to save", GH_ParamAccess.item);
      pManager.AddBooleanParameter("Save?", "Save", "[Optional] Input 'True' to save or use button", GH_ParamAccess.item, false);
      pManager.AddTextParameter("File and Path", "File", "[Optional] Filename and path", GH_ParamAccess.item);
      pManager[1].Optional = true;
      pManager[2].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddTextParameter("SVG string", "SVG", "Text string representing the SVG file", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      AdSecSection section = AdSecInput.AdSecSection(this, DA, 0);
      if (section == null)
        return;

      // create a flattened section
      ISection flat = null;
      if (section.DesignCode != null)
      {
        IAdSec adSec = IAdSec.Create(section.DesignCode);
        flat = adSec.Flatten(section.Section);
      }
      else
      {
        IPerimeterProfile prof = IPerimeterProfile.Create(section.Section.Profile);
        flat = ISection.Create(prof, section.Section.Material);
      }

      // construct image converter
      SectionImageBuilder sectionImageBuilder = new SectionImageBuilder(flat);

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
      if (DA.GetData(2, ref pathString))
      {
        if (this._fileName != pathString)
        {
          this._fileName = pathString;
          _canOpen = false;
        }
      }

      // input save bool
      bool save = false;
      if (DA.GetData(1, ref save))
      {
        if (save)
        {
          // write to file
          File.WriteAllText(this._fileName, imageSVG);
          _canOpen = true;
        }
      }

      DA.SetData(0, imageSVG);
    }

    #region Custom UI
    public override void SetSelected(int i, int j)
    {
    }

    protected override void InitialiseDropdowns()
    {
    }

    public override void CreateAttributes()
    {
      m_attributes = new ThreeButtonAtrributes(this, "Save", "Save As", "Open SVG", SaveFile, SaveAsFile, OpenSVGexe, true, "Save SVG file");
    }

    public void SaveFile()
    {
      if (this._fileName == null | this._fileName == "")
        SaveAsFile();
      else
      {
        // write to file
        File.WriteAllText(this._fileName, imageSVG);
        _canOpen = true;
      }
    }

    public void SaveAsFile()
    {
      var fdi = new Rhino.UI.SaveFileDialog { Filter = "SVG File (*.svg)|*.svg|All files (*.*)|*.*" };
      var res = fdi.ShowSaveDialog();
      if (res) // == DialogResult.OK)
      {
        this._fileName = fdi.FileName;
        // write to file
        File.WriteAllText(this._fileName, imageSVG);

        _canOpen = true;

        //add panel input with string
        //delete existing inputs if any
        while (this.Params.Input[2].Sources.Count > 0)
          Grasshopper.Instances.ActiveCanvas.Document.RemoveObject(this.Params.Input[2].Sources[0], false);

        //instantiate  new panel
        var panel = new Grasshopper.Kernel.Special.GH_Panel();
        panel.CreateAttributes();

        panel.Attributes.Pivot = new PointF((float)Attributes.DocObject.Attributes.Bounds.Left -
            panel.Attributes.Bounds.Width - 40, (float)Attributes.DocObject.Attributes.Bounds.Bottom - panel.Attributes.Bounds.Height);

        //populate value list with our own data
        panel.UserText = this._fileName;

        //Until now, the panel is a hypothetical object.
        // This command makes it 'real' and adds it to the canvas.
        Grasshopper.Instances.ActiveCanvas.Document.AddObject(panel, false);

        //Connect the new slider to this component
        this.Params.Input[2].AddSource(panel);
        this.Params.OnParametersChanged();
        ExpireSolution(true);
      }
    }

    public void OpenSVGexe()
    {
      if (this._fileName != null)
      {
        if (this._fileName != "")
        {
          if (this._canOpen)
            System.Diagnostics.Process.Start(this._fileName);
          else
          {
            File.WriteAllText(this._fileName, imageSVG);
            this._canOpen = true;
          }
        }
      }
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      Params.Input[0].Optional = _fileName != null; //filename can have input from user input
      Params.Input[0].ClearRuntimeMessages(); // this needs to be called to avoid having a runtime warning message after changed to optional
    }

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      writer.SetString("File", this._fileName);
      return base.Write(writer);
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      this._fileName = reader.GetString("File");
      return base.Read(reader);
    }
    #endregion
  }
}

