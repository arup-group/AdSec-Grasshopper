using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AdSecGH.Helpers;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.IO.Serialization;
using OasysGH;
using OasysGH.Components;
using OasysGH.UI;

namespace AdSecGH.Components
{
  public class SaveModel : GH_OasysDropDownComponent
  {
    static string _jsonString;
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("6bba517c-3ec1-45da-a520-ea117f7f901a");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Properties.Resources.SaveAdSec;

    private string _fileName = null;

    bool canOpen = false;


    public SaveModel() : base(
      "Save AdSec",
      "Save",
      "Saves your AdSec Section with loads from this parametric nightmare",
      Ribbon.CategoryName.Name(),
      Ribbon.SubCategoryName.Cat0())
    {
      this.Hidden = true; // sets the initial state of the component to hidden
    }
    #endregion

    internal static string CombineJSonStrings(List<string> jsonStrings)
    {
      if (jsonStrings == null | jsonStrings.Count == 0)
        return null;
      string jsonString = jsonStrings[0].Remove(jsonStrings[0].Length - 2, 2);
      for (int i = 1; i < jsonStrings.Count; i++)
      {
        string jsonString2 = jsonStrings[i];
        int start = jsonString2.IndexOf("components") - 2;
        jsonString2 = "," + jsonString2.Substring(start);
        jsonString += jsonString2.Remove(jsonString2.Length - 2, 2);
      }
      jsonString += jsonStrings[0].Substring(jsonStrings[0].Length - 2);

      return jsonString;
    }

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddGenericParameter("Section", "Sec", "AdSec Section to save", GH_ParamAccess.list);
      pManager.AddGenericParameter("Loads", "Lds", "[Optional] List of AdSec Loads (consistent Load or Deformation type)", GH_ParamAccess.tree);
      pManager.AddBooleanParameter("Save?", "Save", "[Optional] Input 'True' to save or use button", GH_ParamAccess.item, false);
      pManager.AddTextParameter("File and Path", "File", "[Optional] Filename and path", GH_ParamAccess.item);
      pManager[1].Optional = true;
      pManager[2].Optional = true;
      pManager[3].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      List<AdSecSection> sections = AdSecInput.AdSecSections(this, DA, 0);
      if (sections.Count == 0)
        return;

      if (sections.Count > 1)
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Note that the first Section's designcode will be used for all sections in list");

      List<string> jsonStrings = new List<string>();

      // construct json converter
      JsonConverter json = new JsonConverter(sections[0].DesignCode);

