
using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.StandardMaterials;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using TestGrasshopperObjects.Extensions;

using Xunit;

namespace AdSecGHTests.Helpers.Extensions {

  [Collection("GrasshopperFixture collection")]
  public class AdSecSolutionGooTests {
    private AdSecSolutionGooTestComponent _component;
    private readonly string _failToRetrieveDataWarning = "failed";
    private readonly string _convertDataError = "convert";

    public AdSecSolutionGooTests() {
      _component = new AdSecSolutionGooTestComponent();
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
    public void ReturnsSolutionWhenDataCorrect() {
      var length = new Length(1, LengthUnit.Meter);
      var thickness = new Length(0.2, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(thickness, length),
        new WebConstant(thickness)));
      var section = ISection.Create(profile, Concrete.ACI318.Edition_2002.Metric.MPa_20);
      var designCode = new AdSecDesignCode(ACI318.Edition_2002.Metric).DesignCode;
      var adSecSection = new AdSecSection(section, designCode, Plane.WorldXY);

      var adSec = IAdSec.Create(adSecSection.DesignCode);
      var solution = adSec.Analyse(adSecSection.Section);

      var input = new AdSecSolutionGoo(solution, adSecSection);
      ComponentTestHelper.SetInput(_component, input);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }
  }
}
