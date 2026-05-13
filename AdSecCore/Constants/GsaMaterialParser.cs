using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using AdSecCore.Functions;

using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

namespace AdSecCore.Constants {
  public static class GsaMaterialParser {

    // Maps GSA design code strings to keys in AdSecFileHelper.Codes / CodesStrings
    public static readonly IReadOnlyDictionary<string, string> GsaCodeToAdSecKey
      = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
        // ACI 318
        { "ACI 318-02", "ACI318_02" },
        { "ACI 318-05", "ACI318_05" },
        { "ACI 318-08", "ACI318_08" },
        { "ACI 318-11", "ACI318_11" },
        { "ACI 318-14", "ACI318_14" },
        { "ACI 318M-02", "ACI318M_02" },
        { "ACI 318M-05", "ACI318M_05" },
        { "ACI 318M-08", "ACI318M_08" },
        { "ACI 318M-11", "ACI318M_11" },
        { "ACI 318M-14", "ACI318M_14" },
        // AASHTO
        { "AASHTO 17", "AASHTO_17" },
        { "AASHTO 17M", "AASHTO_17M" },
        // AS 3600
        { "AS3600-01", "AS3600_01" },
        { "AS3600-09", "AS3600_09" },
        { "AS3600-18", "AS3600_18" },
        // BS 8110
        { "BS8110-85", "BS8110_85" },
        { "BS8110-97", "BS8110_97" },
        { "BS8110-05", "BS8110_05" },
        // BS 5400
        { "BS5400", "BS5400" },
        // CSA
        { "CSA A23.3-04", "CSA_A23_3_04" },
        { "CSA A23.3-14", "CSA_A23_3_14" },
        { "CSA S6-14", "CSA_S6_14" },
        // EC2 Part 1-1 – no national annex
        { "EC2-1-1", "EC2_04" },
        // EC2 Part 1-1 – national annexes
        { "EC2-1-1 (CY)", "EC2_CY_04" },
        { "EC2-1-1 (DE)", "EC2_DE_04" },
        { "EC2-1-1 (DK)", "EC2_DK_04" },
        { "EC2-1-1 (ES)", "EC2_ES_04" },
        { "EC2 (FI)", "EC2_FI_04" },
        { "EC2 (FR)", "EC2_FR_04" },
        { "EC2 (GB)", "EC2_GB_04" },
        { "EC2 (IE)", "EC2_IE_04" },
        { "EC2 (IT)", "EC2_IT_04" },
        { "EC2 (NL)", "EC2_NL_04" },
        { "EC2 (NO)", "EC2_NO_04" },
        { "EC2 (PL)", "EC2_PL_04" },
        { "EC2-1-1 (SG)", "EC2_SG_04" },
        // BS EC2 PD (GB national annex supplement)
        { "BS EC2/PD:06", "BS_EC2_PD_06" },
        { "BS EC2/PD:10", "BS_EC2_PD_10" },
        // EC2 Part 2 – no national annex
        { "EC2-2", "EC2_2_05" },
        // EC2 Part 2 – national annexes
        { "EC2-2 (DE)", "EC2_2_DE_05" },
        { "EC2-2 (DK)", "EC2_2_DK_05" },
        { "EC2-2 (ES)", "EC2_2_ES_05" },
        { "EC2-2 (FR)", "EC2_2_FR_05" },
        { "EC2-2 (GB)", "EC2_2_GB_05" },
        { "EC2-2 (IE)", "EC2_2_IE_05" },
        { "EC2-2 (IT)", "EC2_2_IT_05" },
        { "EC2-2 (NL)", "EC2_2_NL_05" },
        { "EC2-2 (SG)", "EC2_2_SG_05" },
        // HK
        { "HK CoP (87)", "HKCP_87" },
        { "HK CoP (04)", "HKCP_04" },
        { "HK CoP (07)", "HKCP_07" },
        { "HK CoP (13)", "HKCP_13" },
        { "HK SDM (02)", "HKSDM_02" },
        { "HK SDM (13)", "HKSDM_13" },
        // India
        { "IRS Bridge (97)", "IRS_BRIDGE_97" },
        { "IS 456", "IS456_2000" },
        { "IRC:112", "IRC112_2011" },
      };

