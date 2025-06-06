using System.IO;

using AdSecCore.Functions;

using AdSecGH.Components;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests.Components._3_Rebar {
  [Collection("GrasshopperFixture collection")]
  public class CreateRebarGroupSaveLoadTests {
    public CreateReinforcementGroup component;
    GH_DocumentIO doc;
    private readonly GH_DocumentIO documentIo2;

    public CreateRebarGroupSaveLoadTests() {
      component = new CreateReinforcementGroup();
      doc = new GH_DocumentIO();
      doc.Document = new GH_Document();
      doc.Document.AddObject(component, false);
      documentIo2 = new GH_DocumentIO();
    }

    [Fact]
    public void ShouldBeAbleToSaveAndLoad() {
      var randomPath = GetRandomName();
      Assert.True(doc.SaveQuiet(randomPath));
      Assert.True(documentIo2.Open(randomPath));
    }

    public static string GetRandomName() {
      return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".gh");
    }

    [Fact]
    public void ShouldSaveAndLoadMode() {
      component.SetSelected(0, 1); // Perimeter
      ComponentTesting.ComputeOutputs(component);
      var loadedComponent = SaveAndLoad();
      Assert.Equal(FoldMode.Perimeter, loadedComponent.BusinessComponent.Mode);
    }

    [Fact]
    public void ShouldHaveDefaultMode() {
      Assert.Equal(FoldMode.Template, component.BusinessComponent.Mode);
    }

    [Fact]
    public void ShouldHaveDefaultUnits() {
      Assert.Equal("Meter", component.BusinessComponent.LengthUnitGeometry.ToString());
    }

    [Fact]
    public void ShouldSaveAndLoadUnits() {
      component.SetSelected(1, 1);
      ComponentTesting.ComputeOutputs(component);
      var loadedComponent = SaveAndLoad();
      Assert.Equal("Centimeter", loadedComponent.BusinessComponent.LengthUnitGeometry.ToString());
    }

    private CreateReinforcementGroup SaveAndLoad() {
      var randomPath = GetRandomName();
      doc.SaveQuiet(randomPath);
      documentIo2.Open(randomPath);

      return (CreateReinforcementGroup)documentIo2.Document.FindComponent(component.InstanceGuid);
    }

  }
}
