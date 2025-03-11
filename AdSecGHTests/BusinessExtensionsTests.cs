using System;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

using Oasys.GH.Helpers;

using Xunit;

using ComponentWithNoPreviouslyDefinedParameters = AdSecGHTests.Helpers.ComponentWithNoPreviouslyDefinedParameters;

namespace AdSecGHTests {
  [Collection("GrasshopperFixture collection")]
  public class BusinessExtensionsTests {

    private readonly ComponentWithNoPreviouslyDefinedParameters component;

    public BusinessExtensionsTests() {
      AddFake();
      component = new ComponentWithNoPreviouslyDefinedParameters();
      component.SetInputParamAt(0, "");
    }

    [Fact]
    public void ShouldThrowExceptionWhenParameterAndValueAreNotMatching() {
      component.CollectData();
      //ComponentTestHelper.GetOutput(component);
      //ComponentTestHelper.GetOutput(component);
      Assert.Throws<Exception>(() => ComponentTestHelper.GetOutput(component));
    }

    [Fact]
    public void ShouldHaveNoWarnings() {
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Warning));
    }

    [Fact]
    public void ShouldHaveNoErrors() {
      Assert.Empty(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void TestOutputs() {
      Assert.Single(component.Params.Output);
    }

    private static void AddFake() {
      BusinessExtensions.AddToGhParam(typeof(ParameterMissing),
        a => new Param_Arc { Name = a.Name, Access = GH_ParamAccess.item, });
      BusinessExtensions.AddToGoo(typeof(ParameterMissing), a => "");
    }
  }
}
