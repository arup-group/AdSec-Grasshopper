using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using OasysGH.Parameters;

using OasysUnits;
using OasysUnits.Units;

using TestGrasshopperObjects.Extensions;

using Xunit;

namespace AdSecGHTests.Helpers.Extensions {
  [Collection("GrasshopperFixture collection")]
  public class GetCoversTests {
    private GetCoversTestComponent _component;
    private readonly string _failToRetrieveDataWarning = "failed";

    public GetCoversTests() {
      _component = new GetCoversTestComponent();
    }

    [Fact]
    public void ReturnsWarningWhenNoDataAvailable() {
      ComponentTestHelper.SetInput(_component, new GH_ObjectWrapper(new object()));
      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      var runtimeWarnings = _component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);

      Assert.Contains(runtimeWarnings, item => item.Contains(_failToRetrieveDataWarning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsCoversWhenDataCorrect() {
      var input = new List<object>() {
        new GH_UnitNumber(new Length(1, LengthUnit.Meter)),
      };
      ComponentTestHelper.SetInput(_component, input);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }
  }
}
