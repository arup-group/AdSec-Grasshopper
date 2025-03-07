using AdSecCore.Builders;

using Oasys.AdSec.DesignCode;

namespace AdSecCoreTests {

  public class SolutionBuilderTests {

    [Fact]
    public void CanCreateDefaultSolution() {
      var solution = new SolutionBuilder().Build();
      Assert.NotNull(solution.Strength);
      Assert.NotNull(solution.Serviceability);
    }

    [Fact]
    public void NullSectionWillWillThrowException() {
      Assert.Throws<ArgumentNullException>(() => new SolutionBuilder().WithSection(null).Build());
    }

    [Fact]
    public void NullDesignCodeWillWillThrowException() {
      Assert.Throws<ArgumentNullException>(() => new SolutionBuilder().WithDesignCode(null).Build());
    }

    [Fact]
    public void CanAnalyseSection() {
      var section = new SectionBuilder().WithWidth(30).WithHeight(60).CreateRectangularSection().Build();
      var solution = new SolutionBuilder().WithDesignCode(IS456.Edition_2000).WithSection(section).Build();
      Assert.NotNull(solution.Strength);
      Assert.NotNull(solution.Serviceability);
    }

  }
}
