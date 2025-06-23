using AdSecCore.Functions;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Properties;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components._02_Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateCustomMaterialTests {
    private readonly CreateCustomMaterial _component;

    public CreateCustomMaterialTests() {
      _component = new CreateCustomMaterial();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.CreateCustomMaterial));
    }

    [Fact]
    public void ShouldHave6InputsByDefault() {
      Assert.Equal(6, _component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHave5InputsOnNonConcreteMaterials() {
      _component.SetSelected(0, 1);
      Assert.Equal(5, _component.Params.Input.Count);
    }

    [Fact]
    public void ShouldUndoDropdownChange() {
      var doc = new GH_Document();
      var undoStateChanged = false;
      doc.UndoStateChanged += (sender, args) => {
        undoStateChanged = true;
      };
      doc.AddObject(_component, true);
      _component.SetSelected(0, 1);
      doc.Undo();
      Assert.True(undoStateChanged);
    }
  }
}
