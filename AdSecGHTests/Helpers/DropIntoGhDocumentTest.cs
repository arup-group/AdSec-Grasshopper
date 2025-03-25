using System.IO;
using System.Threading.Tasks;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class DropIntoGhDocumentTest {
    private readonly GH_Document _document;
    private readonly FakeComponent oasysDropdown;

    public DropIntoGhDocumentTest() {
      string tempPath = Path.Combine(Path.GetTempPath(), "AdSecGHTests", "DropIntoGhDocumentTest.gh");

      oasysDropdown = new FakeComponent();
      _document = new GH_Document();
      _document.AddObject(oasysDropdown, true);
      _document.NewSolution(true);
    }

    [Fact]
    public void ShouldBeAbleToAddComponent() {
      _document.Objects.Clear();
      Assert.True(_document.AddObject(oasysDropdown, false));
    }

    [Fact]
    public void ShouldHaveOnlyOneComponent() {
      Assert.Single(_document.Objects);
    }

    [Fact(Timeout = 300)]
    public async void ShouldLoadDocumentWithFile() {
      string path = $"ShouldLoadDocumentWithFile-{Path.GetRandomFileName()}.gh";
      SaveDocumentAs(path);
      await AssertFileCanBeLoadedTask(path);
    }

    [Fact(Skip = "Events cannot be debugged")]
    public void ShouldHaveNoEvents() {
      Assert.False(_document.RaiseEvents);
    }

    private void SaveDocumentAs(string path) {
      var io = new GH_DocumentIO();
      io.Document = _document;
      io.SaveQuiet(path);
    }

    private async Task AssertFileCanBeLoadedTask(string path) {
      // using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10))) {
      var task = Task.Run(() => {
        AssertFileCanBeLoaded(path);
      }); //, cts.Token);

      await task;
      // }
    }

    private void AssertFileCanBeLoaded(string path) {
      var io = new GH_DocumentIO();
      Assert.True(io.Open(path));
    }

    [Fact(Skip = "No Default Values Set Yet")]
    public void ShouldHaveNoWarnings() {
      TestNoRuntimeMessagesInDocument(_document, GH_RuntimeMessageLevel.Warning);
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      Assert.NotNull(_document);
      TestNoRuntimeMessagesInDocument(_document, GH_RuntimeMessageLevel.Error);
    }

    private static void TestNoRuntimeMessagesInDocument(GH_Document doc, GH_RuntimeMessageLevel runtimeMessageLevel) {
      foreach (var obj in doc.Objects) {
        if (obj is GH_Component comp) {
          comp.CollectData();
          Assert.Empty(comp.RuntimeMessages(runtimeMessageLevel));
        }
      }
    }
  }
}