      //GH_Structure<IGH_Goo> loads = new GH_Structure<IGH_Goo>();
      //List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
      //DA.GetDataTree(0, out GH_Structure<IGH_Goo> tree);
      //tree = CleanTree(tree);
      if (DA.GetDataTree(1, out GH_Structure<IGH_Goo> loads))
      {
        if (loads.Branches.Count > 0)
        {
          for (int i = 0; i < sections.Count; i++)
          {
            if (loads.Branches[i] == null | loads.Branches[i].Count == 0)
            {
              // convert to json without loads method
              try
              {
                jsonStrings.Add(json.SectionToJson(sections[i].Section));
              }
              catch (Exception e)
              {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error with section at index " + i + ": " + e.InnerException.Message);
              }
            }
            else if (loads.Branches[i][0].CastTo(out AdSecLoadGoo notusedLoad))
            {
              // create new list of loads
              Oasys.Collections.IList<Oasys.AdSec.ILoad> lds = Oasys.Collections.IList<Oasys.AdSec.ILoad>.Create();
              // loop through input list
              for (int j = 0; j < loads.Branches[i].Count; j++)
              {
                // check if item is load type
                if (loads[i][j].CastTo(out AdSecLoadGoo loadGoo))
                {
                  AdSecLoadGoo load = (AdSecLoadGoo)loadGoo;
                  lds.Add(load.Value);
                }
                else
                {
                  AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + Params.Input[1].NickName + " path " + i + " index " + j + " to AdSec Load. Section will be saved without this load.");
                }
              }
              // convert to json with load method
              try
              {
                jsonStrings.Add(json.SectionToJson(sections[i].Section, lds));

              }
              catch (Exception e)
              {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error with section at index " + i + ": " + e.InnerException.Message);
              }
            }
            else if (loads.Branches[i][0] is AdSecDeformationGoo)
            {
              // create new list of deformations
              Oasys.Collections.IList<Oasys.AdSec.IDeformation> defs = Oasys.Collections.IList<Oasys.AdSec.IDeformation>.Create();
              // loop through input list
              for (int j = 0; j < loads.Branches[i].Count; j++)
              {
                // check if item is load type
                if (loads[i][j] is AdSecDeformationGoo)
                {
                  AdSecDeformationGoo def = (AdSecDeformationGoo)loads[i][j];
                  defs.Add(def.Value);
                }
                else
                {
                  AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + Params.Input[1].NickName + " path " + i + " index " + j + " to AdSec Load. Section will be saved without this load.");
                }
              }
              // convert to json with deformation method
              try
              {
                jsonStrings.Add(json.SectionToJson(sections[i].Section, defs));
              }
              catch (Exception e)
              {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error with section at index " + i + ": " + e.InnerException.Message);
              }
            }
            else
            {
              AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error converting " + Params.Input[1].NickName + " to AdSec Load/Deformation. Section will be saved without loads.");
              // convert to json without loads method
              try
              {
                jsonStrings.Add(json.SectionToJson(sections[i].Section));
              }
              catch (Exception e)
              {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error with section at index " + i + ": " + e.InnerException.Message);
              }
            }
          }
        }
        else
        {
          for (int i = 0; i < sections.Count; i++)
          {
            // if no loads are inputted then just convert section
            try
            {
              jsonStrings.Add(json.SectionToJson(sections[i].Section));
            }
            catch (Exception e)
            {
              AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error with section at index " + i + ": " + e.InnerException.Message);
            }
          }
        }
      }
      else
      {
        for (int i = 0; i < sections.Count; i++)
        {
          // if no loads are inputted then just convert section
          try
          {
            jsonStrings.Add(json.SectionToJson(sections[i].Section));
          }
          catch (Exception e)
          {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error with section at index " + i + ": " + e.InnerException.Message);
          }
        }
      }

      _jsonString = CombineJSonStrings(jsonStrings);

      // filepath
      string pathString = "";
      if (DA.GetData(3, ref pathString))
      {
        if (_fileName != pathString)
        {
          _fileName = pathString;
          canOpen = false;
        }
      }

      // input save bool
      bool save = false;
      if (DA.GetData(2, ref save))
      {
        if (save)
        {
          // write to file
          File.WriteAllText(_fileName, _jsonString);
          canOpen = true;
        }
      }
    }

    #region Custom UI
    public override void SetSelected(int i, int j) { }

    public override void InitialiseDropdowns() { }

    public override void CreateAttributes()
    {
      m_attributes = new ThreeButtonAtrributes(this, "Save", "Save As", "Open AdSec", SaveFile, SaveAsFile, OpenAdSecexe, true, "Save AdSec file");
    }

    public void SaveFile()
    {
      if (this._fileName == null | this._fileName == "")
        SaveAsFile();
      else
      {
        // write to file
        File.WriteAllText(this._fileName, _jsonString);
        this.canOpen = true;
      }
    }

    public void SaveAsFile()
    {
      var fdi = new Rhino.UI.SaveFileDialog { Filter = "AdSec File (*.ads)|*.ads|All files (*.*)|*.*" };
      var res = fdi.ShowSaveDialog();
      if (res) // == DialogResult.OK)
      {
        _fileName = fdi.FileName;

        // write to file
        File.WriteAllText(_fileName, _jsonString);
        canOpen = true;

        //add panel input with string
        //delete existing inputs if any
        while (Params.Input[3].Sources.Count > 0)
          Grasshopper.Instances.ActiveCanvas.Document.RemoveObject(Params.Input[3].Sources[0], false);

        //instantiate  new panel
        var panel = new Grasshopper.Kernel.Special.GH_Panel();
        panel.CreateAttributes();

        panel.Attributes.Pivot = new PointF((float)Attributes.DocObject.Attributes.Bounds.Left -
            panel.Attributes.Bounds.Width - 40, (float)Attributes.DocObject.Attributes.Bounds.Bottom - panel.Attributes.Bounds.Height);

        //populate value list with our own data
        panel.UserText = _fileName;

        //Until now, the panel is a hypothetical object.
        // This command makes it 'real' and adds it to the canvas.
        Grasshopper.Instances.ActiveCanvas.Document.AddObject(panel, false);

        //Connect the new slider to this component
        Params.Input[3].AddSource(panel);
        Params.OnParametersChanged();
        ExpireSolution(true);
      }
    }

    public void OpenAdSecexe()
    {
      if (_fileName != null)
      {
        if (_fileName != "")
        {
          if (canOpen)
            System.Diagnostics.Process.Start(_fileName);
          else
          {
            File.WriteAllText(_fileName, _jsonString);
            canOpen = true;
          }
        }
      }
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      Params.Input[0].Optional = _fileName != null; //filename can have input from user input
      Params.Input[0].ClearRuntimeMessages(); // this needs to be called to avoid having a runtime warning message after changed to optional
      Params.Input[0].Access = GH_ParamAccess.list;
      Params.Input[1].Access = GH_ParamAccess.tree;
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