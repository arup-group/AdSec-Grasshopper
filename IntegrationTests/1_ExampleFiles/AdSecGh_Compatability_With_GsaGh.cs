using System.Collections.Generic;
using System.IO;
using System.Linq;

using Grasshopper.Kernel;

using Xunit;

namespace IntegrationTests.Components {

  [Collection("GrasshopperFixture collection")]
  public class AdSecGh_Compatability_With_GsaGh {

    private static GH_Document _document;
    public static GH_Document Document {
      get {
        if (_document == null) {
          _document = OpenDocument("AdSecGh_Compatability_With_GsaGh.gh");
        }

        return _document;
      }
    }

    [Fact]
    public void NoRuntimeErrorTest() {
      var errors = GetAllMessages(Document, GH_RuntimeMessageLevel.Error);
      var warnings = GetAllMessages(Document, GH_RuntimeMessageLevel.Warning);
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

    public static List<string> GetAllMessages(GH_Document doc, GH_RuntimeMessageLevel runtimeMessageLevel) {
      List<string> messages = new List<string>();
      foreach (var obj in doc.Objects) {
        if (!(obj is GH_Component comp)) {
          continue;
        }

        comp.CollectData();
        comp.ComputeData();

        var runtimeMessages = comp.RuntimeMessages(runtimeMessageLevel);
        if (runtimeMessages.Any()) {
          messages.AddRange(runtimeMessages);
        }
      }

      return messages;
    }
  }
}
