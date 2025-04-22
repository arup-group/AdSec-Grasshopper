using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using AdSecCore.Functions;

using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHCore.Constants;

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
    protected override Bitmap Icon => Resources.StandardMaterial;

    private const string en199 = "EN199";
    private const string en1992 = "EN1992";
    private const string en1993 = "EN1993";
    private const string designCodeName = "Design Code";
    private const string nationalAnnexName = "National Annex";
    private const string codeGroupName = "Code Group";
    private const string noMaterialText = "No material";
    private const string materialTypeText = "Material Type";
    private const string EditionText = "Edition";
    private const string gradeText = "Grade";
    private const string allSearchText = "all";
    private readonly IDictionary<string, string> _prefixMappings = new Dictionary<string, string> {
      { EditionText, EditionText },
      { "Metric", "Unit" },
      { "US", "Unit" },
    };
    private Dictionary<string, FieldInfo> _materials;

    public CreateStandardMaterial() : base("Standard Material", "Material",
      "Create a new AdSec Design Code based standard material", CategoryName.Name(), SubCategoryName.Cat1()) {
      Hidden = true; // sets the initial state of the component to hidden
    }

    public override void SetSelected(int i, int j) {
      _selectedItems[i] = _dropDownItems[i][j];

      if (_selectedItems.Count - 1 != i) {
        string prevSelectedCode = _selectedItems[1];
        string prevSelectedNA = _selectedItems[2];

        ClearDropDownItems();
        ClearSelectedItems(i);

        Enum.TryParse(_selectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
        var designCodeKVP = ReflectionHelper.StandardCodes(materialType);
        _dropDownItems.Add(designCodeKVP.Keys.ToList());
        if (_selectedItems.Count == 1) {
          if (prevSelectedCode.StartsWith(en199)) {
            string matchingCode = _dropDownItems[1].FirstOrDefault(code => code.StartsWith(en199));
            if (matchingCode != null) {
              _selectedItems.Add(matchingCode);
            }
          } else if (_dropDownItems[1].Contains(prevSelectedCode)) {
            _selectedItems.Add(prevSelectedCode);
          } else {
            _selectedItems.Add(designCodeKVP.Keys.First());
          }
        }

        SetSpacerDescriptions();

        // create string for selected item to use for type search while drilling
        int level = 1;
        string typeString = _selectedItems[level];
        bool drill = true;
        while (drill) {
          designCodeKVP.TryGetValue(typeString, out var typ);
          designCodeKVP = ReflectionHelper.ReflectNestedTypes(typ);

          // determine if we have reached the fields layer
          if (designCodeKVP.Count > 1) {
            level++;

            // if kvp has >1 values we add them to create a new dropdown list
            _dropDownItems.Add(designCodeKVP.Keys.ToList());

            // with first item being the selected
            if (_selectedItems.Count - 1 < level) {
              if (level == 2) {
                if (prevSelectedCode.StartsWith(en1992)) {
                  string matchingCode = _dropDownItems[2].FirstOrDefault(code => code.Equals(prevSelectedNA));
                  if (matchingCode != null) {
                    _selectedItems.Add(matchingCode);
                  }
                } else {
                  _selectedItems.Add(designCodeKVP.Keys.First());
                }
              } else {
                _selectedItems.Add(designCodeKVP.Keys.First());
              }

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
            _materials = ReflectionHelper.ReflectFields(typ);
            if (_materials.Count == 0) {
              _dropDownItems.Add(new List<string> {
                noMaterialText,
              });
              _selectedItems.Add(noMaterialText);
            } else {
              // if kvp has values we add them to create a new dropdown list
              _dropDownItems.Add(_materials.Keys.ToList());
              // with first item being the selected
              if (_selectedItems[1].StartsWith(en1992)) {
                if (_materials.Keys.Count > 4) {
                  _selectedItems.Add(_materials.Keys.ElementAt(4)); // C37
                } else if (_materials.Keys.Count == 3) {
                  _selectedItems.Add(_materials.Keys.ElementAt(1)); // B500B
                } else {
                  _selectedItems.Add(_materials.Keys.First());
                }
              } else if (_selectedItems[1].StartsWith(en1993)) {
                _selectedItems.Add(_materials.Keys.ElementAt(2)); // S355
              } else {
                _selectedItems.Add(_materials.Keys.First());
              }
            }

            drill = false;

            _spacerDescriptions[_selectedItems.Count - 1] = gradeText;
          }
        }
      }

      base.UpdateUI();
    }

    private void ClearSelectedItems(int i) {
      while (_selectedItems.Count > i + 1) {
        _selectedItems.RemoveAt(i + 1);
      }
    }

    private void ClearDropDownItems() {
      while (_dropDownItems.Count > 1) {
        _dropDownItems.RemoveAt(1);
      }
    }

    public override void VariableParameterMaintenance() {
      if (Params.Input.Count != 0) {
        return;
      }

      Params.RegisterInputParam(new Param_String());
      Params.Input[0].NickName = "S";
      Params.Input[0].Name = "Search";
      Params.Input[0].Description
        = $"[Optional] Search for Grade {Environment.NewLine}Note: input 'all' to list all grades from the selected code";
      Params.Input[0].Access = GH_ParamAccess.item;
      Params.Input[0].Optional = true;
    }

    protected override void InitialiseDropdowns() {
      _spacerDescriptions = new List<string>(new[] {
        materialTypeText,
        designCodeName,
        nationalAnnexName,
        EditionText,
        gradeText,
        "Another other",
        "This is so deep",
      });

      _dropDownItems = new List<List<string>>();
      _selectedItems = new List<string>();

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
          designCodeKVP.TryGetValue(typeString, out var typ);
          designCodeKVP = ReflectionHelper.ReflectNestedTypes(typ);

          // determine if we have reached the fields layer
          if (designCodeKVP.Count > 1) {
            level++;
            // if kvp has >1 values we add them to create a new dropdown list
            _dropDownItems.Add(designCodeKVP.Keys.ToList());
            // with first item being the selected
            _selectedItems.Add(level == 2 ? designCodeKVP.Keys.ElementAt(6) : designCodeKVP.Keys.First());

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
      string search = "";
      if (DA.GetData(0, ref search)) {
        search = search.ToLower();
        if (search != "") {
          var materialsList = _materials.Keys.ToList();
          var filteredMaterials = new List<AdSecMaterialGoo>();

          foreach (string material in materialsList) {
            var adsecMaterial = new AdSecMaterial(_materials[material]);

            var materialDesign = new MaterialDesign() {
              Material = adsecMaterial.Material,
              DesignCode = new DesignCode() {
                IDesignCode = adsecMaterial.DesignCode.DesignCode,
                DesignCodeName = adsecMaterial.DesignCode.DesignCodeName,
              },
            };
            if (search.ToLower() == allSearchText) {
              filteredMaterials.Add(new AdSecMaterialGoo(materialDesign));
              _selectedItems[_selectedItems.Count - 1] = allSearchText;
            } else {
              if (material.ToLower().Contains(search)) {
                filteredMaterials.Add(new AdSecMaterialGoo(materialDesign));
                _selectedItems[_selectedItems.Count - 1] = material;
              }

              if (search.Any(char.IsDigit)) {
                continue;
              }

              string materialName = material;
              materialName = Regex.Replace(materialName, "[0-9]", string.Empty, RegexOptions.None,
                TimeSpan.FromSeconds(2));
              materialName = materialName.Replace(".", string.Empty);
              materialName = materialName.Replace("-", string.Empty);
              materialName = materialName.ToLower();
              if (!materialName.Contains(search)) {
                continue;
              }

              filteredMaterials.Add(new AdSecMaterialGoo(materialDesign));
              _selectedItems[_selectedItems.Count - 1] = material;
            }
          }

          DA.SetDataList(0, filteredMaterials);
          return;
        }
      }

      var selectedMaterial = _materials[_selectedItems[_selectedItems.Count - 1]];

      var adSecMaterial = new AdSecMaterial(selectedMaterial);
      var materialDesign2 = new MaterialDesign() {
        Material = adSecMaterial.Material,
        DesignCode = new DesignCode() {
          IDesignCode = adSecMaterial.DesignCode.DesignCode,
          DesignCodeName = adSecMaterial.DesignCode.DesignCodeName,
        },
        GradeName = selectedMaterial.Name,
      };

      DA.SetData(0, new AdSecMaterialGoo(materialDesign2));
    }

    protected override void UpdateUIFromSelectedItems() {
      Enum.TryParse(_selectedItems[0], out AdSecMaterial.AdSecMaterialType materialType);
      var designCodeKVP = ReflectionHelper.StandardCodes(materialType);

      SetSpacerDescriptions();

      int level = 1;
      string typeString = _selectedItems[level];
      bool drill = true;
      while (drill) {
        designCodeKVP.TryGetValue(typeString, out var typ);
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

          _spacerDescriptions[_selectedItems.Count - 1] = gradeText;
        }
      }

      base.UpdateUIFromSelectedItems();
    }

    private void SetSpacerDescriptions() {
      if (_selectedItems[1].StartsWith(en1992)) {
        _spacerDescriptions[1] = designCodeName;
        _spacerDescriptions[2] = nationalAnnexName;
      } else {
        _spacerDescriptions[1] = codeGroupName;
        _spacerDescriptions[2] = designCodeName;
      }
    }

    private string GetDescription(string typeString) {
      string result = _prefixMappings.Where(mapping => typeString.StartsWith(mapping.Key))
       .Select(mapping => mapping.Value).FirstOrDefault();

      return string.IsNullOrEmpty(result) ? designCodeName : result;
    }
  }
}
