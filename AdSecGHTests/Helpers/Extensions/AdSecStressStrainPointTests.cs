using Grasshopper.Kernel;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

using TestGrasshopperObjects.Extensions;

using Xunit;

namespace AdSecGHTests.Helpers.Extensions {
  [Collection("GrasshopperFixture collection")]
  public class AdSecStressStrainPointTests {
    private AdSecStressStrainPointTestComponent _component;

    public AdSecStressStrainPointTests() {
      _component = new AdSecStressStrainPointTestComponent();
    }

    [Fact]
    public void ReturnsWarningWhenInputIsNonOptionalAndNoDataAvailable() {
      _component.Optional = false;
      object obj = null;
      ComponentTestHelper.SetInput(_component, obj, 0);
      object result = ComponentTestHelper.GetOutput(_component);
      Assert.Null(result);

      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);

      Assert.Single(runtimeWarnings);
      Assert.Contains("failed", runtimeWarnings[runtimeWarnings.Count - 1]);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsNoMessagesWhenInputIsOptionalAndNoDataAvailable() {
      _component.Optional = true;
      object obj = null;
      ComponentTestHelper.SetInput(_component, obj, 0);

      object result = ComponentTestHelper.GetOutput(_component);

      Assert.Null(result);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsErrorWhenDataIncorrectInputIsOptional() {
      _component.Optional = true;
      ComponentTestHelper.SetInput(_component, string.Empty);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.Null(result);

      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);

      Assert.Single(runtimeMessages);
      Assert.Contains("convert", runtimeMessages[runtimeMessages.Count - 1]);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsErrorWhenDataIncorrectInputIsNonOptional() {
      _component.Optional = false;
      ComponentTestHelper.SetInput(_component, string.Empty);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.Null(result);

      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);

      Assert.Single(runtimeMessages);
      Assert.Contains("convert", runtimeMessages[runtimeMessages.Count - 1]);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsIStressStrainPointWhenDataCorrect() {
      var point = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(2, StrainUnit.Ratio));

      ComponentTestHelper.SetInput(_component, point);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }
  }
}
