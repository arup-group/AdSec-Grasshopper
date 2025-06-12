using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecCore.Constants;
using AdSecCore.Functions;

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

    public AdSecDesignCode() { }

    public AdSecDesignCode(DesignCode designCode) {
      DesignCode = designCode.IDesignCode;
      DesignCodeName = designCode.DesignCodeName;
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
      string description
        = (string.IsNullOrEmpty(DesignCodeName) ? DesignCode?.ToString() : DesignCodeName.Replace("  ", " "))
        ?? string.Empty;
      description = description.Replace("+", " ");
      return description;
    }

    private void CreateFromReflectedLevels(List<string> designCodeReflectedLevels, bool fromDesignCode = false) {
      var codeType = AdSecFileHelper.GetDesignCodeType(designCodeReflectedLevels);
      DesignCode = AdSecFileHelper.GetDesignCode(designCodeReflectedLevels, codeType, fromDesignCode);

      if (IsValid) {
        DesignCodeName = MaterialHelper.DesignCodeName(DesignCode);
      }
    }

  }
}
