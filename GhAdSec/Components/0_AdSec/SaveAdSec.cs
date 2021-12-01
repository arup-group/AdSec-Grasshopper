using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.IO;
using Oasys.AdSec.IO.Serialization;
using AdSecGH.Parameters;

namespace AdSecGH.Components
{
    /// <summary>
    /// Component to open an existing GSA model
    /// </summary>
    public class SaveAdSec : GH_Component, IGH_VariableParameterComponent
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("6bba517c-3ec1-45da-a520-ea117f7f901a");
        public SaveAdSec()
          : base("Save AdSec", "Save", "Saves your AdSec Section with loads from this parametric nightmare",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat0())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.primary;

        //protected override System.Drawing.Bitmap Icon => AdSecGH.Properties.Resources.SaveModel;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            m_attributes = new UI.Button3ComponentUI(this, "Save", "Save As", "Open AdSec", SaveFile, SaveAsFile, OpenAdSecexe, true, "Save GSA file");
        }

        public void SaveFile()
        {
            if (fileName == null | fileName == "")
                SaveAsFile();
            else
            {
                // write to file
                File.WriteAllText(fileName, jsonString);
                canOpen = true;
            }
        }

        public void SaveAsFile()
        {
            var fdi = new Rhino.UI.SaveFileDialog { Filter = "AdSec File (*.ads)|*.ads|All files (*.*)|*.*" };
            var res = fdi.ShowSaveDialog();
            if (res) // == DialogResult.OK)
            {
                fileName = fdi.FileName;
                usersetFileName = true;
                // write to file
                File.WriteAllText(fileName, jsonString);

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
                panel.UserText = fileName;

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
            if (fileName != null)
            {
                if (fileName != "")
                {
                    if (canOpen)
                        System.Diagnostics.Process.Start(fileName);
                }
            }
        }
        #endregion

        #region Input and output
        // This region handles input and output parameters

        string fileName = null;
        bool usersetFileName = false;
        static string jsonString;
        bool canOpen = false;
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Section", "Sec", "AdSec Section to save", GH_ParamAccess.item);
            pManager.AddGenericParameter("Loads", "Lds", "[Optional] List of AdSec Loads (consistent Load or Deformation type)", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Save?", "Save", "Input 'True' to save or use button", GH_ParamAccess.item, false);
            pManager.AddTextParameter("File and Path", "File", "Filename and path", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }
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

            //    Params.Output[i].NickName = "P";
            //    Params.Output[i].Name = "Points";
            //    Params.Output[i].Description = "Points imported from GSA";
            //    Params.Output[i].Access = GH_ParamAccess.list;

        }
        #endregion
        #endregion

        #region (de)serialization
        //This region handles serialisation and deserialisation, meaning that 
        // component states will be remembered when reopening GH script
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            //writer.SetInt32("Mode", (int)_mode);
            writer.SetString("File", (string)fileName);
            //writer.SetBoolean("Advanced", (bool)advanced);
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            //_mode = (FoldMode)reader.GetInt32("Mode");
            fileName = (string)reader.GetString("File");
            //advanced = (bool)reader.GetBoolean("Advanced");
            return base.Read(reader);
        }
        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            AdSecSection section = GetInput.AdSecSection(this, DA, 0);
            if (section == null)
                return;

            // construct json converter
            JsonConverter json = new JsonConverter(section.DesignCode);
            
            List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
            if (DA.GetDataList(1, gh_typs))
            {
                if (gh_typs[0].Value is AdSecLoadGoo)
                {
                    // create new list of loads
                    Oasys.Collections.IList<Oasys.AdSec.ILoad> lds = Oasys.Collections.IList<Oasys.AdSec.ILoad>.Create();
                    // loop through input list
                    for (int i = 0; i < gh_typs.Count; i++)
                    {
                        // check if item is load type
                        if (gh_typs[i].Value is AdSecLoadGoo)
                        {
                            AdSecLoadGoo load = (AdSecLoadGoo)gh_typs[i].Value;
                            lds.Add(load.Value);
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + Params.Input[1].NickName + " index " + i + " to AdSec Load. Section will be saved without this load.");
                        }
                    }
                    // convert to json with load method
                    jsonString = json.SectionToJson(section.Section, lds);
                }
                else if (gh_typs[0].Value is AdSecDeformationGoo)
                {
                    // create new list of deformations
                    Oasys.Collections.IList<Oasys.AdSec.IDeformation> defs = Oasys.Collections.IList<Oasys.AdSec.IDeformation>.Create();
                    // loop through input list
                    for (int i = 0; i < gh_typs.Count; i++)
                    {
                        // check if item is load type
                        if (gh_typs[i].Value is AdSecDeformationGoo)
                        {
                            AdSecDeformationGoo def = (AdSecDeformationGoo)gh_typs[i].Value;
                            defs.Add(def.Value);
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + Params.Input[1].NickName + " index " + i + " to AdSec Load. Section will be saved without this load.");
                        }
                    }
                    // convert to json with load method
                    jsonString = json.SectionToJson(section.Section, defs);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + Params.Input[1].NickName + " to AdSec Load. Section will be saved without loads.");
                }
            }
            else
            {
                // if no loads are inputted then just convert section
                jsonString = json.SectionToJson(section.Section);
            }

        }
    }
}

