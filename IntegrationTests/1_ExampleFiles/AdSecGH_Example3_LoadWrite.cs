using System.Collections.Generic;
using System.IO;
using System.Linq;

using Grasshopper.Kernel;

using Xunit;

namespace IntegrationTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class AdSecGH_Example3_LoadWrite {

    private static GH_Document _document;
    public static GH_Document Document {
      get {
        if (_document == null) {
          _document = OpenDocument("AdSecGH_Example3_SaveLoad.gh");
        }

        return _document;
      }
    }

    [Fact]
    public void NoRuntimeErrorTest() {
      var errors = GetAllMessaged(_document, GH_RuntimeMessageLevel.Error);
      var warnings = GetAllMessaged(_document, GH_RuntimeMessageLevel.Error);
      Assert.Empty(errors);
      Assert.Empty(warnings);
    }

    private static GH_Document OpenDocument(string fileName) {
      string solutionDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;
      string path = Path.Combine(new[] {
        solutionDir,
        "ExampleFiles",
      });

      string file = Path.Combine(path, fileName);
      Assert.True(File.Exists(file));
      return Helper.CreateDocument(file);
    }

    public static List<string> GetAllMessaged(
      GH_Document doc, GH_RuntimeMessageLevel runtimeMessageLevel) {
      List<string> messages = new List<string>();
      foreach (var obj in doc.Objects) {
        if (!(obj is GH_Component comp)) {
          continue;
        }

        comp.CollectData();
        comp.Params.Output[0].CollectData();
        comp.Params.Output[0].VolatileData.get_Branch(0);
        var runtimeMessages = comp.RuntimeMessages(runtimeMessageLevel);
        if (runtimeMessages.Any()) {
          messages.AddRange(runtimeMessages);
        }
      }

      return messages;
    }
  }
}
