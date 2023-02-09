using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using OasysGH;
using OasysGH.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AdSecGH.Components
{
  public class CreateStandardMaterial : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("42f42580-8ed7-42fb-9cc7-c6f6171a0248");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.StandardMaterial;

    public CreateStandardMaterial() : base(
      "Standard Material",
      "Material",
      "Create a new AdSec Design Code based standard material",
      Ribbon.CategoryName.Name(),
      Ribbon.SubCategoryName.Cat1())
    {
      this.Hidden = true; // sets the initial state of the component to hidden
    }
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
      pManager.AddTextParameter("Search", "S", "[Optional] Search for Grade " +
              System.Environment.NewLine + "Note: input 'all' to list all grades from the selected code", GH_ParamAccess.item);
      pManager[0].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("Material", "Mat", "AdSec Material", GH_ParamAccess.list);
    }
    #endregion 

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      string search = "";
      if (DA.GetData(0, ref search))
      {
        search = search.ToLower();
        // filter by search pattern
        if (search != "")
        {
          List<string> materialsList = materials.Keys.ToList();
          List<AdSecMaterialGoo> filteredMaterials = new List<AdSecMaterialGoo>();

          for (int i = 0; i < materialsList.Count; i++)
          {
            if (search.ToLower() == "all")
            {
              filteredMaterials.Add(new AdSecMaterialGoo(new AdSecMaterial(materials[materialsList[i]])));
              this.SelectedItems[this.SelectedItems.Count - 1] = "all";
            }
            else
            {
              if (materialsList[i].ToLower().Contains(search))
              {
                filteredMaterials.Add(new AdSecMaterialGoo(new AdSecMaterial(materials[materialsList[i]])));
                this.SelectedItems[this.SelectedItems.Count - 1] = materialsList[i];
              }
              if (!search.Any(char.IsDigit))
              {
                string test = materialsList[i].ToString();
                test = Regex.Replace(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(search))
                {
                  filteredMaterials.Add(new AdSecMaterialGoo(new AdSecMaterial(materials[materialsList[i]])));
                  this.SelectedItems[this.SelectedItems.Count - 1] = materialsList[i];
                }
              }
            }
          }

          DA.SetDataList(0, filteredMaterials);
          return;
        }
      }

      // update selected material
      selectedMaterial = materials[this.SelectedItems.Last()];

      // create new material
      AdSecMaterial mat = new AdSecMaterial(selectedMaterial);

      DA.SetData(0, new AdSecMaterialGoo(mat));
    }

    #region Custom UI
    // get list of material types defined in material parameter
    List<string> materialTypes = Enum.GetNames(typeof(AdSecMaterial.AdSecMaterialType)).ToList();
    // list of materials
    Dictionary<string, FieldInfo> materials;
    FieldInfo selectedMaterial;
    Dictionary<string, Type> designCodeKVP;

    public override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Material Type",
        "Design Code",
        "National Annex",
        "Edition",
        "Grade",
        "Another other",
        "This is so deep"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      if (this.SelectedItems == null)
      {
        // create a new list of selected items and add the first material type
        this.SelectedItems = new List<string>();
        this.SelectedItems.Add(materialTypes[0]);
      }
      if (this.DropDownItems == null)
      {
        // create a new list of selected items and add the first material type
        this.DropDownItems = new List<List<string>>();
        this.DropDownItems.Add(materialTypes);
      }
      if (this.DropDownItems.Count == 1)
      {
        //Enum.TryParse(this.SelectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
        designCodeKVP = Helpers.ReflectAdSecAPI.StandardCodes(AdSecMaterial.AdSecMaterialType.Concrete);
        this.DropDownItems.Add(designCodeKVP.Keys.ToList());
        // select default code to EN1992
        this.SelectedItems.Add(designCodeKVP.Keys.ElementAt(4));

        // create string for selected item to use for type search while drilling
        string typeString = this.SelectedItems.Last();
        int level = 1;
        bool drill = true;
        while (drill)
        {
          // get the type of the most recent selected from level above
          designCodeKVP.TryGetValue(typeString, out Type typ);

          // update the KVP by reflecting the type
          designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);

          // determine if we have reached the fields layer
          if (designCodeKVP.Count > 1)
          {
            level++;
            // if kvp has >1 values we add them to create a new dropdown list
            this.DropDownItems.Add(designCodeKVP.Keys.ToList());
            // with first item being the selected
            if (level == 2)
              this.SelectedItems.Add(designCodeKVP.Keys.ElementAt(6));
            else
              this.SelectedItems.Add(designCodeKVP.Keys.First());
            // and set the next search item to this
            typeString = this.SelectedItems.Last();
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
            materials = Helpers.ReflectAdSecAPI.ReflectFields(typ);
            // if kvp has values we add them to create a new dropdown list
            this.DropDownItems.Add(materials.Keys.ToList());
            // with first item being the selected
            this.SelectedItems.Add(materials.Keys.ElementAt(4));
            // stop drilling
            drill = false;
          }
        }
      }
      this.IsInitialised = true;
    }

    public override void SetSelected(int i, int j)
    {
      // change selected item
      this.SelectedItems[i] = this.DropDownItems[i][j];

      // if selected item is not in the last dropdown then we need to update lists
      if (this.SelectedItems.Count - 1 != i)
      {
        // remove all sub dropdowns after top level and code level
        while (this.DropDownItems.Count > 1)
          this.DropDownItems.RemoveAt(1);

        string prevSelectedCode = this.SelectedItems[1].ToString();
        string prevSelectedNA = this.SelectedItems[2].ToString();

        // remove all selected items after the dropdown that has been changed
        while (this.SelectedItems.Count > i + 1)
          this.SelectedItems.RemoveAt(i + 1);

        // get the selected material and parse it to type enum
        Enum.TryParse(this.SelectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
        // get list of standard codes for the selected material
        designCodeKVP = Helpers.ReflectAdSecAPI.StandardCodes(materialType);
        // add codes for selected material to list of dropdowns
        this.DropDownItems.Add(designCodeKVP.Keys.ToList());
        if (this.SelectedItems.Count == 1)
        {
          if (prevSelectedCode.StartsWith("EN199"))
          {
            foreach (string code in this.DropDownItems[1])
            {
              if (code.StartsWith("EN199"))
              {
                this.SelectedItems.Add(code);
                break;
              }
            }
          }
          else if (this.DropDownItems[1].Contains(prevSelectedCode))
            this.SelectedItems.Add(prevSelectedCode);
          else
            this.SelectedItems.Add(designCodeKVP.Keys.First());
        }

        // make the UI look more intelligent
        if (this.SelectedItems[1].StartsWith("EN1992"))
        {
          this.SpacerDescriptions[1] = "Design Code";
          this.SpacerDescriptions[2] = "National Annex";
        }
        else
        {
          this.SpacerDescriptions[1] = "Code Group";
          this.SpacerDescriptions[2] = "Design Code";
        }

        // create string for selected item to use for type search while drilling
        int level = 1;
        string typeString = this.SelectedItems[level];
        bool drill = true;
        while (drill)
        {
          // get the type of the most recent selected from level above
          designCodeKVP.TryGetValue(typeString, out Type typ);

          // update the KVP by reflecting the type
          designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);

          // determine if we have reached the fields layer
          if (designCodeKVP.Count > 1)
          {
            level++;

            // if kvp has >1 values we add them to create a new dropdown list
            this.DropDownItems.Add(designCodeKVP.Keys.ToList());

            // with first item being the selected
            if (this.SelectedItems.Count - 1 < level)
            {
              if (level == 2)
              {
                if (prevSelectedCode.StartsWith("EN1992"))
                {
                  foreach (string code in this.DropDownItems[2])
                  {
                    if (code.Equals(prevSelectedNA))
                    {
                      this.SelectedItems.Add(code);
                      typeString = this.SelectedItems.Last();
                      break;
                    }
                  }
                }
                else
                  this.SelectedItems.Add(designCodeKVP.Keys.First());

              }
              else
                this.SelectedItems.Add(designCodeKVP.Keys.First());

              // and set the next search item to this
              typeString = this.SelectedItems.Last();
            }
            else
              typeString = this.SelectedItems[level];

            if (typeString.StartsWith("Edition"))
              this.SpacerDescriptions[level] = "Edition";
            if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
              this.SpacerDescriptions[level] = "Unit";
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
            materials = Helpers.ReflectAdSecAPI.ReflectFields(typ);
            if (materials.Count == 0)
            {
              this.DropDownItems.Add(new List<string>() { "No material" });
              this.SelectedItems.Add("No material");
            }
            else
            {
              // if kvp has values we add them to create a new dropdown list
              this.DropDownItems.Add(materials.Keys.ToList());
              // with first item being the selected
              if (this.SelectedItems[1].StartsWith("EN1992"))
              {
                if (materials.Keys.Count > 4)
                {
                  this.SelectedItems.Add(materials.Keys.ElementAt(4)); // C37

                }
                else if (materials.Keys.Count == 3)
                  this.SelectedItems.Add(materials.Keys.ElementAt(1)); // B500B

                else
                  this.SelectedItems.Add(materials.Keys.First());
              }
              else if (this.SelectedItems[1].StartsWith("EN1993"))
              {
                this.SelectedItems.Add(materials.Keys.ElementAt(2)); // S355
              }
              else
                this.SelectedItems.Add(materials.Keys.First());
            }

            // stop drilling
            drill = false;

            this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Grade";
          }
        }
      }
      base.UpdateUI();
    }

    public override void UpdateUIFromSelectedItems()
    {
      // get the selected material and parse it to type enum
      Enum.TryParse(this.SelectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
      // get list of standard codes for the selected material
      designCodeKVP = Helpers.ReflectAdSecAPI.StandardCodes(materialType);
      // add codes for selected material to list of dropdowns
      //this.DropDownItems.Add(designCodeKVP.Keys.ToList());

      // make the UI look more intelligent
      if (this.SelectedItems[1].StartsWith("EN1992"))
      {
        this.SpacerDescriptions[1] = "Design Code";
        this.SpacerDescriptions[2] = "National Annex";
      }
      else
      {
        this.SpacerDescriptions[1] = "Code Group";
        this.SpacerDescriptions[2] = "Design Code";
      }

      // create string for selected item to use for type search while drilling
      int level = 1;
      string typeString = this.SelectedItems[level];
      bool drill = true;
      while (drill)
      {
        // get the type of the most recent selected from level above
        designCodeKVP.TryGetValue(typeString, out Type typ);

        // update the KVP by reflecting the type
        designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);

        // determine if we have reached the fields layer
        if (designCodeKVP.Count > 1)
        {
          level++;
          typeString = this.SelectedItems[level];

          if (typeString.StartsWith("Edition"))
            this.SpacerDescriptions[level] = "Edition";
          if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
            this.SpacerDescriptions[level] = "Unit";
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
          materials = Helpers.ReflectAdSecAPI.ReflectFields(typ);

          drill = false;

          this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Grade";
        }
      }
      base.UpdateUIFromSelectedItems();
    }
    #endregion

    public override void VariableParameterMaintenance()
    {
      if (Params.Input.Count == 0)
      {
        Params.RegisterInputParam(new Param_String());
        Params.Input[0].NickName = "S";
        Params.Input[0].Name = "Search";
        Params.Input[0].Description = "[Optional] Search for Grade " + System.Environment.NewLine + "Note: input 'all' to list all grades from the selected code";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = true;
      }
    }
  }
}
