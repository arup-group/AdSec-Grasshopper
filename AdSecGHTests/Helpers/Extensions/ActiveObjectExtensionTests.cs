using AdSecGH.Components;
using AdSecGH.Helpers;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class ActiveObjectExtensionTests_NotEmptyMessage {
    private CreatePoint component;

    public ActiveObjectExtensionTests_NotEmptyMessage() {
      component = new CreatePoint();
      component.AddRuntimeWarning("test");
      component.AddRuntimeError("test");
      component.AddRuntimeRemark("test");
    }

    [Fact]
    public void AddRuntimeWarningReturnValidMessage() {
      var actualResult = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(actualResult);
      Assert.Equal("test", actualResult[0]);
    }

    [Fact]
    public void AddRuntimeErrorReturnValidMessage() {
      var actualResult = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(actualResult);
      Assert.Equal("test", actualResult[0]);
    }

    [Fact]
    public void AddRuntimeRemarkReturnValidMessage() {
      var actualResult = component.RuntimeMessages(GH_RuntimeMessageLevel.Remark);
      Assert.Single(actualResult);
      Assert.Equal("test", actualResult[0]);
    }
  }

  [Collection("GrasshopperFixture collection")]
  public class ActiveObjectExtensionTests_EmptyMessage {
    private CreatePoint component;

    public ActiveObjectExtensionTests_EmptyMessage() {
      component = new CreatePoint();
      component.AddRuntimeWarning(string.Empty);
      component.AddRuntimeError(string.Empty);
      component.AddRuntimeRemark(string.Empty);
    }

    [Fact]
    public void AddRuntimeWarningNotReturnMessages() {
      var actualResult = component.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Empty(actualResult);
    }

    [Fact]
    public void AddRuntimeErrorNotReturnMessages() {
      var actualResult = component.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Empty(actualResult);
    }

    [Fact]
    public void AddRuntimeRemarkNotReturnMessages() {
      var actualResult = component.RuntimeMessages(GH_RuntimeMessageLevel.Remark);
      Assert.Empty(actualResult);
    }
  }
}
