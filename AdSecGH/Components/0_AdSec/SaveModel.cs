using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using GH_IO.Serialization;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.IO.Serialization;

using OasysGH;
using OasysGH.Components;
using OasysGH.UI;

using Rhino.UI;

namespace AdSecGH.Components {
  public class SaveModel : GH_OasysDropDownComponent {
    private static string _jsonString;
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
      m_attributes = new ThreeButtonComponentAttributes(this, "Save", "Save As", "Open AdSec", SaveFile, SaveAsFile,
        OpenAdSecexe, true, "Save AdSec file");
    }

    public void OpenAdSecexe() {
      if (_fileName != null) {
        if (_fileName != "") {
          if (canOpen) {
            Process.Start(_fileName);
          } else {
            File.WriteAllText(_fileName, _jsonString);
            canOpen = true;
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
        Filter = "AdSec File (*.ads)|*.ads|All files (*.*)|*.*",
      };
      bool res = fdi.ShowSaveDialog();
      if (res) // == DialogResult.OK)
      {
        _fileName = fdi.FileName;

        // write to file
        File.WriteAllText(_fileName, _jsonString);
        canOpen = true;

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
    }

    public void SaveFile() {
      if (string.IsNullOrEmpty(_fileName)) {
        SaveAsFile();
      } else {
        // write to file
        File.WriteAllText(_fileName, _jsonString);
        canOpen = true;
      }
    }

    public override void SetSelected(int i, int j) { }

    public override bool Write(GH_IWriter writer) {
      writer.SetString("File", _fileName);
      return base.Write(writer);
    }

    internal static string CombineJSonStrings(List<string> jsonStrings) {
      if (jsonStrings == null || jsonStrings.Count == 0) {
        return null;
      }

      string jsonString = jsonStrings[0].Remove(jsonStrings[0].Length - 2, 2);
      for (int i = 1; i < jsonStrings.Count; i++) {
        string jsonString2 = jsonStrings[i];
        int start = jsonString2.IndexOf("components") - 2;
        jsonString2 = $",{jsonString2.Substring(start)}";
        jsonString += jsonString2.Remove(jsonString2.Length - 2, 2);
      }

      jsonString += jsonStrings[0].Substring(jsonStrings[0].Length - 2);

      return jsonString;
    }

    protected override void InitialiseDropdowns() { }

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
      var sections = AdSecInput.AdSecSections(this, DA, 0);
      if (sections.Count == 0) {
        return;
      }

      if (sections.Count > 1) {
        AddRuntimeMessage(GH_RuntimeMessageLevel.Remark,
          "Note that the first Section's designcode will be used for all sections in list");
      }

      var jsonStrings = new List<string>();

      var json = new JsonConverter(sections[0].DesignCode);

      if (DA.GetDataTree(1, out GH_Structure<IGH_Goo> loads)) {
        if (loads.Branches.Count > 0) {
          for (int i = 0; i < sections.Count; i++) {
            if (loads.Branches[i] == null || loads.Branches[i].Count == 0) {
              // convert to json without loads method
              try {
                jsonStrings.Add(json.SectionToJson(sections[i].Section));
              } catch (Exception e) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                  $"Error with section at index {i}: {e.InnerException.Message}");
              }
            } else if (loads.Branches[i][0].CastTo(out AdSecLoadGoo notusedLoad)) {
              // create new list of loads
              var lds = Oasys.Collections.IList<ILoad>.Create();
              // loop through input list
              for (int j = 0; j < loads.Branches[i].Count; j++) {
                // check if item is load type
                if (loads[i][j].CastTo(out AdSecLoadGoo loadGoo)) {
                  var load = loadGoo;
                  lds.Add(load.Value);
                } else {
                  AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Unable to convert {Params.Input[1].NickName} path {i} index {j} to AdSec Load. Section will be saved without this load.");
                }
              }

              // convert to json with load method
              try {
                jsonStrings.Add(json.SectionToJson(sections[i].Section, lds));
              } catch (Exception e) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                  $"Error with section at index {i}: {e.InnerException.Message}");
              }
            } else if (loads.Branches[i][0] is AdSecDeformationGoo) {
              // create new list of deformations
              var defs = Oasys.Collections.IList<IDeformation>.Create();
              // loop through input list
              for (int j = 0; j < loads.Branches[i].Count; j++) {
                // check if item is load type
                if (loads[i][j] is AdSecDeformationGoo def) {
                  defs.Add(def.Value);
                } else {
                  AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Unable to convert {Params.Input[1].NickName} path {i} index {j} to AdSec Load. Section will be saved without this load.");
                }
              }

              // convert to json with deformation method
              try {
                jsonStrings.Add(json.SectionToJson(sections[i].Section, defs));
              } catch (Exception e) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                  $"Error with section at index {i}: {e.InnerException.Message}");
              }
            } else {
              AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                $"Error converting {Params.Input[1].NickName} to AdSec Load/Deformation. Section will be saved without loads.");
              // convert to json without loads method
              try {
                jsonStrings.Add(json.SectionToJson(sections[i].Section));
              } catch (Exception e) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                  $"Error with section at index {i}: {e.InnerException.Message}");
              }
            }
          }
        } else {
          for (int i = 0; i < sections.Count; i++) {
            // if no loads are inputted then just convert section
            try {
              jsonStrings.Add(json.SectionToJson(sections[i].Section));
            } catch (Exception e) {
              AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                $"Error with section at index {i}: {e.InnerException.Message}");
            }
          }
        }
      } else {
        for (int i = 0; i < sections.Count; i++) {
          // if no loads are inputted then just convert section
          try {
            jsonStrings.Add(json.SectionToJson(sections[i].Section));
          } catch (Exception e) {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
              $"Error with section at index {i}: {e.InnerException.Message}");
          }
        }
      }

      _jsonString = CombineJSonStrings(jsonStrings);

      // filepath
      string pathString = "";
      if (DA.GetData(3, ref pathString)) {
        if (_fileName != pathString) {
          _fileName = pathString;
          canOpen = false;
        }
      }

      // input save bool
      bool save = false;
      if (DA.GetData(2, ref save)) {
        if (save) {
          // write to file
          File.WriteAllText(_fileName, _jsonString);
          canOpen = true;
        }
      }
    }
  }
}
