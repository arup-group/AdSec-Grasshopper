using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecGH.Helpers;

using AdSecGHCore;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

namespace AdSecGH.Parameters {
  /// <summary>
  /// AdSec DesignCode class, this class defines the basic properties and methods for any AdSec DesignCode
  /// </summary>
  public class AdSecDesignCode {
    public IDesignCode DesignCode { get; set; }
    public string DesignCodeName { get; set; }
    public bool IsValid => DesignCode != null;

    public AdSecDesignCode() {
    }

    public AdSecDesignCode(IDesignCode designCode, string designCodeName) {
      DesignCode = designCode;
      DesignCodeName = designCodeName;
    }

    internal AdSecDesignCode(FieldInfo fieldDesignCode) {
      string designCodeReflectedLevels
        = fieldDesignCode?.DeclaringType?.FullName?.Replace("Oasys.AdSec.DesignCode.", "");
      if (designCodeReflectedLevels == null) {
        const string Message
          = "Unable to retrieve the full type name from the provided FieldInfo. Ensure that the FieldInfo belongs to a valid type within the 'Oasys.AdSec.DesignCode' namespace.";
        throw new ArgumentNullException(nameof(fieldDesignCode), Message);
      }
      designCodeReflectedLevels = $"{designCodeReflectedLevels}+{fieldDesignCode.Name}";
      var designCodeLevelsSplit = designCodeReflectedLevels.Split('+').ToList();
      CreateFromReflectedLevels(designCodeLevelsSplit);
    }


    internal AdSecDesignCode(List<string> designCodeReflectedLevels) {
      CreateFromReflectedLevels(designCodeReflectedLevels);
    }

    public AdSecDesignCode Duplicate() {
      return IsValid ? (AdSecDesignCode)MemberwiseClone() : null;
    }

    public override string ToString() {
      return (string.IsNullOrEmpty(DesignCodeName) ? DesignCode?.ToString() : DesignCodeName.Replace("  ", " ")) ?? string.Empty;
    }

    private void CreateFromReflectedLevels(List<string> designCodeReflectedLevels, bool fromDesignCode = false) {
      var codeType = GetDesignCodeType(designCodeReflectedLevels);
      GetDesignCode(designCodeReflectedLevels, codeType, fromDesignCode);

      if (IsValid) {
        DesignCodeName = MaterialHelper.DesignCodeName(DesignCode);
      }
    }

    private void GetDesignCode(List<string> designCodeReflectedLevels, Type codeType, bool fromDesignCode) {
      if (codeType == null) {
        return;
      }
      // we need to find the right type Interface under Oasys.AdSec.IAdsec in order to cast to IDesignCode
      // the string to search for depends on where we call this function from, if we come from an IMaterial type
      // we can simply use the full name but if from IDesignCode we need to add the name of the code with a +
      string searchFor = codeType.Namespace;
      for (int i = 0; i < designCodeReflectedLevels.Count; i++) {
        searchFor = $"{searchFor}{(i == 0 ? "." : "+")}{designCodeReflectedLevels[i]}";
      }

      foreach (var field in
      // loop through all types in Oasys.AdSec.IAdsec and cast to IDesignCode if match with above string
      from Type type in Assembly.GetAssembly(typeof(IAdSec)).GetTypes() where type.IsInterface && type.Namespace == "Oasys.AdSec.DesignCode" from FieldInfo field in type.GetFields() where (fromDesignCode ? field.ReflectedType.FullName : $"{field.ReflectedType.FullName}+{field.Name}") == searchFor select field) {
        DesignCode = (IDesignCode)field.GetValue(null);
      }
    }

    private static Type GetDesignCodeType(List<string> designCodeReflectedLevels) {
      // Get all DesignCodes in DLL under namespace
      Dictionary<string, Type> designCodeKVP = ReflectionHelper.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");
      // Loop through DesignCodes types to find the DesignCode type matching our input list of levels
      Type designCodeType = null;
      for (int i = 0; i < designCodeReflectedLevels.Count - 1; i++) {
        designCodeKVP.TryGetValue(designCodeReflectedLevels[i], out designCodeType);
        if (designCodeType == null) {
          return null;
        }
        designCodeKVP = ReflectionHelper.ReflectNestedTypes(designCodeType);
      }

      if (designCodeReflectedLevels.Count != 1) {
        return designCodeType;
      }

      designCodeKVP.TryGetValue(designCodeReflectedLevels[0], out designCodeType);
      return designCodeType ?? null;
    }
  }
}
