using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecGH.Parameters;

using Oasys.AdSec.StandardMaterials;

namespace AdSecGH.Helpers {
  public static class ReflectionHelper {
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

    internal static Dictionary<string, FieldInfo> ReflectFields(Type type) {
      var materials = new Dictionary<string, FieldInfo>();
      if (type == null) {
        return materials;
      }
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

    internal static Dictionary<string, Type> ReflectNestedTypes(Type type) {
      var dict = new Dictionary<string, Type>();
      if (type == null) {
        return dict;
      }
      var members = type.FindMembers(MemberTypes.NestedType, BindingFlags.Public, null, null);
      foreach (var member in members) {
        dict.Add(member.Name, (Type)member);
      }

      var types = type.GetInterfaces();
      foreach (var baseType in types) {
        var baseMembers = baseType.FindMembers(MemberTypes.NestedType, BindingFlags.Public, null, null);
        foreach (var member in baseMembers) {
          dict.Add(member.Name, (Type)member);
        }
      }

      return dict;
    }

    internal static Dictionary<string, Type> StandardCodes(AdSecMaterial.AdSecMaterialType materialType) {
      switch (materialType) {
        case AdSecMaterial.AdSecMaterialType.Concrete: return ReflectNestedTypes(typeof(Concrete));

        case AdSecMaterial.AdSecMaterialType.Steel: return ReflectNestedTypes(typeof(Steel));

        case AdSecMaterial.AdSecMaterialType.FRP: return ReflectNestedTypes(typeof(FRP));

        case AdSecMaterial.AdSecMaterialType.Rebar: return ReflectNestedTypes(typeof(Reinforcement.Steel));

        case AdSecMaterial.AdSecMaterialType.Tendon: return ReflectNestedTypes(typeof(Reinforcement.Tendon));
      }

      return new Dictionary<string, Type>();
    }
  }
}
