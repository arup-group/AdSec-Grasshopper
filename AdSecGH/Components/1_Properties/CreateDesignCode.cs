using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using OasysGH;
using OasysGH.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdSecGH.Components {
  public class CreateDesignCode : GH_OasysDropDownComponent {
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
      SubCategoryName.Cat1()) { this.Hidden = true; } // sets the initial state of the component to hidden

    public override void SetSelected(int i, int j) {
      // change selected item
      this._selectedItems[i] = this._dropDownItems[i][j];

      // if selected item is not in the last dropdown then we need to update lists
      if (this._selectedItems.Count - 1 != i) {
        // remove all sub dropdowns and selected items below changed list
        while (this._dropDownItems.Count > 1) {
          this._dropDownItems.RemoveAt(1);
        }
        while (this._selectedItems.Count > i + 1) {
          this._selectedItems.RemoveAt(i + 1);
        }

        // get list of standard codes for the selected material
        Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");

        // create string for selected item to use for type search while drilling
        int level = 0;
        string typeString = this._selectedItems[level];
        bool drill = true;
        while (drill) {
          // get the type of the most recent selected from level above
          designCodeKVP.TryGetValue(typeString, out Type typ);

          // update the KVP by reflecting the type
          designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);

          // determine if we have reached the fields layer
          if (designCodeKVP.Count > 1) {
            level++;

            // if kvp has >1 values we add them to create a new dropdown list
            this._dropDownItems.Add(designCodeKVP.Keys.ToList());

            // with first item being the selected
            if (this._selectedItems.Count - 1 < level) {
              this._selectedItems.Add(designCodeKVP.Keys.First());
              // and set the next search item to this
              typeString = this._selectedItems.Last();
            }
            else
              typeString = this._selectedItems[level];

            if (typeString.StartsWith("Edition"))
              this._spacerDescriptions[level] = "Edition";
            if (typeString.StartsWith("Part"))
              this._spacerDescriptions[level] = "Part";
            if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
              this._spacerDescriptions[level] = "Unit";
            if (typeString.StartsWith("National"))
              this._spacerDescriptions[level] = "National Annex";
          }
          else if (designCodeKVP.Count == 1) {
            // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
            typeString = designCodeKVP.Keys.First();
          }
          else {
            // if kvp is empty we have reached the field level
            // where we set the materials by reflecting the type
            this._designCodes = Helpers.ReflectAdSecAPI.ReflectFields(typ);
            // if kvp has values we add them to create a new dropdown list
            this._dropDownItems.Add(this._designCodes.Keys.ToList());
            // with first item being the selected
            this._selectedItems.Add(this._designCodes.Keys.First().ToString());
            // stop drilling
            drill = false;

            this._spacerDescriptions[this._selectedItems.Count - 1] = "Design Code";
            if (typeString.StartsWith("Edition"))
              this._spacerDescriptions[this._selectedItems.Count - 1] = "Edition";
            if (typeString.StartsWith("Part"))
              this._spacerDescriptions[this._selectedItems.Count - 1] = "Part";
            if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
              this._spacerDescriptions[this._selectedItems.Count - 1] = "Unit";
          }
        }
      }
      base.UpdateUI();
    }

    protected override void InitialiseDropdowns() {
      this._spacerDescriptions = new List<string>(new string[] {
        "Code Group",
        "Part",
        "National Annex",
        "Edition",
        "Another level",
        "Another other",
        "This is so deep"
      });

      this._dropDownItems = new List<List<string>>();
      this._selectedItems = new List<string>();

      List<string> designCodeGroups = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode").Keys.ToList();

      List<string> tempList = new List<string>();
      foreach (string dc in designCodeGroups) {
        if (!dc.StartsWith("IDesignCode"))
          tempList.Add(dc);
      }
      designCodeGroups = tempList;

      this._selectedItems.Add(designCodeGroups[4]);
      this._dropDownItems.Add(designCodeGroups);

      if (this._dropDownItems.Count == 1) {
        Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");

        // create string for selected item to use for type search while drilling
        string typeString = this._selectedItems.Last();
        bool drill = true;
        while (drill) {
          // get the type of the most recent selected from level above
          designCodeKVP.TryGetValue(typeString, out Type typ);

          // update the KVP by reflecting the type
          designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);

          // determine if we have reached the fields layer
          if (designCodeKVP.Count > 1) {
            // if kvp has >1 values we add them to create a new dropdown list
            this._dropDownItems.Add(designCodeKVP.Keys.ToList());
            // with first item being the selected
            this._selectedItems.Add(designCodeKVP.Keys.First());
            // and set the next search item to this
            typeString = this._selectedItems.Last();
          }
          else if (designCodeKVP.Count == 1) {
            // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
            typeString = designCodeKVP.Keys.First();
          }
          else {
            // if kvp is empty we have reached the field level
            // where we set the materials by reflecting the type
            this._designCodes = Helpers.ReflectAdSecAPI.ReflectFields(typ);
            // if kvp has values we add them to create a new dropdown list
            this._dropDownItems.Add(this._designCodes.Keys.ToList());
            // with first item being the selected
            this._selectedItems.Add(this._designCodes.Keys.First().ToString());
            // stop drilling
            drill = false;
          }
        }
      }
      this._isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("DesignCode", "Code", "AdSec Design Code", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      // update selected material
      FieldInfo selectedCode = this._designCodes[this._selectedItems.Last()];

      // create new material
      AdSecDesignCode dc = new AdSecDesignCode(selectedCode);

      DA.SetData(0, new AdSecDesignCodeGoo(dc));
    }

    protected override void UpdateUIFromSelectedItems() {
      // get list of standard codes for the selected material
      Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");

      // create string for selected item to use for type search while drilling
      int level = 0;
      string typeString = this._selectedItems[level];
      bool drill = true;
      while (drill) {
        // get the type of the most recent selected from level above
        designCodeKVP.TryGetValue(typeString, out Type typ);

        // update the KVP by reflecting the type
        designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);

        // determine if we have reached the fields layer
        if (designCodeKVP.Count > 1) {
          level++;

          typeString = this._selectedItems[level];

          if (typeString.StartsWith("Edition"))
            this._spacerDescriptions[level] = "Edition";
          if (typeString.StartsWith("Part"))
            this._spacerDescriptions[level] = "Part";
          if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
            this._spacerDescriptions[level] = "Unit";
          if (typeString.StartsWith("National"))
            this._spacerDescriptions[level] = "National Annex";
        }
        else if (designCodeKVP.Count == 1) {
          // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
          typeString = designCodeKVP.Keys.First();
        }
        else {
          // if kvp is empty we have reached the field level
          // where we set the materials by reflecting the type
          this._designCodes = Helpers.ReflectAdSecAPI.ReflectFields(typ);
          // stop drilling
          drill = false;

          this._spacerDescriptions[this._selectedItems.Count - 1] = "Design Code";
          if (typeString.StartsWith("Edition"))
            this._spacerDescriptions[this._selectedItems.Count - 1] = "Edition";
          if (typeString.StartsWith("Part"))
            this._spacerDescriptions[this._selectedItems.Count - 1] = "Part";
          if (typeString.StartsWith("Metric") | typeString.StartsWith("US"))
            this._spacerDescriptions[this._selectedItems.Count - 1] = "Unit";
        }
      }
      this.UpdateUIFromSelectedItems();
    }
  }
}
