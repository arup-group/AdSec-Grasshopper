﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.IO.Serialization;
using OasysGH;
using OasysGH.Components;
using OasysGH.UI;
using Rhino.Geometry;

namespace AdSecGH.Components
{
    public class OpenModel : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("42135d0f-bf55-40c0-8f6f-5dc2ad5f7741");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Properties.Resources.OpenAdSec;
    private string _fileName = null;
    private Guid _panelGuid = Guid.NewGuid();

    public OpenModel() : base(
      "Open Model",
      "Open",
      "Open an existing AdSec .ads file",
      CategoryName.Name(),
      SubCategoryName.Cat0())
    {
      this.Hidden = false; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Filename and path", "File", "AdSec file to open and work with." + System.Environment.NewLine + "Input either path component, a text string with path and " + System.Environment.NewLine + "filename or an existing AdSec File created in Grasshopper.", GH_ParamAccess.item);
      pManager.AddPlaneParameter("LocalPlane", "Pln", "[Optional] Plane representing local coordinate system, by default a YZ-plane is used", GH_ParamAccess.list, Plane.WorldYZ);
      pManager[1].Optional = true;
      pManager.HideParameter(1);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Sections", GH_ParamAccess.list);
    }
    #endregion

    #region Custom UI
    public override void CreateAttributes()
    {
      m_attributes = new ButtonComponentAttributes(this, "Open", OpenFile, "Open AdSec file");
    }
    protected override void InitialiseDropdowns()
    {
    }

    public override void SetSelected(int i, int j)
    {
    }

    public void OpenFile()
    {
      var fdi = new Rhino.UI.OpenFileDialog { Filter = "AdSec Files(*.ads)|*.ads|All files (*.*)|*.*" };
      var res = fdi.ShowOpenDialog();
      if (res) // == DialogResult.OK)
      {
        _fileName = fdi.FileName;

        // instantiate  new panel
        var panel = new Grasshopper.Kernel.Special.GH_Panel();
        panel.CreateAttributes();

        // set the location relative to the open component on the canvas
        panel.Attributes.Pivot = new PointF((float)Attributes.DocObject.Attributes.Bounds.Left -
            panel.Attributes.Bounds.Width - 30, (float)Params.Input[0].Attributes.Pivot.Y - panel.Attributes.Bounds.Height / 2);

        // check for existing input
        while (Params.Input[0].Sources.Count > 0)
        {
          var input = Params.Input[0].Sources[0];
          // check if input is the one we automatically create below
          if (Params.Input[0].Sources[0].InstanceGuid == _panelGuid)
          {
            // update the UserText in existing panel
            //RecordUndoEvent("Changed OpenGSA Component input");
            panel = input as Grasshopper.Kernel.Special.GH_Panel;
            panel.UserText = _fileName;
            panel.ExpireSolution(true); // update the display of the panel
          }

          // remove input
          Params.Input[0].RemoveSource(input);
        }

        //populate panel with our own content
        panel.UserText = _fileName;

        // record the panel's GUID if new, so that we can update it on change
        _panelGuid = panel.InstanceGuid;

        //Until now, the panel is a hypothetical object.
        // This command makes it 'real' and adds it to the canvas.
        Grasshopper.Instances.ActiveCanvas.Document.AddObject(panel, false);

        //Connect the new slider to this component
        Params.Input[0].AddSource(panel);

        (this as IGH_VariableParameterComponent).VariableParameterMaintenance();
        Params.OnParametersChanged();

        ExpireSolution(true);
      }
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      Params.Input[0].Optional = _fileName != null; //filename can have input from user input
      Params.Input[0].ClearRuntimeMessages(); // this needs to be called to avoid having a runtime warning message after changed to optional
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(0, ref gh_typ))
      {
        if (gh_typ.Value is GH_String)
        {
          string tempfile = "";
          if (GH_Convert.ToString(gh_typ, out tempfile, GH_Conversion.Both))
            _fileName = tempfile;

          if (!_fileName.EndsWith(".ads"))
            _fileName = _fileName + ".ads";

          string json = File.ReadAllText(_fileName);
          ParsedResult jsonParser = JsonParser.Deserialize(json);

          List<Plane> planes = new List<Plane>();
          DA.GetDataList(1, planes);

          List<AdSecSectionGoo> sections = new List<AdSecSectionGoo>();
          AdSecDesignCode code = Helpers.AdSecFile.GetDesignCode(json);

          for (int i = 0; i < jsonParser.Sections.Count; i++)
          {
            Oasys.AdSec.ISection section = jsonParser.Sections[i];
            Plane pln = (i > planes.Count - 1) ? planes.Last() : planes[i];
            sections.Add(new AdSecSectionGoo(new AdSecSection(section, code.DesignCode, code.DesignCodeName, "", pln)));
          }

          if (sections.Count == 0)
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File contains no valid sections");
          foreach (Oasys.AdSec.IWarning warning in jsonParser.Warnings)
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, warning.Description);
          DA.SetDataList(0, sections);
        }
      }
    }

    #region (de)serialization
    public override bool Write(GH_IO.Serialization.GH_IWriter writer)
    {
      writer.SetString("File", (string)_fileName);
      return base.Write(writer);
    }

    public override bool Read(GH_IO.Serialization.GH_IReader reader)
    {
      _fileName = (string)reader.GetString("File");
      return base.Read(reader);
    }
    #endregion
  }
}
