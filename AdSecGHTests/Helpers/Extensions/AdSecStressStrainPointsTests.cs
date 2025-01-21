using System.Collections.Generic;

using Grasshopper.Kernel;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

using TestGrasshopperObjects.Extensions;

using Xunit;

namespace AdSecGHTests.Helpers.Extensions {
  [Collection("GrasshopperFixture collection")]
  public class AdSecStressStrainPointsTests {
    private AdSecStressStrainPointsTestComponent _component;
    private readonly string _failToRetrieveDataWarning = "failed";
    private readonly string _convertDataError = "convert";
    private readonly string _atLeast2PointsNeededWarning = "2 points";

    public AdSecStressStrainPointsTests() {
      _component = new AdSecStressStrainPointsTestComponent();
    }

    [Fact]
    public void ReturnsWarningWhenInputIsNonOptionalAndNoDataAvailable() {
      _component.Optional = false;
      object obj = null;
      ComponentTestHelper.SetInput(_component, obj);
      object result = ComponentTestHelper.GetOutput(_component);
      Assert.Null(result);

      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);

      Assert.Equal(2, runtimeWarnings.Count);
      Assert.Contains(_atLeast2PointsNeededWarning, runtimeWarnings[0]);
      Assert.Contains(_failToRetrieveDataWarning, runtimeWarnings[runtimeWarnings.Count - 1]);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsNoMessagesWhenInputIsOptionalAndNoDataAvailable() {
      _component.Optional = true;
      object obj = null;
      ComponentTestHelper.SetInput(_component, obj);

      object result = ComponentTestHelper.GetOutput(_component);
      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);

      Assert.Null(result);
      Assert.Single(runtimeWarnings);
      Assert.Contains(_atLeast2PointsNeededWarning, runtimeWarnings[0]);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsErrorWhenDataIncorrectInputIsOptional() {
      _component.Optional = true;
      ComponentTestHelper.SetInput(_component, new List<string>() {
        "sample",
      });

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.Null(result);

      var runtimeErrors = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);

      Assert.Single(runtimeErrors);
      Assert.Contains(_convertDataError, runtimeErrors[0]);
      Assert.Single(runtimeWarnings);
      Assert.Contains(_atLeast2PointsNeededWarning, runtimeWarnings[0]);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsErrorWhenDataIncorrectInputIsNonOptional() {
      _component.Optional = false;
      ComponentTestHelper.SetInput(_component, new List<string>() {
        "sample",
      });
      object result = ComponentTestHelper.GetOutput(_component);
      Assert.Null(result);

      var runtimeErrors = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);

      Assert.Single(runtimeErrors);
      Assert.Contains(_convertDataError, runtimeErrors[0]);
      Assert.Single(runtimeWarnings);
      Assert.Contains(_atLeast2PointsNeededWarning, runtimeWarnings[0]);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsIStressStrainPointWhenDataCorrect() {
      var curve = IExplicitStressStrainCurve.Create();
      var point = IStressStrainPoint.Create(new Pressure(0, PressureUnit.Pascal), new Strain(1, StrainUnit.Ratio));
      curve.Points.Add(point);
      curve.Points.Add(point);

      ComponentTestHelper.SetInput(_component, curve);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }
  }
}
