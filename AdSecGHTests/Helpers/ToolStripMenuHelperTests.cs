using AdSecGH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {

  public class ToolStripMenuHelperTests {
    [Fact]
    public void CreateInvisibleMenuItem_ShouldReturnToolStripMenuItem_WithNotAvailableText() {
      var result = ToolStripMenuHelper.CreateInvisibleMenuItem();
      Assert.NotNull(result);
      Assert.Equal("Not available", result.Text);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldReturnToolStripMenuItem_WithVisibleEqualFalse() {
      var result = ToolStripMenuHelper.CreateInvisibleMenuItem();
      Assert.False(result.Visible);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldReturnNewInstanceEachTime() {
      var result1 = ToolStripMenuHelper.CreateInvisibleMenuItem();
      var result2 = ToolStripMenuHelper.CreateInvisibleMenuItem();
      Assert.NotSame(result1, result2);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldHaveDefaultEnabledTrue() {
      var result = ToolStripMenuHelper.CreateInvisibleMenuItem();
      Assert.True(result.Enabled);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldHaveDefaultCheckedFalse() {
      var result = ToolStripMenuHelper.CreateInvisibleMenuItem();
      Assert.False(result.Checked);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldNotThrowExceptions() {
      var exception = Record.Exception(ToolStripMenuHelper.CreateInvisibleMenuItem);
      Assert.Null(exception);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldSetVisibleToFalse() {
      var result = ToolStripMenuHelper.CreateInvisibleMenuItem();
      Assert.False(result.Visible, "The 'Visible' property should be set to false.");
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldHaveNullTag() {
      var result = ToolStripMenuHelper.CreateInvisibleMenuItem();
      Assert.Null(result.Tag);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldHaveNullImage() {
      var result = ToolStripMenuHelper.CreateInvisibleMenuItem();
      Assert.Null(result.Image);
    }
  }

}