    private const string FallbackCodeKey = "EC2_04";

    /// <summary>
    /// Tries to parse a GSA material string into an AdSec catalogue <see cref="MaterialDesign"/>.
    /// Supports only Concrete materials. Expected format: "GSA Material (&lt;code&gt; Concrete &lt;grade&gt;)"
    /// When the design code or grade cannot be matched exactly, falls back to EN1992 Part1-1 2004
    /// NoNationalAnnex with the best matching grade (C30_37 if grade is absent).
    /// </summary>
    public static bool TryParseConcreteMaterial(string gsaString, out MaterialDesign material)
      => TryParseConcreteMaterial(gsaString, out material, out _);

    /// <inheritdoc cref="TryParseConcreteMaterial(string, out MaterialDesign)"/>
    /// <param name="warning">Non-null when a fallback was applied; describes what was substituted.</param>
    public static bool TryParseConcreteMaterial(string gsaString, out MaterialDesign material, out string warning) {
      material = null;
      warning = null;

      if (string.IsNullOrWhiteSpace(gsaString)) {
        return false;
      }

      var match = Regex.Match(gsaString, @"\((.+)\)", RegexOptions.None, TimeSpan.FromSeconds(1));
      if (!match.Success) {
        return false;
      }

      string content = match.Groups[1].Value.Trim();

      int concreteIdx = content.IndexOf("Concrete", StringComparison.OrdinalIgnoreCase);
      if (concreteIdx < 0) {
        return false;
      }

      string codeString = content.Substring(0, concreteIdx).Trim();
      string gradeString = content.Substring(concreteIdx + "Concrete".Length).Trim();

      if (string.IsNullOrEmpty(gradeString)) {
        return false;
      }

      if (string.IsNullOrEmpty(codeString)) {
        return TryBuildFallback(gradeString,
          "No design code found before 'Concrete' in the input. Falling back to EN1992 Part1-1 2004 NoNationalAnnex.",
          out material, out warning);
      }

      if (!GsaCodeToAdSecKey.TryGetValue(codeString, out string codeKey)
          || !AdSecFileHelper.Codes.TryGetValue(codeKey, out var designCode)
          || !AdSecFileHelper.CodesStrings.TryGetValue(codeKey, out string designCodeName)) {
        return TryBuildFallback(gradeString,
          $"Design code '{codeString}' is not supported by AdSec. Falling back to EN1992 Part1-1 2004 NoNationalAnnex.",
          out material, out warning);
      }

      var codeLevels = new List<string>(designCodeName.Split('+'));
      var catalogueType = NavigateConcreteCatalogue(codeLevels);
      if (catalogueType == null) {
        return TryBuildFallback(gradeString,
          $"Could not navigate the AdSec catalogue for '{codeString}'. Falling back to EN1992 Part1-1 2004 NoNationalAnnex.",
          out material, out warning);
      }

      var gradeField = FindGradeFromString(catalogueType, gradeString);
      if (gradeField == null) {
        return TryBuildFallback(gradeString,
          $"Grade '{gradeString}' was not found in the '{codeString}' catalogue. Falling back to EN1992 Part1-1 2004 NoNationalAnnex.",
          out material, out warning);
      }

      material = new MaterialDesign {
        Material = (IMaterial)gradeField.GetValue(null),
        GradeName = gradeField.Name,
        DesignCode = new DesignCode {
          IDesignCode = designCode,
          DesignCodeName = designCodeName,
        },
      };

      return true;
    }

