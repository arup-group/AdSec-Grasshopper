using System.Windows.Forms;

using AdSecGH.Helpers;

using Xunit;

namespace AdSecGHTests.Helpers {

  public class ToolStripMenuHelperTests {
    private ToolStripMenuItem result;

    public ToolStripMenuHelperTests() {
      result = ToolStripMenuHelper.CreateInvisibleMenuItem();
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldReturnToolStripMenuItem_WithNotAvailableText() {
      Assert.NotNull(result);
      Assert.Equal("Not available", result.Text);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldReturnToolStripMenuItem_WithVisibleEqualFalse() {
      Assert.False(result.Visible);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldHaveDefaultEnabledTrue() {
      Assert.True(result.Enabled);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldHaveDefaultCheckedFalse() {
      Assert.False(result.Checked);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldNotThrowExceptions() {
      var exception = Record.Exception(ToolStripMenuHelper.CreateInvisibleMenuItem);
      Assert.Null(exception);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldSetVisibleToFalse() {
      Assert.False(result.Visible, "The 'Visible' property should be set to false.");
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldHaveNullTag() {
      Assert.Null(result.Tag);
    }

    [Fact]
    public void CreateInvisibleMenuItem_ShouldHaveNullImage() {
      Assert.Null(result.Image);
    }
  }
}
