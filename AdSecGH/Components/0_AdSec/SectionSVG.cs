using System;
using System.Drawing;
using System.IO;
using AdSecGH.Helpers;
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
   public class SectionSVG : GH_OasysComponent
  {
    static string imageSVG;

    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("baf1ad7d-efca-4851-a6a3-21a65471a041");
    public override GH_Exposure Exposure => GH_Exposure.secondary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Properties.Resources.SVG;

    public SectionSVG()
      : base("Section SVG", "SVG", "Creates a SVG file from an AdSec Section",
            Ribbon.CategoryName.Name(),
            Ribbon.SubCategoryName.Cat0())
    { this.Hidden = true; } // sets the initial state of the component to hidden
    #endregion

    #region Custom UI
    //This region overrides the typical component layout
    public override void CreateAttributes()
    {
      m_attributes = new ThreeButtonAtrributes(this, "Save", "Save As", "Open SVG", SaveFile, SaveAsFile, OpenSVGexe, true, "Save SVG file");
    }

    public void SaveFile()
    {
      if (fileName == null | fileName == "")
        SaveAsFile();
      else
      {
        // write to file
        File.WriteAllText(fileName, imageSVG);
        canOpen = true;
      }
    }

    public void SaveAsFile()
    {
      var fdi = new Rhino.UI.SaveFileDialog { Filter = "SVG File (*.svg)|*.svg|All files (*.*)|*.*" };
      var res = fdi.ShowSaveDialog();
      if (res) // == DialogResult.OK)
      {
        fileName = fdi.FileName;
        // write to file
        File.WriteAllText(fileName, imageSVG);

        canOpen = true;

        //add panel input with string
        //delete existing inputs if any
        while (Params.Input[2].Sources.Count > 0)
          Grasshopper.Instances.ActiveCanvas.Document.RemoveObject(Params.Input[2].Sources[0], false);

        //instantiate  new panel
        var panel = new Grasshopper.Kernel.Special.GH_Panel();
        panel.CreateAttributes();

        panel.Attributes.Pivot = new PointF((float)Attributes.DocObject.Attributes.Bounds.Left -
            panel.Attributes.Bounds.Width - 40, (float)Attributes.DocObject.Attributes.Bounds.Bottom - panel.Attributes.Bounds.Height);

        //populate value list with our own data
        panel.UserText = fileName;

        //Until now, the panel is a hypothetical object.
        // This command makes it 'real' and adds it to the canvas.
        Grasshopper.Instances.ActiveCanvas.Document.AddObject(panel, false);

        //Connect the new slider to this component
        Params.Input[2].AddSource(panel);
        Params.OnParametersChanged();
        ExpireSolution(true);
      }
    }

    public void OpenSVGexe()
    {
      if (fileName != null)
      {
        if (fileName != "")
        {
          if (canOpen)
            System.Diagnostics.Process.Start(fileName);
          else
          {
            File.WriteAllText(fileName, imageSVG);
            canOpen = true;
          }
        }
      }
    }
    #endregion

    #region Input and output
    // This region handles input and output parameters
    string fileName = null;
    bool canOpen = false;
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

    #region IGH_VariableParameterComponent null implementation
    //This sub region handles any changes to the component after it has been placed on the canvas
    bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
    {
      return false;
    }
    bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
    {
      return false;
    }
    IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
    {
      return null;
    }
    bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
    {
      return false;
    }
    void IGH_VariableParameterComponent.VariableParameterMaintenance()
    {
      Params.Input[0].Optional = fileName != null; //filename can have input from user input
      Params.Input[0].ClearRuntimeMessages(); // this needs to be called to avoid having a runtime warning message after changed to optional
    }
    #endregion

    #region (de)serialization
    //This region handles serialisation and deserialisation, meaning that 
    // component states will be remembered when reopening GH script
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      writer.SetString("File", (string)fileName);
      return base.Write(writer);
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      fileName = (string)reader.GetString("File");
      return base.Read(reader);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      AdSecSection section = GetInput.AdSecSection(this, DA, 0);
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
        if (fileName != pathString)
        {
          fileName = pathString;
          canOpen = false;
        }
      }

      // input save bool
      bool save = false;
      if (DA.GetData(1, ref save))
      {
        if (save)
        {
          // write to file
          File.WriteAllText(fileName, imageSVG);
          canOpen = true;
        }
      }

      DA.SetData(0, imageSVG);
    }
  }
}

