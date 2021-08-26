using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;

namespace GhAdSec.Helpers
{
    public static class ReflectAdSecAPI
    {
        internal static Dictionary<string, Type> StandardCodes(Parameters.AdSecMaterial.AdSecMaterialType materialType)
        {
            switch (materialType)
            {
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

        internal static Dictionary<string, Type> ReflectNestedTypes(Type type)
        {
            Dictionary<string, Type> dict = new Dictionary<string, Type>();
            var subClasses = type.FindMembers(MemberTypes.NestedType, BindingFlags.Public, null, null);
            foreach (var subClass in subClasses)
            {
                dict.Add(subClass.Name, (Type)subClass);
            }
            return dict;
        }
        internal static Dictionary<List<string>, Type> ReflectMethods(Type type)
        {
            Dictionary<List<string>, Type> dict = new Dictionary<List<string>, Type>();
            var subClasses = type.FindInterfaces(null, null);
            //foreach (MemberInfo subClass in subClasses)
            //{
            //    subClass.CustomAttributes.ToList();
            //    List<string> nameSummary = new List<string>();
            //    nameSummary.Add(subClass.Name);
            //    nameSummary.Add(subClass.Name);
            //    dict.Add(nameSummary, (Type)subClass);
            //}
            return dict;
        }
        internal static Dictionary<string, FieldInfo> ReflectFields(Type type)
        {
            Dictionary<string, FieldInfo> materials = new Dictionary<string, FieldInfo>();

            FieldInfo[] fields = type.GetFields();
            if (fields.Length > 0)
            {
                foreach (var field in fields)
                    materials.Add(field.Name, field);
            }
            return materials;
        }

        internal static Dictionary<string, Type> ReflectNamespace(string nspace)
        {
            Assembly adsecAPI = GhAdSec.AddReferencePriority.AdSecAPI;
            var q = from t in adsecAPI.GetTypes()
                    where t.IsInterface && t.Namespace == nspace
                    select t;
            Dictionary<string, Type> dict = new Dictionary<string, Type>();
            foreach(Type typ in q)
            {
                if (nspace + "." + typ.Name == typ.FullName)
                    dict.Add(typ.Name, typ);
            }
            return dict;
        }
    }
}
