using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

namespace AdSecGHCore {

  public static class MaterialHelper {

    private static Dictionary<string, object> Materials = new Dictionary<string, object>();
    private static readonly Type[] materialTypes = {
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
  }
}
