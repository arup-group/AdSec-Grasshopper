
using Xunit;

using static AdSecGH.Helpers.Diagram;

namespace AdSecGHTests.Helpers {
  [Collection("GrasshopperFixture collection")]
  public class DiagramTest {

    [Fact]
    public void StepIsInfinityWhenMaximumAndMinimumValueAreUsed() {
      GridAxis gridAxis = new GridAxis(float.MaxValue, float.MinValue);
      Assert.True(float.IsInfinity(gridAxis.major_step));
      Assert.True(float.IsInfinity(gridAxis.minor_step));
    }

    [Fact]
    public void StepIsInfinityWhenWhenMinimumAndMaximumValueAreUsed() {
      GridAxis gridAxis = new GridAxis(float.MinValue, float.MaxValue);
      Assert.True(float.IsInfinity(gridAxis.major_step));
      Assert.True(float.IsInfinity(gridAxis.minor_step));
    }

    [Fact]
    public void StepIsExpectedWhenMinimumAndMaximumValueAreZero() {
      GridAxis gridAxis = new GridAxis(0, 0);
      Assert.Equal(0.2, gridAxis.major_step, 5);
      Assert.Equal(0.05, gridAxis.minor_step, 5);
    }

    [Fact]
    public void StepIsExpectedWhenMinimumAndMaximumValueAreSame() {
      GridAxis gridAxis = new GridAxis(100, 100);
      Assert.Equal(0.2, gridAxis.major_step, 5);
      Assert.Equal(0.05, gridAxis.minor_step, 5);
    }

    [Fact]
    public void MajorRangeIsExpected() {
      GridAxis gridAxis = new GridAxis(5, -1);
      int i = 0;
      foreach (float range in gridAxis.MajorRange) {
        Assert.Equal(gridAxis.min_value + gridAxis.major_step * i++, range, 5);
      }
    }

    [Fact]
    public void MinorRangeIsExpected() {
      GridAxis gridAxis = new GridAxis(5, -1);
      int i = 0;
      foreach (float range in gridAxis.MinorRange) {
        Assert.Equal(gridAxis.min_value + gridAxis.minor_step * i++, range, 5);
      }
    }

    [Fact]
    public void StepIsWithinExpectedRangeWhenScalingIsLessThanTwoPointFive() {
      GridAxis gridAxis = new GridAxis(200, -20);
      Assert.Equal(20, gridAxis.major_step, 5);
      Assert.Equal(5, gridAxis.minor_step, 5);
    }

    [Fact]
    public void StepIsWithinExpectedRangeWhenScalingIsLessThanFive() {
      GridAxis gridAxis = new GridAxis(450, -20);
      Assert.Equal(50, gridAxis.major_step, 5);
      Assert.Equal(10, gridAxis.minor_step, 5);
    }

    [Fact]
    public void StepIsWithinExpectedRangeWhenScalingIsLessThanSevenPointFive() {
      GridAxis gridAxis = new GridAxis(700, -20);
      Assert.Equal(100, gridAxis.major_step, 5);
      Assert.Equal(20, gridAxis.minor_step, 5);
    }

    [Fact]
    public void StepIsWithinExpectedRangeWhenScalingIsGreaterThanSevenPointFive() {
      GridAxis gridAxis = new GridAxis(800, -20);
      Assert.Equal(200, gridAxis.major_step, 5);
      Assert.Equal(50, gridAxis.minor_step, 5);
    }

  }
}
