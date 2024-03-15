using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdSecGH.Helpers {
  public static class ReflectionHelper {
    internal static Dictionary<string, Type> ReflectAdSecNamespace(string nspace) {
      Assembly adsecAPI = AddReferencePriority.AdSecAPI;
      IEnumerable<Type> q = from t in adsecAPI.GetTypes()
                            where t.IsInterface && t.Namespace == nspace
                            select t;
      var dict = new Dictionary<string, Type>();
      foreach (Type typ in q) {
        if (nspace + "." + typ.Name == typ.FullName) {
          dict.Add(typ.Name, typ);
        }
      }

      return dict;
    }

    internal static Dictionary<string, FieldInfo> ReflectFields(Type type) {
      var fields = type.GetFields().ToList();

      Type[] types = type.GetInterfaces();
      foreach (Type baseType in types) {
        fields.AddRange(baseType.GetFields());
      }

      var materials = new Dictionary<string, FieldInfo>();
      foreach (FieldInfo field in fields) {
        materials.Add(field.Name, field);
      }

      return materials;
    }

    internal static Dictionary<string, Type> ReflectNestedTypes(Type type) {
      var dict = new Dictionary<string, Type>();
      MemberInfo[] members = type.FindMembers(MemberTypes.NestedType, BindingFlags.Public, null, null);
      foreach (MemberInfo member in members) {
        dict.Add(member.Name, (Type)member);
      }

      Type[] types = type.GetInterfaces();
      foreach (Type baseType in types) {
        MemberInfo[] baseMembers = baseType.FindMembers(MemberTypes.NestedType, BindingFlags.Public, null, null);
        foreach (MemberInfo member in baseMembers) {
          dict.Add(member.Name, (Type)member);
        }
      }

      return dict;
    }

    internal static Dictionary<string, Type> StandardCodes(Parameters.AdSecMaterial.AdSecMaterialType materialType) {
      switch (materialType) {
        case Parameters.AdSecMaterial.AdSecMaterialType.Concrete:
          return ReflectNestedTypes(typeof(Oasys.AdSec.StandardMaterials.Concrete));

        case Parameters.AdSecMaterial.AdSecMaterialType.Steel:
          return ReflectNestedTypes(typeof(Oasys.AdSec.StandardMaterials.Steel));

        case Parameters.AdSecMaterial.AdSecMaterialType.FRP:
          return ReflectNestedTypes(typeof(Oasys.AdSec.StandardMaterials.FRP));

        case Parameters.AdSecMaterial.AdSecMaterialType.Rebar:
          return ReflectNestedTypes(typeof(Oasys.AdSec.StandardMaterials.Reinforcement.Steel));

        case Parameters.AdSecMaterial.AdSecMaterialType.Tendon:
          return ReflectNestedTypes(typeof(Oasys.AdSec.StandardMaterials.Reinforcement.Tendon));
      }
      return null;
    }
  }
}
