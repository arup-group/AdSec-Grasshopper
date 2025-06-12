using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

using AdSecCore.Constants;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

using Grasshopper.Kernel;

using OasysGH;
using OasysGH.Components;

namespace AdSecGH.Components {
  public class CreateDesignCode : GH_OasysDropDownComponent {
    private const string _excludeKey = "National";
    private const string _designCodeNamespaceToReflect = "Oasys.AdSec.DesignCode";

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("bbad3d3b-f585-474b-8cc6-76fd375819de");
    public override GH_Exposure Exposure => GH_Exposure.septenary | GH_Exposure.obscure;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.CreateDesignCode;
    private readonly IDictionary<string, string> _prefixMappings = new Dictionary<string, string> {
      { "Edition", "Edition" },
      { "Part", "Part" },
      { "Metric", "Unit" },
      { "US", "Unit" },
      { _excludeKey, "National Annex" },
    };
    private Dictionary<string, FieldInfo> _designCodes;

    public CreateDesignCode() : base($"Create{AdSecDesignCodeGoo.Name.Replace(" ", string.Empty)}",
      AdSecDesignCodeGoo.Name.Replace(" ", string.Empty), $"Create a {AdSecDesignCodeGoo.Description}",
      CategoryName.Name(), SubCategoryName.Cat1()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      // change selected item
      _selectedItems[i] = _dropDownItems[i][j];

      // if selected item is not in the last dropdown then we need to update lists
      if (_selectedItems.Count - 1 != i) {
        // remove all sub dropdowns and selected items below changed list
        while (_dropDownItems.Count > 1) {
          _dropDownItems.RemoveAt(1);
        }

        while (_selectedItems.Count > i + 1) {
          _selectedItems.RemoveAt(i + 1);
        }

        // get list of standard codes for the selected material
        var designCodeKVP = ReflectionHelper.ReflectAdSecNamespace(_designCodeNamespaceToReflect);

        // create string for selected item to use for type search while drilling
        int level = 0;
        string typeString = _selectedItems[level];
        bool drill = true;
        while (drill) {
          // get the type of the most recent selected from level above
          designCodeKVP.TryGetValue(typeString, out var typ);

          // update the KVP by reflecting the type
          designCodeKVP = AdSecFileHelper.ReflectNestedTypes(typ);

          // determine if we have reached the fields layer
          if (designCodeKVP.Count > 1) {
            level++;

            // if kvp has >1 values we add them to create a new dropdown list
            _dropDownItems.Add(designCodeKVP.Keys.ToList());

            // with first item being the selected
            if (_selectedItems.Count - 1 < level) {
              _selectedItems.Add(designCodeKVP.Keys.First());
              // and set the next search item to this
              typeString = _selectedItems[_selectedItems.Count - 1];
            } else {
              typeString = _selectedItems[level];
            }

            _spacerDescriptions[level] = GetDescription(typeString);
          } else if (designCodeKVP.Count == 1) {
            // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
            typeString = designCodeKVP.Keys.First();
          } else {
            // if kvp is empty we have reached the field level
            // where we set the materials by reflecting the type
            _designCodes = ReflectionHelper.ReflectFields(typ);
            // if kvp has values we add them to create a new dropdown list
            _dropDownItems.Add(_designCodes.Keys.ToList());
            // with first item being the selected
            _selectedItems.Add(_designCodes.Keys.First());
            // stop drilling
            drill = false;

            _spacerDescriptions[_selectedItems.Count - 1] = GetDescription(typeString, _excludeKey);
          }
        }
      }

      base.UpdateUI();
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Code Group",
        "Part",
        "National Annex",
        "Edition",
        "Another level",
        "Another other",
        "This is so deep",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      var designCodeGroups = ReflectionHelper.ReflectAdSecNamespace(_designCodeNamespaceToReflect).Keys.ToList();

      var tempList = designCodeGroups.Where(dc => !dc.StartsWith("IDesignCode")).ToList();
      designCodeGroups = tempList;

      _selectedItems.Add(designCodeGroups[4]);
      _dropDownItems.Add(designCodeGroups);

      if (_dropDownItems.Count == 1) {
        var designCodeKVP = ReflectionHelper.ReflectAdSecNamespace(_designCodeNamespaceToReflect);

        // create string for selected item to use for type search while drilling
        string typeString = _selectedItems[_selectedItems.Count - 1];
        bool drill = true;
        while (drill) {
          // get the type of the most recent selected from level above
          designCodeKVP.TryGetValue(typeString, out var typ);

          // update the KVP by reflecting the type
          designCodeKVP = AdSecFileHelper.ReflectNestedTypes(typ);

          // determine if we have reached the fields layer
          if (designCodeKVP.Count > 1) {
            // if kvp has >1 values we add them to create a new dropdown list
            _dropDownItems.Add(designCodeKVP.Keys.ToList());
            // with first item being the selected
            _selectedItems.Add(designCodeKVP.Keys.First());
            // and set the next search item to this
            typeString = _selectedItems[_selectedItems.Count - 1];
          } else if (designCodeKVP.Count == 1) {
            // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
            typeString = designCodeKVP.Keys.First();
          } else {
            // if kvp is empty we have reached the field level
            // where we set the materials by reflecting the type
            _designCodes = ReflectionHelper.ReflectFields(typ);
            // if kvp has values we add them to create a new dropdown list
            _dropDownItems.Add(_designCodes.Keys.ToList());
            // with first item being the selected
            _selectedItems.Add(_designCodes.Keys.First());
            // stop drilling
            drill = false;
          }
        }
      }

      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) { }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("DesignCode", "Code", "AdSec Design Code", GH_ParamAccess.item);
    }

    protected override void SolveInternal(IGH_DataAccess DA) {
      // update selected material
      var selectedCode = _designCodes[_selectedItems[_selectedItems.Count - 1]];

      // create new material
      var dc = new AdSecDesignCode(selectedCode);

      DA.SetData(0, new AdSecDesignCodeGoo(dc));
    }

    protected override void UpdateUIFromSelectedItems() {
      // get list of standard codes for the selected material
      var designCodeKVP = ReflectionHelper.ReflectAdSecNamespace(_designCodeNamespaceToReflect);

      // create string for selected item to use for type search while drilling
      int level = 0;
      string typeString = _selectedItems[level];
      bool drill = true;
      while (drill) {
        // get the type of the most recent selected from level above
        designCodeKVP.TryGetValue(typeString, out var typ);

        // update the KVP by reflecting the type
        designCodeKVP = AdSecFileHelper.ReflectNestedTypes(typ);

        // determine if we have reached the fields layer
        if (designCodeKVP.Count > 1) {
          level++;
          typeString = _selectedItems[level];
          _spacerDescriptions[level] = GetDescription(typeString);
        } else if (designCodeKVP.Count == 1) {
          // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
          typeString = designCodeKVP.Keys.First();
        } else {
          // if kvp is empty we have reached the field level
          // where we set the materials by reflecting the type
          _designCodes = ReflectionHelper.ReflectFields(typ);
          // stop drilling
          drill = false;

          _spacerDescriptions[_selectedItems.Count - 1] = GetDescription(typeString, _excludeKey);
        }
      }

      base.UpdateUIFromSelectedItems();
    }

    private string GetDescription(string typeString, string excludedKey = "") {
      string result = _prefixMappings.Where(mapping => typeString.StartsWith(mapping.Key) && mapping.Key != excludedKey)
       .Select(mapping => mapping.Value).FirstOrDefault();

      return string.IsNullOrEmpty(result) ? "Design Code" : result;
    }
  }
}
