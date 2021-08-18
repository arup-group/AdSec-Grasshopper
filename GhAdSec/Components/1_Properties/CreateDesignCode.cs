using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Grasshopper.Kernel.Attributes;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;
using GhAdSec.Parameters;
using System.Resources;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;

namespace GhAdSec.Components
{
    /// <summary>
    /// Component to create a new Material
    /// </summary>
    public class DesignCode : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("bbad3d3b-f585-474b-8cc6-76fd375819de");
        public DesignCode()
          : base("DesignCode", "DesignCode", "Select an AdSec Design Code",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        //protected override System.Drawing.Bitmap Icon => GhSA.Properties.Resources.CreateMaterial;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                List<string> tempList = new List<string>();
                foreach (string dc in designCodeGroups)
                {
                    if (!dc.StartsWith("IDesignCode"))
                        tempList.Add(dc);
                }
                designCodeGroups = tempList;
                
                if (selecteditems == null)
                {
                    // create a new list of selected items and add the first material type
                    selecteditems = new List<string>();
                    selecteditems.Add(designCodeGroups[4]);
                }
                if (dropdownitems == null)
                {
                    // create a new list of selected items and add the first material type
                    dropdownitems = new List<List<string>>();
                    dropdownitems.Add(designCodeGroups);
                }
                if (dropdownitems.Count == 1)
                {
                    designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.ReflectNamespace("Oasys.AdSec.DesignCode");

                    // create string for selected item to use for type search while drilling
                    string typeString = selecteditems.Last();
                    bool drill = true;
                    while (drill)
                    {
                        // get the type of the most recent selected from level above
                        designCodeKVP.TryGetValue(typeString, out Type typ);
                        
                        // update the KVP by reflecting the type
                        designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.ReflectTypes(typ);
                        
                        // determine if we have reached the fields layer
                        if (designCodeKVP.Count > 1)
                        {
                            // if kvp has >1 values we add them to create a new dropdown list
                            dropdownitems.Add(designCodeKVP.Keys.ToList());
                            // with first item being the selected
                            selecteditems.Add(designCodeKVP.Keys.First());
                            // and set the next search item to this
                            typeString = selecteditems.Last();
                        }
                        else if (designCodeKVP.Count == 1)
                        {
                            // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
                            typeString = designCodeKVP.Keys.First();
                        }
                        else
                        {
                            // if kvp is empty we have reached the field level
                            // where we set the materials by reflecting the type
                            designCodes = GhAdSec.Helpers.ReflectAdSecAPI.ReflectFields(typ);
                            // if kvp has values we add them to create a new dropdown list
                            dropdownitems.Add(designCodes.Keys.ToList());
                            // with first item being the selected
                            selecteditems.Add(designCodes.Keys.First().ToString());
                            // stop drilling
                            drill = false;
                        }
                    }
                }
                first = false;
            }

