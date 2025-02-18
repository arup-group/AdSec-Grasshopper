
using AdSecGH.Parameters;

using Grasshopper.Kernel;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.StandardMaterials;

using OasysUnits;
using OasysUnits.Units;

using TestGrasshopperObjects.Extensions;

using Xunit;

namespace AdSecGHTests.Helpers.Extensions {

  [Collection("GrasshopperFixture collection")]
  public class AdSecRebarBundleGooTests {
    private AdSecRebarBundleGooTestComponent _component;
    private readonly string _failToRetrieveDataWarning = "failed";
    private readonly string _convertDataError = "convert";

    public AdSecRebarBundleGooTests() {
      _component = new AdSecRebarBundleGooTestComponent();
    }

    [Fact]
    public void ReturnsWarningWhenInputIsNonOptionalAndNoDataAvailable() {
      _component.Optional = false;
      object obj = null;
      ComponentTestHelper.SetInput(_component, obj);
      object result = ComponentTestHelper.GetOutput(_component);
      Assert.Null(result);

      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);

      Assert.Single(runtimeWarnings);
      Assert.Contains(runtimeWarnings, item => item.Contains(_failToRetrieveDataWarning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsNoMessagesWhenInputIsOptionalAndNoDataAvailable() {
      _component.Optional = true;
      object obj = null;
      ComponentTestHelper.SetInput(_component, obj);

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
      Assert.Contains(runtimeMessages, item => item.Contains(_convertDataError));
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
      Assert.Contains(runtimeMessages, item => item.Contains(_convertDataError));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsRebarBundleWhenDataCorrect() {
      var stressStrainPoint
        = IStressStrainPoint.Create(new Pressure(1, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio));
      IStressStrainCurve curve = ILinearStressStrainCurve.Create(stressStrainPoint);
      var tensionCompressionCurve = ITensionCompressionCurve.Create(curve, curve);
      var length = new Length(1, LengthUnit.Meter);
      var bundle = IBarBundle.Create(IReinforcement.Create(tensionCompressionCurve, tensionCompressionCurve), length,
        1);

      var adSecRebarBundleGoo = new AdSecRebarBundleGoo(bundle);
      ComponentTestHelper.SetInput(_component, adSecRebarBundleGoo);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsRebarBundleWhenDataCorrectAndShowRemark() {
      var topReinforcementLayer = ILayerByBarCount.Create(2,
        IBarBundle.Create(Reinforcement.Steel.IS456.Edition_2000.S415, Length.FromMillimeters(20)));
      var adSecRebarBundleGoo = new AdSecRebarLayerGoo(topReinforcementLayer);
      ComponentTestHelper.SetInput(_component, adSecRebarBundleGoo);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Remark);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Single(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
      Assert.Contains(runtimeMessages, item => item.Contains("RebarSpacing"));
    }
  }
}
