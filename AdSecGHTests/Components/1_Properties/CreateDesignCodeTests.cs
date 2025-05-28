using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Helpers;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec.DesignCode;
using Oasys.GH.Helpers;

using Xunit;

namespace AdSecGHTests.Components.Properties {
  [Collection("GrasshopperFixture collection")]
  public class CreateDesignCodeTests {
    private readonly CreateDesignCode _component;
    public CreateDesignCodeTests() {
      AdSecUtility.LoadAdSecAPI();
      _component = new CreateDesignCode();
    }

    private void SetACI318Edition2002Metrric() {
      _component.SetSelected(0, 1);
    }

    [Fact]
    public void ExpectedDesignCodeIsACI318Edition2002Metric() {
      SetACI318Edition2002Metrric();
      var output = (AdSecDesignCodeGoo)ComponentTestHelper.GetOutput(_component);
      Assert.Equal("ACI318+Edition_2002+Metric", output.Value.DesignCodeName);
    }

  }
}
