using AdSecCore;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Components._3_Rebar;
using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.GH.Helpers;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Commands;

using Xunit;

namespace AdSecGHTests.Components._02_Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateStressStrainPointTests {
    private readonly CreateStressStrainPoint _component;

    public CreateStressStrainPointTests() {
      _component = new CreateStressStrainPoint();
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.StressStrainPt));
    }

    [Fact]
    public void ComputeWithValidInputsShouldCreatePoint() {
      SetStrainInput();
      SetStressInput();
      AssertStressStrainPointOutput(_component);
    }

    private void AssertStressStrainPointOutput(CreateStressStrainPoint component) {
      var result = (AdSecStressStrainPointGoo)ComponentTestHelper.GetOutput(component);
      Assert.NotNull(result);
      Assert.Equal(0.002, result.StressStrainPoint.Strain.As(StrainUnit.Ratio), new DoubleComparer());
      Assert.Equal(30000, result.StressStrainPoint.Stress.As(PressureUnit.Kilopascal), new DoubleComparer());
    }

    [Fact]
    public void RectangleModelShouldUseSavedMode() {
      SetStrainInput();
      SetStressInput();
      var doc = new GH_DocumentIO();
      doc.Document = new GH_Document();
      doc.Document.AddObject(_component, false);
      var randomPath = CreateRebarGroupSaveLoadTests.GetRandomName();
      doc.SaveQuiet(randomPath);
      doc.Open(randomPath);
      doc.Document.NewSolution(true);
      var component = (CreateStressStrainPoint)doc.Document.FindComponent(_component.InstanceGuid);
      ComponentTestHelper.SetInput(component, 30000, 1);
      ComponentTestHelper.SetInput(component, 0.002, 0);
      AssertStressStrainPointOutput(component);
    }

    private void SetStressInput() {
      //Kilo pascal
      _component.SetSelected(1, 1);
      ComponentTestHelper.SetInput(_component, 30000, 1);
    }

    private void SetStrainInput() {
      //millistrain
      _component.SetSelected(0, 2);
      ComponentTestHelper.SetInput(_component, 0.002, 0);
    }
  }
}
