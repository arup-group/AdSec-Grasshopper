using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using AdSecCore.Constants;

using AdSecGH.Parameters;

using Oasys.AdSec;
using Oasys.AdSec.IO.Serialization;

using Rhino.UI;

namespace AdSecGH.Helpers {
  internal class AdSecFile {
    private AdSecFile() { }


    internal static AdSecDesignCode GetDesignCode(string json) {
      // "codes":{"concrete":"EC2_GB_04"}
      string[] jsonSplit = json.Split(new string[] { "\"codes\": {\r\n        \"concrete\": \"" }, StringSplitOptions.None);
      if (jsonSplit.Length == 1) {
        jsonSplit = json.Split(new string[] { "codes\":{\"concrete\":\"" }, StringSplitOptions.None);
      }
      if (jsonSplit.Length < 2) {
        return null;
      }
      string codeName = jsonSplit[1].Split('"')[0];

      if (!AdSecFileHelper.CodesStrings.TryGetValue(codeName, out string codeString)) {
        return null;
      }

      var designCodeLevelsSplit = codeString.Split('+').ToList();

      return new AdSecDesignCode(designCodeLevelsSplit);
    }

    public static List<ISection> ReadSection(string fileName) {
      var sections = new List<ISection>();
      if (!File.Exists(fileName)) {
        return sections;
      }
      string json = File.ReadAllText(fileName);
      var jsonParser = JsonParser.Deserialize(json);
      sections.AddRange(from section in jsonParser.Sections select section);
      return sections;
    }

    internal static List<string> SectionJson(List<AdSecSection> sections, Dictionary<int, List<object>> loads) {
      var jsonStrings = new List<string>();

      if (sections[0].DesignCode == null) {
        throw new ArgumentException("AdSec design code is null");
      }

      var json = new JsonConverter(sections[0].DesignCode);
      for (int sectionId = 0; sectionId < sections.Count; sectionId++) {

        PopulateLoadAndDeformationLists(loads, sectionId, out var adSecload, out var adSecDeformation);

        if (adSecload.Any() && adSecDeformation.Any()) {
          throw new ArgumentException("Only either deformation or load can be specified to a section.");
        }
        try {
          if (adSecload.Any()) {
            jsonStrings.Add(json.SectionToJson(sections[sectionId].Section, adSecload));
          } else {
            jsonStrings.Add(json.SectionToJson(sections[sectionId].Section, adSecDeformation));
          }
        } catch (System.Reflection.TargetInvocationException exception) {
          ParseJsonException(exception);
        }
      }
      return jsonStrings;
    }

    private static void PopulateLoadAndDeformationLists(Dictionary<int, List<object>> loads, int sectionId, out Oasys.Collections.IList<ILoad> adSecload, out Oasys.Collections.IList<IDeformation> adSecDeformation) {
      adSecload = Oasys.Collections.IList<ILoad>.Create();
      adSecDeformation = Oasys.Collections.IList<IDeformation>.Create();

      if (loads.ContainsKey(sectionId)) {

        foreach (var item in loads[sectionId].Where(x => x.GetType() == typeof(AdSecLoadGoo))) {
          adSecload.Add(((AdSecLoadGoo)item).Value);
        }

        foreach (var item in loads[sectionId].Where(x => x.GetType() == typeof(AdSecDeformationGoo))) {
          adSecDeformation.Add(((AdSecDeformationGoo)item).Value);
        }
      }
    }

    private static void ParseJsonException(Exception exception) {
      var messageBuilder = new StringBuilder();
      messageBuilder.Append(exception.Message);
      if (exception.InnerException != null) {
        foreach (var value in exception.InnerException.Data.Values) {
          if (value is IEnumerable<string> messages) {
            foreach (var message in messages) {
              messageBuilder.AppendLine(message);
            }
          }
        }
      }
      string exceptionMessage = messageBuilder.ToString();

      if (exceptionMessage.Contains("definition is not a standard")) {
        messageBuilder.AppendLine(" The AdSec file cannot be created if the section material and rebar grade are not consistent with the design code.");
      }

      throw new InvalidOperationException(messageBuilder.ToString());
    }

    internal static string ModelJson(List<AdSecSection> sections, Dictionary<int, List<object>> loads) {
      if (sections == null || !sections.Any()) {
        return string.Empty;
      }
      var json = SectionJson(sections, loads);
      return CombineJSonStrings(json);
    }

    internal static string SaveFilePath() {
      var saveDialog = new SaveFileDialog {
        Filter = "AdSec File (*.ads)|*.ads|All files (*.*)|*.*",
      };
      return saveDialog.ShowSaveDialog() ? saveDialog.FileName : string.Empty;
    }

    internal static string CombineJSonStrings(List<string> jsonStrings) {
      if (jsonStrings == null || jsonStrings.Count == 0) {
        return null;
      }

      var stringBuilder = new StringBuilder();
      string firstJson = jsonStrings[0].Remove(jsonStrings[0].Length - 2, 2);
      stringBuilder.Append(firstJson);
      for (int i = 1; i < jsonStrings.Count; i++) {
        string nextJson = jsonStrings[i];
        int start = nextJson.IndexOf("components") - 2;
        nextJson = $",{nextJson.Substring(start)}";
        stringBuilder.Append(nextJson.Remove(nextJson.Length - 2, 2));
      }
      stringBuilder.Append(jsonStrings[0].Substring(jsonStrings[0].Length - 2));
      return stringBuilder.ToString();
    }
  }
}
