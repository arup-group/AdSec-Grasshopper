//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Oasys.AdSec.Materials;
//using System.Reflection;

//namespace GhAdSec.Test
//{
//    class Test
//    {
//        // list of materials
//        Dictionary<string, FieldInfo> materials;
//        FieldInfo selectedMaterial;
//        // list of lists with all dropdown lists conctent
//        List<List<string>> dropdownitems;
//        Dictionary<string, Type> designCodeKVP;
//        // list of selected items
//        List<string> selecteditems;
//        void setup()
//        {
//            if (selecteditems == null)
//            {
//                // create a new list of selected items and add the first material type
//                selecteditems = new List<string>();
//                selecteditems.Add("Concrete");
//            }
//            if (dropdownitems == null)
//            {
//                // create a new list of selected items and add the first material type
//                dropdownitems = new List<List<string>>();
//                //dropdownitems.Add(materialTypes);
//            }
//            if (dropdownitems.Count == 1)
//            {
//                //Enum.TryParse(selecteditems[0], out AdSecMaterial.AdSecMaterialType materialType);
//                designCodeKVP = GhAdSec.Test.ReflectAdSecAPI.StandardCodes();
//                dropdownitems.Add(designCodeKVP.Keys.ToList());
//                // select default code to EN1992
//                selecteditems.Add(designCodeKVP.Keys.ElementAt(4));

//                // create string for selected item to use for type search while drilling
//                string typeString = selecteditems.Last();
//                bool drill = true;
//                while (drill)
//                {
//                    // get the type of the most recent selected from level above
//                    designCodeKVP.TryGetValue(typeString, out Type typ);

//                    // update the KVP by reflecting the type
//                    designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.ReflectTypes(typ);

//                    // determine if we have reached the fields layer
//                    if (designCodeKVP.Count > 1)
//                    {
//                        // if kvp has >1 values we add them to create a new dropdown list
//                        dropdownitems.Add(designCodeKVP.Keys.ToList());
//                        // with first item being the selected
//                        selecteditems.Add(designCodeKVP.Keys.First());
//                        // and set the next search item to this
//                        typeString = selecteditems.Last();
//                    }
//                    else if (designCodeKVP.Count == 1)
//                    {
//                        // if kvp is = 1 then we do not need to create dropdown list, but keep drilling
//                        typeString = designCodeKVP.Keys.First();
//                    }
//                    else
//                    {
//                        // if kvp is empty we have reached the field level
//                        // where we set the materials by reflecting the type
//                        materials = GhAdSec.Helpers.ReflectAdSecAPI.ReflectFields(typ);
//                        // if kvp has values we add them to create a new dropdown list
//                        dropdownitems.Add(materials.Keys.ToList());
//                        // with first item being the selected
//                        selecteditems.Add(materials.Keys.First().ToString());
//                        // stop drilling
//                        drill = false;
//                    }
//                }
//            }
//        }
//        void test()
//        {
//            FieldInfo fieldGrade = selectedMaterial;
//            // convert reflected interface to member
//            IMaterial m_material = (IMaterial)fieldGrade.GetValue(null);
//            // get the name of the grade
//            string m_materialGradeName = fieldGrade.Name;

//            // Get material type
//            string matStr = fieldGrade.DeclaringType.FullName.Replace("Oasys.AdSec.StandardMaterials.", "");

//            string[] matStrs = matStr.Split('+');

//            // Get all DesignCodes
//            Dictionary<string, Type> designCodeKVP = GhAdSec.Test.ReflectAdSecAPI.ReflectNamespace("Oasys.AdSec.DesignCode");

//            // Find the DesignCode
//            string designcodeName = "";
//            Type typ = null;
//            for (int i = 1; i < matStrs.Length - 1; i++)
//            {
//                designcodeName = designcodeName + matStrs[i] + " ";

//                designCodeKVP.TryGetValue(matStrs[i], out typ);
//                designCodeKVP = GhAdSec.Helpers.ReflectAdSecAPI.ReflectTypes(typ);
//            }

//            Dictionary<string, FieldInfo> designcodes = GhAdSec.Test.ReflectAdSecAPI.ReflectFields(typ);
//            FieldInfo fieldDesignCode;
//            designcodes.TryGetValue(matStrs.Last(), out fieldDesignCode);
//            Oasys.AdSec.DesignCode.IDesignCode m_designCode = (Oasys.AdSec.DesignCode.IDesignCode)fieldDesignCode.GetValue(null);
//            string m_designCodeName = designcodeName + " " + matStrs.Last();

//        }
//    }
//}
//namespace GhAdSec.Test
//{
//    public static class ReflectAdSecAPI
//    {
//        internal static Dictionary<string, Type> StandardCodes()
//        {
//            return ReflectTypes(typeof(Oasys.AdSec.StandardMaterials.Concrete));
//        }

//        internal static Dictionary<string, Type> ReflectTypes(Type type)
//        {
//            Dictionary<string, Type> dict = new Dictionary<string, Type>();
//            var subClasses = type.FindMembers(MemberTypes.NestedType, BindingFlags.Public, null, null);
//            foreach (var subClass in subClasses)
//            {
//                dict.Add(subClass.Name, (Type)subClass);
//            }
//            return dict;
//        }
//        internal static Dictionary<string, FieldInfo> ReflectFields(Type type)
//        {
//            Dictionary<string, FieldInfo> materials = new Dictionary<string, FieldInfo>();

//            FieldInfo[] fields = type.GetFields();
//            if (fields.Length > 0)
//            {
//                foreach (var field in fields)
//                    materials.Add(field.Name, field);
//            }
//            return materials;
//        }

//        internal static Dictionary<string, Type> ReflectNamespace(string nspace)
//        {
//            Assembly adsecAPI = GhAdSec.AddReferencePriority.adsecAPI;
//            var q = from t in adsecAPI.GetTypes()
//                    where t.IsInterface && t.Namespace == nspace
//                    select t;
//            Dictionary<string, Type> dict = new Dictionary<string, Type>();
//            foreach (Type typ in q)
//            {
//                if (nspace + "." + typ.Name == typ.FullName)
//                    dict.Add(typ.Name, typ);
//            }
//            return dict;
//        }
//    }
//}

