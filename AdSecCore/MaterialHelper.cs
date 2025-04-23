using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

namespace AdSecGHCore {

  public static class MaterialHelper {

    private static Dictionary<string, object> Materials = new();
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

    public static string DesignCodeName(IDesignCode code) {
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

    // Add this helper method to the class
    public static string ReplaceWithTimeout(string input, string pattern, string replacement, int timeoutMs = 1000) {
      try {
        var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(timeoutMs));
        return regex.Replace(input, replacement);
      } catch (RegexMatchTimeoutException) {
        throw new RegexMatchTimeoutException($"Regex operation timed out after {timeoutMs}ms");
      }
    }
  }
}
