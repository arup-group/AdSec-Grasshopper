using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

namespace AdSecCore.Constants {

  public static class AdSecFileHelper {

    private static Assembly _adSecAPI;

    private static Assembly AdSecAPI() {
      if (_adSecAPI == null) {
#pragma warning disable S3885 // Avoid using Assembly.LoadFrom
        _adSecAPI = Assembly.Load("AdSec_API.dll");
#pragma warning restore S3885
      }

      return _adSecAPI;
    }

    private static Dictionary<string, Type> ReflectAdSecNamespace(string @namespace) {
      var adsecAPI = AdSecAPI();
      var q = from t in adsecAPI.GetTypes() where t.IsInterface && t.Namespace == @namespace select t;
      var dict = new Dictionary<string, Type>();
      foreach (var typ in q) {
        if ($"{@namespace}.{typ.Name}" == typ.FullName) {
          dict.Add(typ.Name, typ);
        }
      }

      return dict;
    }

    public static Dictionary<string, Type> ReflectNestedTypes(Type type) {
      var dict = new Dictionary<string, Type>();

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

    public static Type GetDesignCodeType(List<string> designCodeReflectedLevels) {
      // Get all DesignCodes in DLL under namespace
      Dictionary<string, Type> designCodeKVP = ReflectAdSecNamespace("Oasys.AdSec.DesignCode");
      // Loop through DesignCodes types to find the DesignCode type matching our input list of levels
      Type designCodeType = null;
      for (int i = 0; i < designCodeReflectedLevels.Count - 1; i++) {
        designCodeKVP.TryGetValue(designCodeReflectedLevels[i], out designCodeType);
        if (designCodeType == null) {
          return null;
        }

        designCodeKVP = ReflectNestedTypes(designCodeType);
      }

      if (designCodeReflectedLevels.Count != 1) {
        return designCodeType;
      }

      designCodeKVP.TryGetValue(designCodeReflectedLevels[0], out designCodeType);
      return designCodeType ?? null;
    }

    public static bool TryGetConcreteCode(string json, out string codeName) {
      codeName = null;
      try {
        using (var doc = JsonDocument.Parse(json)) {
          if (doc.RootElement.TryGetProperty("codes", out var codes)
            && codes.TryGetProperty("concrete", out var concrete)) {
            codeName = concrete.GetString();
            return true;
          }
        }
      } catch {
        // Ignore parsing errors
      }

      return false;
    }

    public static bool ProcessJsonIntoDesignCodeParts(string json, out List<string> designCodeLevelsSplit) {
      string codeName;
      if (TryGetConcreteCode(json, out var codeNameJson)) {
        codeName = codeNameJson;
      } else {
        string[] jsonSplit = json.Split(new[] { "\"codes\": {\r\n        \"concrete\": \"" }, StringSplitOptions.None);
        if (jsonSplit.Length == 1) {
          jsonSplit = json.Split(new[] { "codes\":{\"concrete\":\"" }, StringSplitOptions.None);
        }

        if (jsonSplit.Length < 2) {
          designCodeLevelsSplit = null;
          return false;
        }

        codeName = jsonSplit[1].Split('"')[0];
      }

      if (!CodesStrings.TryGetValue(codeName, out string codeString)) {
        designCodeLevelsSplit = null;
        return false;
      }

      designCodeLevelsSplit = codeString.Split('+').ToList();
      return true;
    }

    public static IDesignCode GetDesignCode(
      List<string> designCodeReflectedLevels, Type codeType, bool fromDesignCode) {
      if (codeType == null) {
        return null;
      }

      // we need to find the right type Interface under Oasys.AdSec.IAdsec in order to cast to IDesignCode
      // the string to search for depends on where we call this function from, if we come from an IMaterial type
      // we can simply use the full name but if from IDesignCode we need to add the name of the code with a +
      string searchFor = codeType.Namespace;
      for (int i = 0; i < designCodeReflectedLevels.Count; i++) {
        searchFor = $"{searchFor}{(i == 0 ? "." : "+")}{designCodeReflectedLevels[i]}";
      }

      var availableFields = from Type type in Assembly.GetAssembly(typeof(IAdSec)).GetTypes()
                            where type.IsInterface && type.Namespace == "Oasys.AdSec.DesignCode" from FieldInfo field in type.GetFields()
                            where (fromDesignCode ? field.ReflectedType.FullName : $"{field.ReflectedType.FullName}+{field.Name}")
                              == searchFor select field;

      var listOfFields = availableFields.ToList();
      if (!listOfFields.Any()) {
        return null;
      }

      return (IDesignCode)listOfFields[0].GetValue(null);
    }

    public static Dictionary<string, IDesignCode> Codes { get; } = new Dictionary<string, IDesignCode>() {
      { "ACI318M_02", ACI318.Edition_2002.Metric },
      { "ACI318M_05", ACI318.Edition_2005.Metric },
      { "ACI318M_08", ACI318.Edition_2008.Metric },
      { "ACI318M_11", ACI318.Edition_2011.Metric },
      { "ACI318M_14", ACI318.Edition_2014.Metric },
      { "ACI318_02", ACI318.Edition_2002.US },
      { "ACI318_05", ACI318.Edition_2005.US },
      { "ACI318_08", ACI318.Edition_2008.US },
      { "ACI318_11", ACI318.Edition_2011.US },
      { "ACI318_14", ACI318.Edition_2014.US },
      { "AASHTO_17", AASHTO.Edition_2017.US },
      { "AASHTO_17M", AASHTO.Edition_2017.Metric },
      { "AS3600_01", AS3600.Edition_2001 },
      { "AS3600_09", AS3600.Edition_2009 },
      { "AS3600_18", AS3600.Edition_2018 },
      { "BS8110_85", BS8110.Edition_1985 },
      { "BS8110_97", BS8110.Edition_1997 },
      { "BS8110_05", BS8110.Edition_2005 },
      { "BS5400", BS5400.Edition_1990 },
      { "BS5400_IAN70_06", BS5400.Edition_2006 },
      { "CSA_A23_3_04", CSA.A23_3.Edition_2004 },
      { "CSA_A23_3_14", CSA.A23_3.Edition_2014 },
      { "CSA_S6_14", CSA.S6.Edition_2014 },
      { "EC2_04", EN1992.Part1_1.Edition_2004.NationalAnnex.NoNationalAnnex },
      { "EC2_CY_04", EN1992.Part1_1.Edition_2004.NationalAnnex.CY.Edition_2019 },
      { "EC2_DE_04", EN1992.Part1_1.Edition_2004.NationalAnnex.DE.Edition_2013 },
      { "EC2_DK_04", EN1992.Part1_1.Edition_2004.NationalAnnex.DK.Edition_2013 },
      { "EC2_ES_04", EN1992.Part1_1.Edition_2004.NationalAnnex.ES.Edition_2013 },
      { "EC2_FI_04", EN1992.Part1_1.Edition_2004.NationalAnnex.FI.Edition_2017 },
      { "EC2_FR_04", EN1992.Part1_1.Edition_2004.NationalAnnex.FR.Edition_2016 },
      { "EC2_GB_04", EN1992.Part1_1.Edition_2004.NationalAnnex.GB.Edition_2014 },
      { "EC2_IE_04", EN1992.Part1_1.Edition_2004.NationalAnnex.IE.Edition_2010 },
      { "EC2_IT_04", EN1992.Part1_1.Edition_2004.NationalAnnex.IT.Edition_2007 },
      { "EC2_NL_04", EN1992.Part1_1.Edition_2004.NationalAnnex.NL.Edition_2020 },
      { "EC2_NO_04", EN1992.Part1_1.Edition_2004.NationalAnnex.NO.Edition_2010 },
      { "EC2_PL_04", EN1992.Part1_1.Edition_2004.NationalAnnex.PL.Edition_2008 },
      { "EC2_SG_04", EN1992.Part1_1.Edition_2004.NationalAnnex.SG.Edition_2008 },
      { "BS_EC2_PD_06", EN1992.Part1_1.Edition_2004.NationalAnnex.GB.PD6687.Edition_2006 },
      { "BS_EC2_PD_10", EN1992.Part1_1.Edition_2004.NationalAnnex.GB.PD6687.Edition_2010 },
      { "EC2_2_05", EN1992.Part2.Edition_2005.NationalAnnex.NoNationalAnnex },
      { "EC2_2_DE_05", EN1992.Part2.Edition_2005.NationalAnnex.DE.Edition_2013 },
      { "EC2_2_DK_05", EN1992.Part2.Edition_2005.NationalAnnex.DK.Edition_2015 },
      { "EC2_2_ES_05", EN1992.Part2.Edition_2005.NationalAnnex.ES.Edition_2013 },
      { "EC2_2_FR_05", EN1992.Part2.Edition_2005.NationalAnnex.FR.Edition_2007 },
      { "EC2_2_GB_05", EN1992.Part2.Edition_2005.NationalAnnex.GB.Edition_2007 },
      { "EC2_2_IE_05", EN1992.Part2.Edition_2005.NationalAnnex.IE.Edition_2005 },
      { "EC2_2_IT_05", EN1992.Part2.Edition_2005.NationalAnnex.IT.Edition_2006 },
      { "EC2_2_NL_05", EN1992.Part2.Edition_2005.NationalAnnex.NL.Edition_2016 },
      { "EC2_2_SG_05", EN1992.Part2.Edition_2005.NationalAnnex.SG.Edition_2012 },
      { "HKCP_87", HK.CP.Edition_1987 },
      { "HKCP_04", HK.CP.Edition_2004 },
      { "HKCP_07", HK.CP.Edition_2007 },
      { "HKCP_13", HK.CP.Edition_2013 },
      { "HKSDM_02", HK.SDM.Edition_2002 },
      { "HKSDM_13", HK.SDM.Edition_2013 },
      { "IRS_BRIDGE_97", IRS.Edition_1997 },
      { "IS456_2000", IS456.Edition_2000 },
      { "IRC112_2011", IRC112.Edition_2011 }
    };

    public static Dictionary<string, string> CodesStrings { get; } = new Dictionary<string, string>() {
      { "ACI318M_02", "ACI318+Edition_2002+Metric" },
      { "ACI318M_05", "ACI318+Edition_2005+Metric" },
      { "ACI318M_08", "ACI318+Edition_2008+Metric" },
      { "ACI318M_11", "ACI318+Edition_2011+Metric" },
      { "ACI318M_14", "ACI318+Edition_2014+Metric" },
      { "ACI318_02", "ACI318+Edition_2002+US" },
      { "ACI318_05", "ACI318+Edition_2005+US" },
      { "ACI318_08", "ACI318+Edition_2008+US" },
      { "ACI318_11", "ACI318+Edition_2011+US" },
      { "ACI318_14", "ACI318+Edition_2014+US" },
      { "AASHTO_17", "AASHTO+Edition_2017+US" },
      { "AASHTO_17M", "AASHTO+Edition_2017+Metric" },
      { "AS3600_01", "AS3600+Edition_2001" },
      { "AS3600_09", "AS3600+Edition_2009" },
      { "AS3600_18", "AS3600+Edition_2018" },
      { "BS8110_85", "BS8110+Edition_1985" },
      { "BS8110_97", "BS8110+Edition_1997" },
      { "BS8110_05", "BS8110+Edition_2005" },
      { "BS5400", "BS5400+Edition_1990" },
      { "BS5400_IAN70_06", "BS5400+Edition_2006" },
      { "CSA_A23_3_04", "CSA+A23_3+Edition_2004" },
      { "CSA_A23_3_14", "CSA+A23_3+Edition_2014" },
      { "CSA_S6_14", "CSA+S6+Edition_2014" },
      { "EC2_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+NoNationalAnnex" },
      { "EC2_CY_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+CY+Edition_2019" },
      { "EC2_DE_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+DE+Edition_2013" },
      { "EC2_DK_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+DK+Edition_2013" },
      { "EC2_ES_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+ES+Edition_2013" },
      { "EC2_FI_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+FI+Edition_2017" },
      { "EC2_FR_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+FR+Edition_2016" },
      { "EC2_GB_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+GB+Edition_2014" },
      { "EC2_IE_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+IE+Edition_2010" },
      { "EC2_IT_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+IT+Edition_2007" },
      { "EC2_NL_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+NL+Edition_2020" },
      { "EC2_NO_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+NO+Edition_2010" },
      { "EC2_PL_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+PL+Edition_2008" },
      { "EC2_SG_04", "EN1992+Part1_1+Edition_2004+NationalAnnex+SG+Edition_2008" },
      { "BS_EC2_PD_06", "EN1992+Part1_1+Edition_2004+NationalAnnex+GB+PD6687+Edition_2006" },
      { "BS_EC2_PD_10", "EN1992+Part1_1+Edition_2004+NationalAnnex+GB+PD6687+Edition_2010" },
      { "EC2_2_05", "EN1992+Part2+Edition_2005+NationalAnnex+NoNationalAnnex" },
      { "EC2_2_DE_05", "EN1992+Part2+Edition_2005+NationalAnnex+DE+Edition_2013" },
      { "EC2_2_DK_05", "EN1992+Part2+Edition_2005+NationalAnnex+DK+Edition_2015" },
      { "EC2_2_ES_05", "EN1992+Part2+Edition_2005+NationalAnnex+ES+Edition_2013" },
      { "EC2_2_FR_05", "EN1992+Part2+Edition_2005+NationalAnnex+FR+Edition_2007" },
      { "EC2_2_GB_05", "EN1992+Part2+Edition_2005+NationalAnnex+GB+Edition_2007" },
      { "EC2_2_IE_05", "EN1992+Part2+Edition_2005+NationalAnnex+IE+Edition_2005" },
      { "EC2_2_IT_05", "EN1992+Part2+Edition_2005+NationalAnnex+IT+Edition_2006" },
      { "EC2_2_NL_05", "EN1992+Part2+Edition_2005+NationalAnnex+NL+Edition_2016" },
      { "EC2_2_SG_05", "EN1992+Part2+Edition_2005+NationalAnnex+SG+Edition_2012" },
      { "HKCP_87", "HK+CP+Edition_1987" },
      { "HKCP_04", "HK+CP+Edition_2004" },
      { "HKCP_07", "HK+CP+Edition_2007" },
      { "HKCP_13", "HK+CP+Edition_2013" },
      { "HKSDM_02", "HK+SDM+Edition_2002" },
      { "HKSDM_13", "HK+SDM+Edition_2013" },
      { "IRS_BRIDGE_97", "IRS+Edition_1997" },
      { "IS456_2000", "IS456+Edition_2000" },
      { "IRC112_2011", "IRC112+Edition_2011" }
    };
  }
}
