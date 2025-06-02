using System;
using System.Collections.Generic;
using System.Linq;

using AdSecCore.Helpers;

using AdSecGH.Parameters;

using Oasys.AdSec.StandardMaterials;

namespace AdSecGH.Helpers {
  public static class CodeHelper {

    internal static AdSecDesignCode GetDesignCode(string json) {
      // "codes":{"concrete":"EC2_GB_04"}

      string[] jsonSplit = json.Split(new string[] { "\"codes\": {\r\n        \"concrete\": \"" }, StringSplitOptions.None);
      if (jsonSplit.Length == 1) {
        jsonSplit = json.Split(new string[] { "codes\":{\"concrete\":\"" }, StringSplitOptions.None);
      }
      if (jsonSplit.Length < 2) {
        return null;
      }
      string codeName = jsonSplit[1].Split('"')[0];

      if (!AdSecCore.Helpers.FileHelper.CodesStrings.TryGetValue(codeName, out string codeString)) {
        return null;
      }

      var designCodeLevelsSplit = codeString.Split('+').ToList();

      return new AdSecDesignCode(designCodeLevelsSplit);
    }

    internal static Dictionary<string, Type> StandardCodes(AdSecMaterial.AdSecMaterialType materialType) {
      switch (materialType) {
        case AdSecMaterial.AdSecMaterialType.Concrete: return ReflectionHelper.ReflectNestedTypes(typeof(Concrete));

        case AdSecMaterial.AdSecMaterialType.Steel: return ReflectionHelper.ReflectNestedTypes(typeof(Steel));

        case AdSecMaterial.AdSecMaterialType.FRP: return ReflectionHelper.ReflectNestedTypes(typeof(FRP));

        case AdSecMaterial.AdSecMaterialType.Rebar: return ReflectionHelper.ReflectNestedTypes(typeof(Reinforcement.Steel));

        case AdSecMaterial.AdSecMaterialType.Tendon: return ReflectionHelper.ReflectNestedTypes(typeof(Reinforcement.Tendon));
      }
      return new Dictionary<string, Type>();
    }
  }
}
