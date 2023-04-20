using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AdSecGH.Helpers.GH;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using OasysGH;
using OasysGH.Components;

namespace AdSecGH.Components {
  public class CreateStandardMaterial : GH_OasysDropDownComponent {
    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("42f42580-8ed7-42fb-9cc7-c6f6171a0248");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.StandardMaterial;
    private Dictionary<string, FieldInfo> _materials;

    public CreateStandardMaterial() : base(
      "Standard Material",
      "Material",
      "Create a new AdSec Design Code based standard material",
      CategoryName.Name(),
      SubCategoryName.Cat1()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      // change selected item
      _selectedItems[i] = _dropDownItems[i][j];

      // if selected item is not in the last dropdown then we need to update lists
      if (_selectedItems.Count - 1 != i) {
        // remove all sub dropdowns after top level and code level
        while (_dropDownItems.Count > 1) {
          _dropDownItems.RemoveAt(1);
        }

        string prevSelectedCode = _selectedItems[1].ToString();
        string prevSelectedNA = _selectedItems[2].ToString();

        // remove all selected items after the dropdown that has been changed
        while (_selectedItems.Count > i + 1) {
          _selectedItems.RemoveAt(i + 1);
        }

        // get the selected material and parse it to type enum
        Enum.TryParse(_selectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
        // get list of standard codes for the selected material
        Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.StandardCodes(materialType);
        // add codes for selected material to list of dropdowns
        _dropDownItems.Add(designCodeKVP.Keys.ToList());
        if (_selectedItems.Count == 1) {
          if (prevSelectedCode.StartsWith("EN199")) {
            foreach (string code in _dropDownItems[1]) {
              if (code.StartsWith("EN199")) {
                _selectedItems.Add(code);
                break;
              }
            }
          } else if (_dropDownItems[1].Contains(prevSelectedCode)) {
            _selectedItems.Add(prevSelectedCode);
          } else {
            _selectedItems.Add(designCodeKVP.Keys.First());
          }
        }

        // make the UI look more intelligent
        if (_selectedItems[1].StartsWith("EN1992")) {
          _spacerDescriptions[1] = "Design Code";
          _spacerDescriptions[2] = "National Annex";
        } else {
          _spacerDescriptions[1] = "Code Group";
          _spacerDescriptions[2] = "Design Code";
        }

        // create string for selected item to use for type search while drilling
        int level = 1;
        string typeString = _selectedItems[level];
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
            _dropDownItems.Add(designCodeKVP.Keys.ToList());

            // with first item being the selected
            if (_selectedItems.Count - 1 < level) {
              if (level == 2) {
                if (prevSelectedCode.StartsWith("EN1992")) {
                  foreach (string code in _dropDownItems[2]) {
                    if (code.Equals(prevSelectedNA)) {
                      _selectedItems.Add(code);
                      typeString = _selectedItems.Last();
                      break;
                    }
                  }
                } else {
                  _selectedItems.Add(designCodeKVP.Keys.First());
                }
              } else {
                _selectedItems.Add(designCodeKVP.Keys.First());
              }

              // and set the next search item to this
              typeString = _selectedItems.Last();
            } else {
              typeString = _selectedItems[level];
            }

            if (typeString.StartsWith("Edition")) {
              _spacerDescriptions[level] = "Edition";
            }
            if (typeString.StartsWith("Metric") | typeString.StartsWith("US")) {
              _spacerDescriptions[level] = "Unit";
            }
          } else if (designCodeKVP.Count == 1) {
            // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
            typeString = designCodeKVP.Keys.First();
          } else {
            // if kvp is empty we have reached the field level
            // where we set the materials by reflecting the type
            _materials = Helpers.ReflectAdSecAPI.ReflectFields(typ);
            if (_materials.Count == 0) {
              _dropDownItems.Add(new List<string>() { "No material" });
              _selectedItems.Add("No material");
            } else {
              // if kvp has values we add them to create a new dropdown list
              _dropDownItems.Add(_materials.Keys.ToList());
              // with first item being the selected
              if (_selectedItems[1].StartsWith("EN1992")) {
                if (_materials.Keys.Count > 4) {
                  _selectedItems.Add(_materials.Keys.ElementAt(4)); // C37
                } else if (_materials.Keys.Count == 3) {
                  _selectedItems.Add(_materials.Keys.ElementAt(1)); // B500B
                } else {
                  _selectedItems.Add(_materials.Keys.First());
                }
              } else if (_selectedItems[1].StartsWith("EN1993")) {
                _selectedItems.Add(_materials.Keys.ElementAt(2)); // S355
              } else {
                _selectedItems.Add(_materials.Keys.First());
              }
            }

            // stop drilling
            drill = false;

            _spacerDescriptions[_selectedItems.Count - 1] = "Grade";
          }
        }
      }
      base.UpdateUI();
    }

