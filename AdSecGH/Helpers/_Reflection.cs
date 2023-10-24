using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdSecGH.Helpers {
  public static class ReflectAdSecAPI {

    internal static Dictionary<string, Type> ReflectAdSecNamespace(string nspace) {
      Assembly adsecAPI = AddReferencePriority.AdSecAPI;
      try {
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
      } catch (Exception e) {

        throw e;
      }
    }

    internal static Dictionary<string, FieldInfo> ReflectFields(Type type) {
      var materials = new Dictionary<string, FieldInfo>();

      FieldInfo[] fields = type.GetFields();
      if (fields.Length > 0) {
        foreach (FieldInfo field in fields) {
          materials.Add(field.Name, field);
        }
      }
      return materials;
    }

    internal static Dictionary<string, Type> ReflectNestedTypes(Type type) {
      var dict = new Dictionary<string, Type>();
      MemberInfo[] subClasses = type.FindMembers(MemberTypes.NestedType, BindingFlags.Public, null, null);
      foreach (MemberInfo subClass in subClasses) {
        dict.Add(subClass.Name, (Type)subClass);
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
