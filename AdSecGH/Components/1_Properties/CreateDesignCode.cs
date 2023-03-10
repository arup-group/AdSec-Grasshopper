using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH;
using OasysGH.Components;
using AdSecGH.Helpers.GH;

namespace AdSecGH.Components
{
  public class CreateDesignCode : GH_OasysDropDownComponent
  {
    #region Name and Ribbon Layout
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("bbad3d3b-f585-474b-8cc6-76fd375819de");
    public override GH_Exposure Exposure => GH_Exposure.septenary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CreateDesignCode;
    private Dictionary<string, FieldInfo> _designCodes;

    public CreateDesignCode() : base(
      "Create" + AdSecDesignCodeGoo.Name.Replace(" ", string.Empty),
      AdSecDesignCodeGoo.Name.Replace(" ", string.Empty),
      "Create a " + AdSecDesignCodeGoo.Description,
      CategoryName.Name(),
      SubCategoryName.Cat1())
    { this.Hidden = true; } // sets the initial state of the component to hidden
    #endregion

    #region Input and output
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
      pManager.AddGenericParameter("DesignCode", "Code", "AdSec Design Code", GH_ParamAccess.item);
    }
    #endregion

    protected override void SolveInstance(IGH_DataAccess DA)
    {
      // update selected material
      FieldInfo selectedCode = this._designCodes[this.SelectedItems.Last()];

      // create new material
      AdSecDesignCode dc = new AdSecDesignCode(selectedCode);

      DA.SetData(0, new AdSecDesignCodeGoo(dc));
    }

    #region Custom UI
    protected override void InitialiseDropdowns()
    {
      this.SpacerDescriptions = new List<string>(new string[] {
        "Code Group",
        "Part",
        "National Annex",
        "Edition",
        "Another level",
        "Another other",
        "This is so deep"
      });

      this.DropDownItems = new List<List<string>>();
      this.SelectedItems = new List<string>();

      List<string> designCodeGroups = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode").Keys.ToList();

      List<string> tempList = new List<string>();
      foreach (string dc in designCodeGroups)
      {
        if (!dc.StartsWith("IDesignCode"))
          tempList.Add(dc);
      }
      designCodeGroups = tempList;

      this.SelectedItems.Add(designCodeGroups[4]);
      this.DropDownItems.Add(designCodeGroups);

      if (this.DropDownItems.Count == 1)
      {
        Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");

        // create string for selected item to use for type search while drilling
        string typeString = this.SelectedItems.Last();
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
            // if kvp has >1 values we add them to create a new dropdown list
            this.DropDownItems.Add(designCodeKVP.Keys.ToList());
            // with first item being the selected
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
            this._designCodes = Helpers.ReflectAdSecAPI.ReflectFields(typ);
            // if kvp has values we add them to create a new dropdown list
            this.DropDownItems.Add(this._designCodes.Keys.ToList());
            // with first item being the selected
            this.SelectedItems.Add(this._designCodes.Keys.First().ToString());
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
        // remove all sub dropdowns and selected items below changed list
        while (this.DropDownItems.Count > 1)
        {
          this.DropDownItems.RemoveAt(1);
        }
        while (this.SelectedItems.Count > i + 1)
        {
          this.SelectedItems.RemoveAt(i + 1);
        }

        // get list of standard codes for the selected material
        Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");

        // create string for selected item to use for type search while drilling
        int level = 0;
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
              this.SelectedItems.Add(designCodeKVP.Keys.First());
              // and set the next search item to this
              typeString = this.SelectedItems.Last();
            }
            else
              typeString = this.SelectedItems[level];

            if (typeString.StartsWith("Edition"))
              this.SpacerDescriptions[level] = "Edition";
            if (typeString.StartsWith("Part"))
              this.SpacerDescriptions[level] = "Part";
            if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
              this.SpacerDescriptions[level] = "Unit";
            if (typeString.StartsWith("National"))
              this.SpacerDescriptions[level] = "National Annex";
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
            this._designCodes = Helpers.ReflectAdSecAPI.ReflectFields(typ);
            // if kvp has values we add them to create a new dropdown list
            this.DropDownItems.Add(this._designCodes.Keys.ToList());
            // with first item being the selected
            this.SelectedItems.Add(this._designCodes.Keys.First().ToString());
            // stop drilling
            drill = false;

            this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Design Code";
            if (typeString.StartsWith("Edition"))
              this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Edition";
            if (typeString.StartsWith("Part"))
              this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Part";
            if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
              this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Unit";
          }
        }
      }
      base.UpdateUI();
    }

    protected override void UpdateUIFromSelectedItems()
    {
      // get list of standard codes for the selected material
      Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");

      // create string for selected item to use for type search while drilling
      int level = 0;
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
          if (typeString.StartsWith("Part"))
            this.SpacerDescriptions[level] = "Part";
          if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
            this.SpacerDescriptions[level] = "Unit";
          if (typeString.StartsWith("National"))
            this.SpacerDescriptions[level] = "National Annex";
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
          this._designCodes = Helpers.ReflectAdSecAPI.ReflectFields(typ);
          // stop drilling
          drill = false;

          this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Design Code";
          if (typeString.StartsWith("Edition"))
            this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Edition";
          if (typeString.StartsWith("Part"))
            this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Part";
          if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
            this.SpacerDescriptions[this.SelectedItems.Count - 1] = "Unit";
        }
      }
      this.UpdateUIFromSelectedItems();
    }
    #endregion
  }
}