    private static bool TryBuildFallback(string gradeString, string reason, out MaterialDesign material, out string warning) {
      material = null;
      warning = null;

      if (!AdSecFileHelper.Codes.TryGetValue(FallbackCodeKey, out var fallbackCode)
          || !AdSecFileHelper.CodesStrings.TryGetValue(FallbackCodeKey, out string fallbackCodeName)) {
        return false;
      }

      var codeLevels = new List<string>(fallbackCodeName.Split('+'));
      var catalogueType = NavigateConcreteCatalogue(codeLevels);
      if (catalogueType == null) {
        return false;
      }

      var gradeField = FindGradeFromString(catalogueType, gradeString)
        ?? FindGrade(catalogueType, "C30_37")
        ?? GetFields(catalogueType).Values.FirstOrDefault();

      if (gradeField == null) {
        return false;
      }

      var embeddedGrade = Regex.Match(gradeString, @"C\d+[/_]\d+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
      string effectiveGradeString = embeddedGrade.Success ? embeddedGrade.Value : gradeString;

      string gradeNote = NormalizeGrade(gradeField.Name) == NormalizeGrade(effectiveGradeString)
        ? null
        : $" Grade '{gradeString}' not found; using '{gradeField.Name}'.";

      warning = reason + gradeNote;

      material = new MaterialDesign {
        Material = (IMaterial)gradeField.GetValue(null),
        GradeName = gradeField.Name,
        DesignCode = new DesignCode {
          IDesignCode = fallbackCode,
          DesignCodeName = fallbackCodeName,
        },
      };

      return true;
    }

    private static Type NavigateConcreteCatalogue(List<string> codeLevels) {
      var currentTypes = AdSecFileHelper.ReflectNestedTypes(typeof(Concrete));
      Type currentType = null;

      foreach (string level in codeLevels) {
        if (!currentTypes.TryGetValue(level, out currentType)) {
          return null;
        }

        currentTypes = AdSecFileHelper.ReflectNestedTypes(currentType);
      }

      return currentType;
    }

    /// <summary>
    /// Tries to find a grade in the catalogue by the raw grade string.
    /// First attempts a direct match, then scans for an embedded concrete grade token
    /// such as "C25/30" or "C25_30" within the string.
    /// </summary>
    private static FieldInfo FindGradeFromString(Type catalogueType, string gradeString) {
      var direct = FindGrade(catalogueType, gradeString);
      if (direct != null) {
        return direct;
      }

      var embedded = Regex.Match(gradeString, @"C\d+[/_]\d+", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
      if (embedded.Success) {
        return FindGrade(catalogueType, embedded.Value);
      }

      return null;
    }

    private static FieldInfo FindGrade(Type catalogueType, string gradeString) {
      var fields = GetFields(catalogueType);

      string normalizedInput = NormalizeGrade(gradeString);
      foreach (var kvp in fields) {
        if (NormalizeGrade(kvp.Key) == normalizedInput) {
          return kvp.Value;
        }
      }

      string inputNumeric = ExtractFirstNumeric(gradeString);
      if (!string.IsNullOrEmpty(inputNumeric)) {
        foreach (var kvp in fields) {
          if (ExtractFirstNumeric(kvp.Key) == inputNumeric) {
            return kvp.Value;
          }
        }
      }

      return null;
    }

    private static Dictionary<string, FieldInfo> GetFields(Type type) {
      var result = new Dictionary<string, FieldInfo>();

      foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public)) {
        if (!result.ContainsKey(field.Name)) {
          result[field.Name] = field;
        }
      }

      foreach (var iface in type.GetInterfaces()) {
        foreach (var field in iface.GetFields()) {
          if (!result.ContainsKey(field.Name)) {
            result[field.Name] = field;
          }
        }
      }

      return result;
    }

    private static string NormalizeGrade(string grade) {
      return Regex.Replace(grade, "[^a-zA-Z0-9]", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1))
                  .ToLowerInvariant();
    }

    private static string ExtractFirstNumeric(string s) {
      var m = Regex.Match(s, @"\d+", RegexOptions.None, TimeSpan.FromSeconds(1));
      return m.Success ? m.Value : string.Empty;
    }
  }
}
