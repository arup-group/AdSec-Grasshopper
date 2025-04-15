
using System.Collections;

using AdSecGH.Helpers;
using AdSecGH.Parameters;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.AdSec.StandardMaterials;
using Oasys.Taxonomy.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Rhino.Geometry;

using TestGrasshopperObjects.Extensions;

using Xunit;

namespace AdSecGHTests.Helpers.Extensions {

  [Collection("GrasshopperFixture collection")]
  public class AdSecSectionsTests {
    private AdSecSectionsTestComponent _component;
    private readonly string _failToRetrieveDataWarning = "failed";
    private readonly string _convertDataError = "convert";

    public AdSecSectionsTests() {
      _component = new AdSecSectionsTestComponent();
    }

    [Fact]
    public void ReturnsWarningWhenInputIsNonOptionalAndNoDataAvailable() {
      _component.Optional = false;
      object obj = null;
      ComponentTestHelper.SetInput(_component, obj);
      var result = (IEnumerable)((GH_ObjectWrapper)ComponentTestHelper.GetOutput(_component)).Value;
      Assert.Empty(result);

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

      var result = (IEnumerable)((GH_ObjectWrapper)ComponentTestHelper.GetOutput(_component)).Value;

      Assert.Empty(result);
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsErrorWhenDataIncorrectInputIsOptional() {
      _component.Optional = true;
      ComponentTestHelper.SetInput(_component, string.Empty);

      var result = (IEnumerable)((GH_ObjectWrapper)ComponentTestHelper.GetOutput(_component)).Value;
      Assert.Empty(result);

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

      var result = (IEnumerable)((GH_ObjectWrapper)ComponentTestHelper.GetOutput(_component)).Value;

      Assert.Empty(result);

      var runtimeMessages = _component.RuntimeMessages(GH_RuntimeMessageLevel.Error);

      Assert.Single(runtimeMessages);
      Assert.Contains(runtimeMessages, item => item.Contains(_convertDataError));
      Assert.Contains(runtimeMessages, item => item.Contains("item 0"));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }

    [Fact]
    public void ReturnsSectionsWhenDataCorrect() {
      var length = new Length(1, LengthUnit.Meter);
      var profile = AdSecProfiles.CreateProfile(new AngleProfile(length, new Flange(length, length),
        new WebConstant(length)));
      var material = Steel.ASTM.A242_46;
      var section = ISection.Create(profile, material);
      var designCode = new AdSecDesignCode().DesignCode;
      var input = new AdSecSectionGoo(new AdSecSection(section, designCode, Plane.WorldXY));
      ComponentTestHelper.SetInput(_component, input);

      object result = ComponentTestHelper.GetOutput(_component);
      Assert.NotNull(result);

      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
      Assert.Empty(_component.RuntimeMessages(GH_RuntimeMessageLevel.Remark));
    }
  }
}
