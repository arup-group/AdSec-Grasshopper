using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

namespace AdSecGH.Parameters {
  /// <summary>
  /// AdSec DesignCode class, this class defines the basic properties and methods for any AdSec DesignCode
  /// </summary>
  public class AdSecDesignCode {
    public IDesignCode DesignCode { get; set; }
    public string DesignCodeName { get; set; }
    public bool IsValid {
      get {
        if (DesignCode == null) {
          return false;
        }
        return true;
      }
    }

    public AdSecDesignCode() {
    }

    public AdSecDesignCode(IDesignCode designCode, string designCodeName) {
      DesignCode = designCode;
      DesignCodeName = designCodeName;
    }

    internal AdSecDesignCode(FieldInfo fieldDesignCode) {
      string designCodeReflectedLevels = fieldDesignCode.DeclaringType.FullName.Replace("Oasys.AdSec.DesignCode.", "");
      var designCodeLevelsSplit = designCodeReflectedLevels.Split('+').ToList();
      CreateFromReflectedLevels(designCodeLevelsSplit, true);
    }

    internal AdSecDesignCode(List<string> designCodeReflectedLevels) {
      CreateFromReflectedLevels(designCodeReflectedLevels);
    }

    public AdSecDesignCode Duplicate() {
      if (this == null) {
        return null;
      }
      var dup = (AdSecDesignCode)MemberwiseClone();
      return dup;
    }

    public override string ToString() {
      return DesignCodeName.Replace("  ", " ");
    }

    private bool CreateFromReflectedLevels(List<string> designCodeReflectedLevels, bool fromDesignCode = false) {
      // Get all DesignCodes in DLL under namespace
      Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");

      // Loop through DesignCodes types to find the DesignCode type matching our input list of levels
      string designcodeName = "";
      Type typ = null;
      for (int i = 0; i < designCodeReflectedLevels.Count - 1; i++) {
        designcodeName = designcodeName + designCodeReflectedLevels[i] + " ";
        designCodeKVP.TryGetValue(designCodeReflectedLevels[i], out typ);
        if (typ == null) {
          return false;
        }
        designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);
      }
      if (designCodeReflectedLevels.Count == 1) {
        designcodeName = designCodeReflectedLevels[0];
        designCodeKVP.TryGetValue(designCodeReflectedLevels[0], out typ);
        if (typ == null) {
          return false;
        }
      }

      // we need to find the right type Interface under Oasys.AdSec.IAdsec in order to cast to IDesignCode
      // the string to search for depends on where we call this function from, if we come from an IMaterial type
      // we can simply use the full name but if from IDesignCode we need to add the name of the code with a +
      string searchFor = fromDesignCode ? typ.FullName + "+" + designCodeReflectedLevels.Last() : typ.FullName;

      // loop through all types in Oasys.AdSec.IAdsec and cast to IDesignCode if match with above string
      foreach (Type type in Assembly.GetAssembly(typeof(IAdSec)).GetTypes()) {
        if (type.IsInterface && type.Namespace == "Oasys.AdSec.DesignCode") {
          foreach (FieldInfo field in type.GetFields()) {
            if (field.DeclaringType.FullName == searchFor) {
              DesignCode = (IDesignCode)field.GetValue(null);
            }
          }
        }
      }

      if (DesignCode == null) { return false; }
      DesignCodeName = designcodeName.TrimEnd(' ') + " " + designCodeReflectedLevels.Last();
      return true;
    }
  }
}
