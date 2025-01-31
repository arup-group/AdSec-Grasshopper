using System.Collections.Generic;

using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

using TestGrasshopperObjects.Extensions;

using Xunit;

namespace AdSecGHTests.Helpers.Extensions {
  [Collection("GrasshopperFixture collection")]
  public class IPointsTests {
    private IpointsTestComponent _component;
    private readonly string _failToRetrieveDataWarning = "failed";
    private readonly string _convertDataError = "convert";

    public IPointsTests() {
      _component = new IpointsTestComponent();
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
      ComponentTestHelper.SetInput(_component, new List<string>());

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
      ComponentTestHelper.SetInput(_component, new List<string>());

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.Null(result);

      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);

      Assert.Single(runtimeMessages);
      Assert.Contains(runtimeMessages, item => item.Contains(_convertDataError));
      Assert.Contains(runtimeMessages, item => item.Contains("item 0"));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsIPointWhenDataCorrect() {
      ComponentTestHelper.SetInput(_component, new AdSecPointGoo(new Point3d()));

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsIPointWhenDataCorrectButConvertedSinglePoint() {
      ComponentTestHelper.SetInput(_component, new Point3d());

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Remark);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Single(runtimeMessages);
      Assert.Contains(runtimeMessages, item => item.Contains("converted to local point."));
    }

    [Fact]
    public void ReturnsIPointWhenDataCorrectButConvertedManyPoints() {
      var wrapper = new GH_ObjectWrapper(new List<Point3d>() { new Point3d(), new Point3d() });
      ComponentTestHelper.SetInput(_component, wrapper);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Remark);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Single(runtimeMessages);
      Assert.Contains(runtimeMessages, item => item.Contains("List of Points"));
    }
  }
}
