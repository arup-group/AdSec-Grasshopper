using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecCore.Constants;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;

namespace AdSecGH.Helpers {
  public static class ReflectionHelper {

    internal static Dictionary<string, FieldInfo> ReflectFields(Type type) {
      var materials = new Dictionary<string, FieldInfo>();

      var fields = type.GetFields().ToList();

      var types = type.GetInterfaces();
      foreach (var baseType in types) {
        fields.AddRange(baseType.GetFields());
      }

      foreach (var field in fields) {
        materials.Add(field.Name, field);
      }

      return materials;
    }

    public static Dictionary<string, Type> ReflectAdSecNamespace(string @namespace) {
      var adsecAPI = AddReferencePriority.AdSecAPI;
      var q = from t in adsecAPI.GetTypes() where t.IsInterface && t.Namespace == @namespace select t;
      var dict = new Dictionary<string, Type>();
      foreach (var typ in q) {
        if ($"{@namespace}.{typ.Name}" == typ.FullName) {
          dict.Add(typ.Name, typ);
        }
      }

      return dict;
    }

    internal static Dictionary<string, Type> StandardCodes(MaterialType materialType) {
      switch (materialType) {
        case MaterialType.Concrete: return AdSecFileHelper.ReflectNestedTypes(typeof(Concrete));

        case MaterialType.Steel: return AdSecFileHelper.ReflectNestedTypes(typeof(Steel));

        case MaterialType.FRP: return AdSecFileHelper.ReflectNestedTypes(typeof(FRP));

        case MaterialType.Rebar:
          return AdSecFileHelper.ReflectNestedTypes(typeof(Reinforcement.Steel));

        case MaterialType.Tendon:
          return AdSecFileHelper.ReflectNestedTypes(typeof(Reinforcement.Tendon));
      }

      return new Dictionary<string, Type>();
    }

  }
}
