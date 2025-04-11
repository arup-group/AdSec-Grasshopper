using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

public static class MaterialHelper {

  private static Dictionary<string, object> Materials = new Dictionary<string, object>();
  private static Type[] materialTypes = new[] {
    typeof(IReinforcement), typeof(IConcrete),
    typeof(ISteel), typeof(Reinforcement.Tendon)
  };

  public static string FindPath(object instance) {
    if (instance == null) {
      throw new ArgumentNullException(nameof(instance));
    }

    if (Materials.Count == 0) {
      Materials = FindMaterials();
    }

    foreach (var kvp in Materials) {
      if (kvp.Value.Equals(instance)) {
        return kvp.Key;
      }
    }

    return null;
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
    foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public)) {
      if (materialTypes.Any(x => x == field.FieldType)) {
        try {
          var value = field.GetValue(null);
          dictionary.Add($"{type.FullName}.{field.Name}", value);
        } catch { }
      }
    }
  }
}
