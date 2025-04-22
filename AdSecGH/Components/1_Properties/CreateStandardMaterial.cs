using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore;
using AdSecGHCore.Constants;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Oasys.AdSec.DesignCode;

using OasysGH;
using OasysGH.Components;

using OasysUnits;

namespace AdSecGH.Components {
  public class CreateStandardMaterial : GH_OasysDropDownComponent {
    private Dictionary<string, FieldInfo> _materials;
    private static string designCodeString = "Design Code";
    private static string euroCodeString = "EN1992";
    public CreateStandardMaterial() : base("Standard Material", "Material",
      $"Create a new AdSec {designCodeString} based standard material", CategoryName.Name(), SubCategoryName.Cat1()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    // This region handles how the component in displayed on the ribbon including name, exposure level and icon
    public override Guid ComponentGuid => new Guid("42f42580-8ed7-42fb-9cc7-c6f6171a0248");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    public override OasysPluginInfo PluginInfo => AdSecGH.PluginInfo.Instance;
    protected override Bitmap Icon => Resources.StandardMaterial;

    private readonly IDictionary<string, string> _prefixMappings = new Dictionary<string, string> {
      { "Edition", "Edition" },
      { "Metric", "Unit" },
      { "US", "Unit" },
    };

    private void DrillDownMaterialHierarchy(Dictionary<string, Type> designCodeKVP, string typeString, string prevSelectedCode = "", string prevSelectedNA = "") {
      int level = 1;
      bool drill = true;

      while (drill) {
        // get the type of the most recent selected from level above
        designCodeKVP.TryGetValue(typeString, out var typ);

        // update the KVP by reflecting the type
        designCodeKVP = ReflectionHelper.ReflectNestedTypes(typ);

        if (designCodeKVP.Count > 1) {
          level++;
          _dropDownItems.Add(designCodeKVP.Keys.ToList());

          typeString = _selectedItems.Count - 1 < level
            ? GetNextSelection(level, designCodeKVP.Keys, prevSelectedCode, prevSelectedNA)
            : _selectedItems[level];

          _spacerDescriptions[level] = GetDescription(typeString);
        } else if (designCodeKVP.Count == 1) {
          typeString = designCodeKVP.Keys.First();
        } else {
          ProcessMaterialFields(typ);
          drill = false;
        }
      }
    }

    private string GetNextSelection(int level, IEnumerable<string> availableChoices, string prevSelectedCode, string prevSelectedNA) {
      // Try to maintain previous National Annex selection for Eurocode
      const int nationalAnnexLevel = 2;
      if (level == nationalAnnexLevel && prevSelectedCode.StartsWith(euroCodeString)) {
        var matchingNA = availableChoices.FirstOrDefault(code => code.Equals(prevSelectedNA));
        if (matchingNA != null) {
          _selectedItems.Add(matchingNA);
          return matchingNA;
        }
      }

      // Default to first available choice
      var defaultSelection = availableChoices.First();
      _selectedItems.Add(defaultSelection);
      return defaultSelection;
    }

    private void ProcessMaterialFields(Type typ) {
      _materials = ReflectionHelper.ReflectFields(typ);
      if (_materials.Count == 0) {
        _dropDownItems.Add(new List<string> { "No material" });
        _selectedItems.Add("No material");
      } else {
        _dropDownItems.Add(_materials.Keys.ToList());
        _selectedItems.Add(GetDefaultMaterialGrade());
      }
      _spacerDescriptions[_selectedItems.Count - 1] = "Grade";
    }

    private string GetDefaultMaterialGrade() {
      if (_selectedItems[1].StartsWith(euroCodeString)) {
        if (_materials.Keys.Count > 4) {
          return _materials.Keys.ElementAt(4); // C37
        }
        return _materials.Keys.Count == 3 ? _materials.Keys.ElementAt(1) : _materials.Keys.First(); // B500B
      }
      if (_selectedItems[1].StartsWith(euroCodeString)) {
        return _materials.Keys.ElementAt(2); // S355
      }
      return _materials.Keys.First();
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      if (_selectedItems.Count - 1 != i) {
        string prevSelectedCode = _selectedItems[1];
        string prevSelectedNA = _selectedItems[2];
        ResetDropdowns(i);

        Enum.TryParse(_selectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
        var designCodeKVP = ReflectionHelper.StandardCodes(materialType);
        _dropDownItems.Add(designCodeKVP.Keys.ToList());

        if (_selectedItems.Count == 1) {
          _selectedItems.Add(GetInitialCodeSelection(designCodeKVP.Keys, prevSelectedCode));
        }

        UpdateUIDescriptions(_selectedItems[1]);
        DrillDownMaterialHierarchy(designCodeKVP, _selectedItems[1], prevSelectedCode, prevSelectedNA);
      }

      base.UpdateUI();
    }

    private void ResetDropdowns(int i) {
      while (_dropDownItems.Count > 1) {
        _dropDownItems.RemoveAt(1);
      }
      while (_selectedItems.Count > i + 1) {
        _selectedItems.RemoveAt(i + 1);
      }
    }

    private static string GetInitialCodeSelection(IEnumerable<string> codes, string prevSelectedCode) {
      if (prevSelectedCode.StartsWith(euroCodeString)) {
        return codes.FirstOrDefault(code => code.StartsWith(euroCodeString)) ?? codes.First();
      }
      return codes.Contains(prevSelectedCode) ? prevSelectedCode : codes.First();
    }

    public override void VariableParameterMaintenance() {
      if (Params.Input.Count == 0) {
        Params.RegisterInputParam(new Param_String());
        Params.Input[0].NickName = "S";
        Params.Input[0].Name = "Search";
        Params.Input[0].Description
          = $"[Optional] Search for Grade {Environment.NewLine}Note: input 'all' to list all grades from the selected code";
        Params.Input[0].Access = GH_ParamAccess.item;
        Params.Input[0].Optional = true;
      }
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        "Material Type",
        designCodeString,
        "National Annex",
        "Edition",
        "Grade",
        "Another other",
        "This is so deep",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

      // get list of material types defined in material parameter
      var materialTypes = Enum.GetNames(typeof(AdSecMaterial.AdSecMaterialType)).ToList();

      _dropDownItems.Add(materialTypes);
      _selectedItems.Add(materialTypes[0]);

      if (_dropDownItems.Count == 1) {
        var designCodeKVP = ReflectionHelper.StandardCodes(AdSecMaterial.AdSecMaterialType.Concrete);
        _dropDownItems.Add(designCodeKVP.Keys.ToList());
        // select default code to EN1992
        _selectedItems.Add(designCodeKVP.Keys.ElementAt(4));

        // create string for selected item to use for type search while drilling
        string typeString = _selectedItems[_selectedItems.Count - 1];
        int level = 1;
        bool drill = true;
        while (drill) {
          // get the type of the most recent selected from level above
          designCodeKVP.TryGetValue(typeString, out var typ);

          // update the KVP by reflecting the type
          designCodeKVP = ReflectionHelper.ReflectNestedTypes(typ);

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
            typeString = _selectedItems[_selectedItems.Count - 1];
          } else if (designCodeKVP.Count == 1) {
            // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
            typeString = designCodeKVP.Keys.First();
          } else {
            // if kvp is empty we have reached the field level
            // where we set the materials by reflecting the type
            _materials = ReflectionHelper.ReflectFields(typ);
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
      pManager.AddTextParameter("Search", "S",
        $"[Optional] Search for Grade {Environment.NewLine}Note: input 'all' to list all grades from the selected code",
        GH_ParamAccess.item);
      pManager[0].Optional = true;
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager) {
      pManager.AddGenericParameter("Material", "Mat", "AdSec Material", GH_ParamAccess.list);
    }



    protected override void SolveInternal(IGH_DataAccess DA) {
      string search = string.Empty;
      if (DA.GetData(0, ref search)) {
        search = search.ToLower();
        // filter by search pattern
        if (!string.IsNullOrEmpty(search)) {
          var materialsList = _materials.Keys.ToList();
          var filteredMaterials = new List<AdSecMaterialGoo>();

          for (int i = 0; i < materialsList.Count; i++) {
            var material = new AdSecMaterial(_materials[materialsList[i]]);

            var materialDesign = new MaterialDesign() {
              Material = material.Material,
              DesignCode = GetDesignCode(material),
            };
            if (search.ToLower() == "all") {
              filteredMaterials.Add(new AdSecMaterialGoo(materialDesign));
              _selectedItems[_selectedItems.Count - 1] = "all";
            } else {
              if (materialsList[i].ToLower().Contains(search)) {
                filteredMaterials.Add(new AdSecMaterialGoo(materialDesign));
                _selectedItems[_selectedItems.Count - 1] = materialsList[i];
              }

              if (!search.Any(char.IsDigit)) {
                string test = materialsList[i];
                test = MaterialHelper.ReplaceWithTimeout(test, "[0-9]", string.Empty);
                test = test.Replace(".", string.Empty);
                test = test.Replace("-", string.Empty);
                test = test.ToLower();
                if (test.Contains(search)) {
                  filteredMaterials.Add(new AdSecMaterialGoo(materialDesign));
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
      var selectedMaterial = _materials[_selectedItems[_selectedItems.Count - 1]];

      // create new material
      var adSecMaterial = new AdSecMaterial(selectedMaterial);
      var updatedDesignMaterial = new MaterialDesign() {
        Material = adSecMaterial.Material,
        DesignCode = GetDesignCode(adSecMaterial),
        GradeName = selectedMaterial.Name,
      };

      DA.SetData(0, new AdSecMaterialGoo(updatedDesignMaterial));
    }

    private static DesignCode GetDesignCode(AdSecMaterial material) {
      var designCode = new DesignCode();
      if (material.DesignCode.DesignCode != null) {
        designCode.IDesignCode = material.DesignCode.DesignCode;
      } else {
        designCode.IDesignCode = EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014;
      }
      designCode.DesignCodeName = MaterialHelper.DesignCodeName(designCode.IDesignCode);
      return designCode;
    }

    protected override void UpdateUIFromSelectedItems() {
      // get the selected material and parse it to type enum
      Enum.TryParse(_selectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
      // get list of standard codes for the selected material
      var designCodeKVP = ReflectionHelper.StandardCodes(materialType);
      // add codes for selected material to list of dropdowns
      // make the UI look more intelligent
      UpdateUIDescriptions(_selectedItems[1]);

      // create string for selected item to use for type search while drilling
      int level = 1;
      string typeString = _selectedItems[level];
      bool drill = true;
      while (drill) {
        // get the type of the most recent selected from level above
        designCodeKVP.TryGetValue(typeString, out var typ);

        // update the KVP by reflecting the type
        designCodeKVP = ReflectionHelper.ReflectNestedTypes(typ);

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
          _materials = ReflectionHelper.ReflectFields(typ);

          drill = false;

          _spacerDescriptions[_selectedItems.Count - 1] = "Grade";
        }
      }

      base.UpdateUIFromSelectedItems();
    }

    private string GetDescription(string typeString) {
      string result = _prefixMappings.Where(mapping => typeString.StartsWith(mapping.Key))
       .Select(mapping => mapping.Value).FirstOrDefault();

      return string.IsNullOrEmpty(result) ? designCodeString : result;
    }

    private void UpdateUIDescriptions(string codeType) {
      if (codeType.StartsWith(euroCodeString)) {
        _spacerDescriptions[1] = designCodeString;
        _spacerDescriptions[2] = "National Annex";
      } else {
        _spacerDescriptions[1] = "Code Group";
        _spacerDescriptions[2] = designCodeString;
      }
    }
  }
}
