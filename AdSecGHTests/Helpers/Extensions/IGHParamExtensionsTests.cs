﻿using AdSecGH.Components;
using AdSecGH.Helpers;

using Grasshopper.Kernel;

using Xunit;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class IGHParamExtensionsTests {
    private GH_Component owner;

    public IGHParamExtensionsTests() {
      owner = new CreatePoint(); // whatever component
    }

    [Fact]
    public void ConvertToErrorShouldAddErrorIntoParam() {
      owner.Params.Input[0].ConvertToError("test");

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(actualResult);
      Assert.Contains("test", actualResult[0]);
    }

    [Fact]
    public void ConvertToErrorShouldAddCorrectErrorWithIntoParam() {
      owner.Params.Input[0].ConvertToError(null);

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(actualResult);
      Assert.Contains("to ", actualResult[0]);
    }

    [Fact]
    public void ConvertToErrorShouldAddEmptyErrorWithIntoParam() {
      owner.Params.Input[0].ConvertToError(string.Empty);

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(actualResult);
      Assert.Contains("to ", actualResult[0]);
    }

    [Fact]
    public void ConvertFromToErrorShouldAddErrorIntoParam() {
      owner.Params.Input[0].ConvertFromToError("test", "test1");

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(actualResult);
      Assert.Contains("from test to test1", actualResult[0]);
    }

    [Fact]
    public void ConvertFromToErrorShouldAddCorrectErrorIntoParam() {
      owner.Params.Input[0].ConvertFromToError(null, null);

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(actualResult);
      Assert.Contains("to ", actualResult[0]);
    }

    [Fact]
    public void ConvertFromToErrorShouldAddEmptyErrorIntoParam() {
      owner.Params.Input[0].ConvertFromToError(string.Empty, string.Empty);

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Error);
      Assert.Single(actualResult);
      Assert.Contains("to ", actualResult[0]);
    }

    [Fact]
    public void FailedToCollectDataWarningShouldAddWarningIntoParam() {
      owner.Params.Input[0].FailedToCollectDataWarning();

      var actualResult = owner.RuntimeMessages(GH_RuntimeMessageLevel.Warning);
      Assert.Single(actualResult);
      Assert.Contains("failed", actualResult[0]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateInput_SetsPropertiesCorrectlyForItemAccess(bool optional) {
      var param = owner.Params.Input[0];
      param.UpdateItemInput("NewName", "NewNickName", "NewDescription", optional);

      Assert.Equal("NewName", param.Name);
      Assert.Equal("NewNickName", param.NickName);
      Assert.Equal("NewDescription", param.Description);
      Assert.Equal(GH_ParamAccess.item, param.Access);
      Assert.Equal(optional, param.Optional);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateInput_SetsPropertiesCorrectlyForListAccess(bool optional) {
      var param = owner.Params.Input[0];
      param.UpdateListInput("X", "Y", "Z", optional);

      Assert.Equal("X", param.Name);
      Assert.Equal("Y", param.NickName);
      Assert.Equal("Z", param.Description);
      Assert.Equal(GH_ParamAccess.list, param.Access);
      Assert.Equal(optional, param.Optional);
    }
  }
}