    public override void VariableParameterMaintenance() {
      if (Params.Input.Count == 0) {
        Params.RegisterInputParam(new Param_String());
        Params.Input[0].NickName = "S";
        Params.Input[0].Name = "Search";
        Params.Input[0].Description = "[Optional] Search for Grade " + System.Environment.NewLine + "Note: input 'all' to list all grades from the selected code";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = true;
      }
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new string[] {
        "Material Type",
        "Design Code",
        "National Annex",
        "Edition",
        "Grade",
        "Another other",
        "This is so deep"
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      // get list of material types defined in material parameter
      var materialTypes = Enum.GetNames(typeof(AdSecMaterial.AdSecMaterialType)).ToList();

      _dropDownItems.Add(materialTypes);
      _selectedItems.Add(materialTypes[0]);

      if (_dropDownItems.Count == 1) {
        //Enum.TryParse(_selectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
        Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.StandardCodes(AdSecMaterial.AdSecMaterialType.Concrete);
        _dropDownItems.Add(designCodeKVP.Keys.ToList());
        // select default code to EN1992
        _selectedItems.Add(designCodeKVP.Keys.ElementAt(4));

        // create string for selected item to use for type search while drilling
        string typeString = _selectedItems.Last();
        int level = 1;
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
            _dropDownItems.Add(designCodeKVP.Keys.ToList());
            // with first item being the selected
            if (level == 2) {
              _selectedItems.Add(designCodeKVP.Keys.ElementAt(6));
            } else {
              _selectedItems.Add(designCodeKVP.Keys.First());
            }
            // and set the next search item to this
            typeString = _selectedItems.Last();
          } else if (designCodeKVP.Count == 1) {
            // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
            typeString = designCodeKVP.Keys.First();
          } else {
            // if kvp is empty we have reached the field level
            // where we set the materials by reflecting the type
            _materials = Helpers.ReflectAdSecAPI.ReflectFields(typ);
            // if kvp has values we add them to create a new dropdown list
            _dropDownItems.Add(_materials.Keys.ToList());
            // with first item being the selected
            _selectedItems.Add(_materials.Keys.ElementAt(4));
            // stop drilling
            drill = false;
          }
        }
      }
      _isInitialised = true;
    }

    protected override void RegisterInputParams(GH_InputParamManager pManager) {
      pManager.AddTextParameter("Search", "S", "[Optional] Search for Grade " +
              System.Environment.NewLine + "Note: input 'all' to list all grades from the selected code", GH_ParamAccess.item);
      pManager[0].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Material", "Mat", "AdSec Material", GH_ParamAccess.list);
    }

    protected override void SolveInstance(IGH_DataAccess DA) {
      string search = "";
      if (DA.GetData(0, ref search)) {
        search = search.ToLower();
        // filter by search pattern
        if (search != "") {
          var materialsList = _materials.Keys.ToList();
          var filteredMaterials = new List<AdSecMaterialGoo>();

          for (int i = 0; i < materialsList.Count; i++) {
            if (search.ToLower() == "all") {
              filteredMaterials.Add(new AdSecMaterialGoo(new AdSecMaterial(_materials[materialsList[i]])));
              _selectedItems[_selectedItems.Count - 1] = "all";
            } else {
              if (materialsList[i].ToLower().Contains(search)) {
                filteredMaterials.Add(new AdSecMaterialGoo(new AdSecMaterial(_materials[materialsList[i]])));
                _selectedItems[_selectedItems.Count - 1] = materialsList[i];
              }
              if (!search.Any(char.IsDigit)) {
                string test = materialsList[i].ToString();
                test = Regex.Replace(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(search)) {
                  filteredMaterials.Add(new AdSecMaterialGoo(new AdSecMaterial(_materials[materialsList[i]])));
                  _selectedItems[_selectedItems.Count - 1] = materialsList[i];
                }
              }
            }
          }

          DA.SetDataList(0, filteredMaterials);
          return;
        }
      }

      // update selected material
      FieldInfo selectedMaterial = _materials[_selectedItems.Last()];

      // create new material
      var mat = new AdSecMaterial(selectedMaterial);

      DA.SetData(0, new AdSecMaterialGoo(mat));
    }

    protected override void UpdateUIFromSelectedItems() {
      // get the selected material and parse it to type enum
      Enum.TryParse(_selectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
      // get list of standard codes for the selected material
      Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.StandardCodes(materialType);
      // add codes for selected material to list of dropdowns
      //_dropDownItems.Add(designCodeKVP.Keys.ToList());

      // make the UI look more intelligent
      if (_selectedItems[1].StartsWith("EN1992")) {
        _spacerDescriptions[1] = "Design Code";
        _spacerDescriptions[2] = "National Annex";
      } else {
        _spacerDescriptions[1] = "Code Group";
        _spacerDescriptions[2] = "Design Code";
      }

      // create string for selected item to use for type search while drilling
      int level = 1;
      string typeString = _selectedItems[level];
      bool drill = true;
      while (drill) {
        // get the type of the most recent selected from level above
        designCodeKVP.TryGetValue(typeString, out Type typ);

        // update the KVP by reflecting the type
        designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);

        // determine if we have reached the fields layer
        if (designCodeKVP.Count > 1) {
          level++;
          typeString = _selectedItems[level];

          if (typeString.StartsWith("Edition")) {
            _spacerDescriptions[level] = "Edition";
          }
          if (typeString.StartsWith("Metric") | typeString.StartsWith("US")) {
            _spacerDescriptions[level] = "Unit";
          }
        } else if (designCodeKVP.Count == 1) {
          // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
          typeString = designCodeKVP.Keys.First();
        } else {
          // if kvp is empty we have reached the field level
          // where we set the materials by reflecting the type
          _materials = Helpers.ReflectAdSecAPI.ReflectFields(typ);

          drill = false;

          _spacerDescriptions[_selectedItems.Count - 1] = "Grade";
        }
      }
      base.UpdateUIFromSelectedItems();
    }
  }
}