            m_attributes = new UI.MultiDropDownComponentUI(this, SetSelected, dropdownitems, selecteditems, spacerDescriptions);
        }

        public void SetSelected(int i, int j)
        {
            // change selected item
            selecteditems[i] = dropdownitems[i][j];

            // if selected item is not in the last dropdown then we need to update lists
            if (selecteditems.Count - 1 != i)
            {
                // remove all sub dropdowns and selected items below changed list
                while (dropdownitems.Count > 1)
                {
                    dropdownitems.RemoveAt(1);
                }
                while (selecteditems.Count > i + 1)
                { 
                    selecteditems.RemoveAt(i + 1);
                }
                
                // get list of standard codes for the selected material
                designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.ReflectNamespace("Oasys.AdSec.DesignCode");
                
                //// add codes for selected material to list of dropdowns
                //dropdownitems.Add(designCodeKVP.Keys.ToList());
                //if (selecteditems.Count == 1)
                //    selecteditems.Add(designCodeKVP.Keys.First());

                //if (selecteditems[1].StartsWith("EN1992"))
                //{
                //    spacerDescriptions[1] = "Design Code";
                //    spacerDescriptions[2] = "National Annex";
                //}
                //else
                //{
                //    spacerDescriptions[1] = "Code Group";
                //    spacerDescriptions[2] = "Design Code";
                //}


                // create string for selected item to use for type search while drilling
                int level = 0;
                string typeString = selecteditems[level];
                bool drill = true;
                while (drill)
                {
                    // get the type of the most recent selected from level above
                    designCodeKVP.TryGetValue(typeString, out Type typ);

                    // update the KVP by reflecting the type
                    designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.ReflectTypes(typ);

                    // determine if we have reached the fields layer
                    if (designCodeKVP.Count > 1)
                    {
                        level++;

                        // if kvp has >1 values we add them to create a new dropdown list
                        dropdownitems.Add(designCodeKVP.Keys.ToList());

                        // with first item being the selected
                        if (selecteditems.Count - 1 < level)
                        {
                            selecteditems.Add(designCodeKVP.Keys.First());
                            // and set the next search item to this
                            typeString = selecteditems.Last();
                        }
                        else
                            typeString = selecteditems[level];

                        if (typeString.StartsWith("Edition"))
                            spacerDescriptions[level] = "Edition";
                        if (typeString.StartsWith("Part"))
                            spacerDescriptions[level] = "Part";
                        if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
                            spacerDescriptions[level] = "Unit";
                        if (typeString.StartsWith("National"))
                            spacerDescriptions[level] = "National Annex";
                    }
                    else if (designCodeKVP.Count == 1)
                    {
                        // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
                        typeString = designCodeKVP.Keys.First();
                    }
                    else
                    {
                        // if kvp is empty we have reached the field level
                        // where we set the materials by reflecting the type
                        designCodes = GhAdSec.Helpers.ReflectAdSecAPI.ReflectFields(typ);
                        // if kvp has values we add them to create a new dropdown list
                        dropdownitems.Add(designCodes.Keys.ToList());
                        // with first item being the selected
                        selecteditems.Add(designCodes.Keys.First().ToString());
                        // stop drilling
                        drill = false;

                        typeString = selecteditems.Last();
                        spacerDescriptions[selecteditems.Count - 1] = "Design Code";
                        if (typeString.StartsWith("Edition"))
                            spacerDescriptions[selecteditems.Count - 1] = "Edition";
                        if (typeString.StartsWith("Part"))
                            spacerDescriptions[selecteditems.Count - 1] = "Part";
                        if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
                            spacerDescriptions[selecteditems.Count - 1] = "Unit";
                    }
                }
            }
        }
        #endregion

        #region Input and output
        // get list of material types defined in material parameter
        List<string> designCodeGroups = GhAdSec.Helpers.ReflectAdSecAPI.ReflectNamespace("Oasys.AdSec.DesignCode").Keys.ToList();
        // list of materials
        Dictionary<string, FieldInfo> designCodes;
        FieldInfo selectedCode;
        // list of lists with all dropdown lists conctent
        List<List<string>> dropdownitems;
        Dictionary<string, Type> designCodeKVP;
        // list of selected items
        List<string> selecteditems;
        // list of descriptions 
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Code Group",
            "Part",
            "National Annex",
            "Edition",
            "Another level",
            "Another other",
            "This is so deep"
        });
        private bool first = true;
        #endregion

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("DesignCode", "Code", "AdSec Design Code", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // update selected material
            selectedCode = designCodes[selecteditems.Last()];

            // create new material
            AdSecDesignCode dc = new AdSecDesignCode(selectedCode);

            DA.SetData(0, new GhAdSec.Parameters.AdSecDesignCodeGoo(dc));
        }
        
        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            // to save the dropdownlist content, spacer list and selection list 
            // loop through the lists and save number of lists as well
            writer.SetInt32("dropdownCount", dropdownitems.Count);
            for (int i = 0; i < dropdownitems.Count; i++)
            {
                writer.SetInt32("dropdowncontentsCount" + i, dropdownitems[i].Count);
                for (int j = 0; j < dropdownitems[i].Count; j++)
                    writer.SetString("dropdowncontents" + i + j, dropdownitems[i][j]);
            }
            // spacer list
            writer.SetInt32("spacerCount", spacerDescriptions.Count);
            for (int i = 0; i < spacerDescriptions.Count; i++)
                writer.SetString("spacercontents" + i, spacerDescriptions[i]);
            // selection list
            writer.SetInt32("selectionCount", selecteditems.Count);
            for (int i = 0; i < selecteditems.Count; i++)
                writer.SetString("selectioncontents" + i, selecteditems[i]);
            
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // dropdown content list
            int dropdownCount = reader.GetInt32("dropdownCount");
            dropdownitems = new List<List<string>>();
            for (int i = 0; i < dropdownCount; i++)
            {
                int dropdowncontentsCount = reader.GetInt32("dropdowncontentsCount" + i);
                List<string> tempcontent = new List<string>();
                for (int j = 0; j < dropdowncontentsCount; j++)
                    tempcontent.Add(reader.GetString("dropdowncontents" + i + j));
                dropdownitems.Add(tempcontent);
            }
            // spacer list
            int dropdownspacerCount = reader.GetInt32("spacerCount");
            spacerDescriptions = new List<string>();
            for (int i = 0; i < dropdownspacerCount; i++)
                spacerDescriptions.Add(reader.GetString("spacercontents" + i));
            // selection list
            int selectionsCount = reader.GetInt32("selectionCount");
            selecteditems = new List<string>();
            for (int i = 0; i < selectionsCount; i++)
                selecteditems.Add(reader.GetString("selectioncontents" + i));

            first = false;
            return base.Read(reader);
        }
        #endregion
    }
}