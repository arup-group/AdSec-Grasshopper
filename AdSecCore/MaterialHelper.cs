using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

namespace AdSecGHCore {

  public static class MaterialHelper {

    private static Dictionary<string, object> Materials = new Dictionary<string, object>();
    private static readonly Type[] materialTypes = {
      typeof(IReinforcement), typeof(IConcrete),typeof(IDesignCode),
      typeof(ISteel), typeof(Reinforcement.Tendon)
    };

    public static string FindPath(object instance) {
      if (instance == null) {
        throw new ArgumentNullException(nameof(instance));
      }

      if (Materials.Count == 0) {
        Materials = FindMaterials();
      }

      return Materials.FirstOrDefault(x => x.Value.Equals(instance)).Key;
    }

    private static Dictionary<string, object> FindMaterials() {
      var asm = AppDomain.CurrentDomain.GetAssemblies().First(x => x.Modules.First().Name.Equals("AdSec_API.dll"));
      var dictionary = new Dictionary<string, object>();

      foreach (var t in asm.GetTypes()) {
        if (!t.IsClass && !t.IsInterface) {
          continue;
        }

        GetFieldInfos(t, ref dictionary);
      }

      return dictionary;
    }

    private static void GetFieldInfos(Type type, ref Dictionary<string, object> dictionary) {
      var fieldInfos = type.GetFields(BindingFlags.Static | BindingFlags.Public);
      var fieldInfosOfType = fieldInfos.Where(x => materialTypes.Any(y => y == x.FieldType));
      foreach (var field in fieldInfosOfType) {
        var value = field.GetValue(null);
        if (value == null) {
          continue;
        }

        dictionary.Add($"{type.FullName}.{field.Name}", value);
      }
    }

    private static string DesignCodeName(IDesignCode code) {
      if (code == null) {
        return string.Empty;
      }

      string path = FindPath(code);
      if (string.IsNullOrEmpty(path)) {
        return string.Empty;
      }

      // Remove prefix
      const string prefix = "Oasys.AdSec.DesignCode.";
      if (path.StartsWith(prefix)) {
        path = path.Substring(prefix.Length);
      }

      // Build code name
      var builder = new System.Text.StringBuilder();
      var parts = path.Split('.');

      for (int i = 0; i < parts.Length; i++) {
        if (i > 0) {
          builder.Append('+');
        }
        builder.Append(parts[i]);
      }

      return builder.ToString();
    }

    private static string MaterialDesignCodeName(string path) {
      if (string.IsNullOrEmpty(path)) {
        return string.Empty;
      }
      var parts = path.Split('+');
      var materialCode = new System.Text.StringBuilder();
      for (int i = 2; i < parts.Length; i++) {
        if (i > 2) {
          materialCode.Append('+');
        }
        materialCode.Append(parts[i]);
      }

      return RemoveAfterDot(materialCode.ToString());
    }

    private static string RemoveAfterDot(string input) {
      if (string.IsNullOrEmpty(input)) {
        return input;
      }

      int dotIndex = input.IndexOf('.');
      return dotIndex < 0 ? input : input.Substring(0, dotIndex);
    }

    private static bool IsMaterialRebar(string path) {
      return path.Contains("Reinforcement");
    }


    public static bool ValidateSection(MaterialDesign concreteSectionMaterial, AdSecRebarGroup[] groups) {
      string concreteSectionMaterialCode = DesignCodeName(concreteSectionMaterial.DesignCode);
      if (IsMaterialRebar(FindPath(concreteSectionMaterial.Material))) {
        throw new ArgumentException($"Section material can not be of rebar type. Select either concrete, steel or FRP");
      }
      if (groups != null) {
        foreach (var group in groups) {
          if (!IsMaterialRebar(group.CodeDescription)) {
            throw new ArgumentException($"Rebar material is not valid. Select either Rebar or Tendon");
          }
          string rebarMaterialCode = MaterialDesignCodeName(group.CodeDescription);
          if (!concreteSectionMaterialCode.Equals(rebarMaterialCode)) {
            throw new ArgumentException($"Rebar material and concrete design code are not consistent");
          }
        }
      }
      return true;
    }
  }
}
