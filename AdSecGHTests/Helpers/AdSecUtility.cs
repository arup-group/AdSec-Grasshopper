
using System.Collections.Generic;
using System.IO;

using AdSecCore;
using AdSecCore.Builders;

using AdSecGH.Components;
using AdSecGH.Parameters;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.IO.Serialization;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.GH.Helpers;

using Rhino.Geometry;
namespace AdSecGHTests.Helpers {
  public class AdSecUtility {
    private static readonly IDesignCode designCode = IS456.Edition_2000;
    private AdSecUtility() {
    }

    public static ISection CreateSTDRectangularSection() {
      var topRight = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Position(13, 28)).Build();
      var BottomRight = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Position(13, -28)).Build();
      var topLeft = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Position(-13, 28)).Build();
      var BottomLeft = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Position(-13, -28)).Build();
      return new SectionBuilder().WithWidth(30).WithHeight(60).CreateRectangularSection().WithReinforcementGroups(new List<IGroup>() { topRight, BottomRight, topLeft, BottomLeft }).Build();
    }

    public static GH_Component AnalyzeComponent() {
      var component = new Analyse();
      var section = new AdSecSection(CreateSTDRectangularSection(), designCode, "", "", Plane.WorldXY);
      SaveAdSecModel(section, "C:\\AdSecCatch.ads");
      component.SetInputParamAt(0, new AdSecSectionGoo(section));
      return component;
    }

    public static AdSecSolutionGoo GetResult() {
      return (AdSecSolutionGoo)ComponentTestHelper.GetOutput(AnalyzeComponent());
    }

    public static void SaveAdSecModel(AdSecSection section, string path) {
      var jsonStrings = new List<string>();
      var json = new JsonConverter(designCode);
      jsonStrings.Add(json.SectionToJson(section.Section));
      File.WriteAllText(path, CombineJSonStrings(jsonStrings));
    }

    private static string CombineJSonStrings(List<string> jsonStrings) {
      if (jsonStrings == null || jsonStrings.Count == 0) {
        return null;
      }

      string jsonString = jsonStrings[0].Remove(jsonStrings[0].Length - 2, 2);
      for (int i = 1; i < jsonStrings.Count; i++) {
        string jsonString2 = jsonStrings[i];
        int start = jsonString2.IndexOf("components") - 2;
        jsonString2 = $",{jsonString2.Substring(start)}";
        jsonString += jsonString2.Remove(jsonString2.Length - 2, 2);
      }

      jsonString += jsonStrings[0].Substring(jsonStrings[0].Length - 2);

      return jsonString;
    }

    public static bool IsBoundingBoxEqual(BoundingBox actual, BoundingBox expected) {
      var comparer = new DoubleComparer(0.001);
      return comparer.Equals(expected.Min.X, actual.Min.X) && comparer.Equals(expected.Min.Y, actual.Min.Y) &&
         comparer.Equals(expected.Max.X, actual.Max.X) && comparer.Equals(expected.Max.X, actual.Max.X);
    }

  }
}
