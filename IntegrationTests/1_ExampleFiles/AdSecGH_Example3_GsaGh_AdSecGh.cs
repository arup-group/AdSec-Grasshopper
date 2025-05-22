using System.IO;
using System.Reflection;

using Grasshopper.Kernel;

using Xunit;

namespace IntegrationTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class AdSecGH_Example3_GsaGh_AdSecGh {
    private static GH_Document _document;
    public static GH_Document Document {
      get {
        if (_document == null) {
          _document = OpenDocument();
        }

        return _document;
      }
    }

    [Fact]
    public void NoRuntimeErrorTest() {
      Assert.Empty(Helper.TestNoRuntimeMessagesInDocument(Document, GH_RuntimeMessageLevel.Error));
      Assert.Empty(Helper.TestNoRuntimeMessagesInDocument(Document, GH_RuntimeMessageLevel.Warning));
    }

    private static GH_Document OpenDocument() {
      var thisClass = MethodBase.GetCurrentMethod().DeclaringType;
      string fileName = $"{thisClass.Name}.gh";
      fileName = fileName.Replace(thisClass.Namespace, string.Empty).Replace("Tests", string.Empty);

      string solutiondir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent.FullName;
      string path = Path.Combine(new[] {
        solutiondir,
        "ExampleFiles",
      });

      return Helper.CreateDocument(Path.Combine(path, fileName));
    }
  }
}
