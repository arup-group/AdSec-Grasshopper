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
    /// Component to create a new Standard Material for AdSec
    /// </summary>
    public class StandardMaterial : GH_Component
    {
        #region Name and Ribbon Layout
        // This region handles how the component in displayed on the ribbon
        // including name, exposure level and icon
        public override Guid ComponentGuid => new Guid("42f42580-8ed7-42fb-9cc7-c6f6171a0248");
        public StandardMaterial()
          : base("Standard Material", "Material", "Create a new AdSec Design Code based standard material",
                Ribbon.CategoryName.Name(),
                Ribbon.SubCategoryName.Cat1())
        { this.Hidden = true; } // sets the initial state of the component to hidden
        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => GhAdSec.Properties.Resources.CreateStandardMaterial;
        #endregion

        #region Custom UI
        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            if (first)
            {
                if (selecteditems == null)
                {
                    // create a new list of selected items and add the first material type
                    selecteditems = new List<string>();
                    selecteditems.Add(materialTypes[0]);
                }
                if (dropdownitems == null)
                {
                    // create a new list of selected items and add the first material type
                    dropdownitems = new List<List<string>>();
                    dropdownitems.Add(materialTypes);
                }
                if (dropdownitems.Count == 1)
                {
                    //Enum.TryParse(selecteditems[0], out AdSecMaterial.AdSecMaterialType materialType);
                    designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.StandardCodes(AdSecMaterial.AdSecMaterialType.Concrete);
                    dropdownitems.Add(designCodeKVP.Keys.ToList());
                    // select default code to EN1992
                    selecteditems.Add(designCodeKVP.Keys.ElementAt(4));
                    
                    // create string for selected item to use for type search while drilling
                    string typeString = selecteditems.Last();
                    int level = 1;
                    bool drill = true;
                    while (drill)
                    {
                        // get the type of the most recent selected from level above
                        designCodeKVP.TryGetValue(typeString, out Type typ);
                        
                        // update the KVP by reflecting the type
                        designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);
                        
                        // determine if we have reached the fields layer
                        if (designCodeKVP.Count > 1)
                        {
                            level++;
                            // if kvp has >1 values we add them to create a new dropdown list
                            dropdownitems.Add(designCodeKVP.Keys.ToList());
                            // with first item being the selected
                            if (level == 2)
                                selecteditems.Add(designCodeKVP.Keys.ElementAt(6));
                            else
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
                            materials = GhAdSec.Helpers.ReflectAdSecAPI.ReflectFields(typ);
                            // if kvp has values we add them to create a new dropdown list
                            dropdownitems.Add(materials.Keys.ToList());
                            // with first item being the selected
                            selecteditems.Add(materials.Keys.ElementAt(4));
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
                // remove all sub dropdowns after top level and code level
                while (dropdownitems.Count > 1)
                    dropdownitems.RemoveAt(1);

                string prevSelectedCode = selecteditems[1].ToString();
                
                // remove all selected items after the dropdown that has been changed
                while (selecteditems.Count > i + 1)
                    selecteditems.RemoveAt(i + 1);

                // get the selected material and parse it to type enum
                Enum.TryParse(selecteditems[0], out AdSecMaterial.AdSecMaterialType materialType);
                // get list of standard codes for the selected material
                designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.StandardCodes(materialType);
                // add codes for selected material to list of dropdowns
                dropdownitems.Add(designCodeKVP.Keys.ToList());
                if (selecteditems.Count == 1)
                {
                    if (prevSelectedCode.StartsWith("EN199"))
                    {
                        foreach (string code in dropdownitems[1])
                        {
                            if (code.StartsWith("EN199"))
                            {
                                selecteditems.Add(code);
                                break;
                            }
                        }
                    }
                    else if (dropdownitems[1].Contains(prevSelectedCode))
                        selecteditems.Add(prevSelectedCode);
                    else
                        selecteditems.Add(designCodeKVP.Keys.First());
                }

                // make the UI look more intelligent
                if (selecteditems[1].StartsWith("EN1992"))
                {
                    spacerDescriptions[1] = "Design Code";
                    spacerDescriptions[2] = "National Annex";
                }
                else
                {
                    spacerDescriptions[1] = "Code Group";
                    spacerDescriptions[2] = "Design Code";
                }

                // create string for selected item to use for type search while drilling
                int level = 1;
                string typeString = selecteditems[level];
                bool drill = true;
                while (drill)
                {
                    // get the type of the most recent selected from level above
                    designCodeKVP.TryGetValue(typeString, out Type typ);

                    // update the KVP by reflecting the type
                    designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);

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
                        if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
                            spacerDescriptions[level] = "Unit";
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
                        materials = GhAdSec.Helpers.ReflectAdSecAPI.ReflectFields(typ);
                        // if kvp has values we add them to create a new dropdown list
                        dropdownitems.Add(materials.Keys.ToList());
                        // with first item being the selected
                        if (selecteditems[1].StartsWith("EN1992"))
                        {
                            if (materials.Keys.Count > 4)
                                selecteditems.Add(materials.Keys.ElementAt(4));
                            else if (materials.Keys.Count == 3)
                                selecteditems.Add(materials.Keys.ElementAt(2));
                            else
                                selecteditems.Add(materials.Keys.First());
                        }
                        else
                            selecteditems.Add(materials.Keys.First());
                        // stop drilling
                        drill = false;

                        spacerDescriptions[selecteditems.Count - 1] = "Grade";
                    }
                }
            }
        }
        #endregion

        #region Input and output
        // get list of material types defined in material parameter
        List<string> materialTypes = Enum.GetNames(typeof(AdSecMaterial.AdSecMaterialType)).ToList();
        // list of materials
        Dictionary<string, FieldInfo> materials;
        FieldInfo selectedMaterial;
        // list of lists with all dropdown lists conctent
        List<List<string>> dropdownitems;
        Dictionary<string, Type> designCodeKVP;
        // list of selected items
        List<string> selecteditems;
        // list of descriptions 
        List<string> spacerDescriptions = new List<string>(new string[]
        {
            "Material Type",
            "Design Code",
            "National Annex",
            "Grade",
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
            pManager.AddGenericParameter("Material", "Mat", "AdSec Material", GH_ParamAccess.item);
            
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // update selected material
            selectedMaterial = materials[selecteditems.Last()];

            // create new material
            AdSecMaterial mat = new AdSecMaterial(selectedMaterial);

            DA.SetData(0, new GhAdSec.Parameters.AdSecMaterialGoo(mat));
            
        }

        #region (de)serialization
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            GhAdSec.Helpers.DeSerialization.writeDropDownComponents(ref writer, dropdownitems, selecteditems, spacerDescriptions);

            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            GhAdSec.Helpers.DeSerialization.readDropDownComponents(ref reader, ref dropdownitems, ref selecteditems, ref spacerDescriptions);

            first = false;
            return base.Read(reader);
        }
        #endregion
    }
}