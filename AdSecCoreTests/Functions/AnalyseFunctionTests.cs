using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec.DesignCode;

namespace AdSecCoreTests.Functions {
  public class AnalyseFunctionTests {

    private readonly AnalyseFunction analyseFunction;

    public AnalyseFunctionTests() {
      analyseFunction = new AnalyseFunction();
    }

    [Fact]
    public void ShouldHaveOneInput() {
      Assert.Single(analyseFunction.GetAllInputAttributes());
    }

    [Fact]
    public void ShouldHaveTwoOutputs() {
      Assert.Equal(2, analyseFunction.GetAllOutputAttributes().Length);
    }

    [Fact]
    public void ShouldComputeOutputs() {
      var sectionBuilder = new SectionBuilder();
      var section = sectionBuilder.WithHeight(0.0001).WithWidth(0.01).CreateRectangularSection().Build();
      analyseFunction.Section = new SectionParameter() {
        Value = new SectionDesign() {
          DesignCode = IS456.Edition_2000,
          Section = section
        }
      };

      analyseFunction.Compute();
      Assert.NotNull(analyseFunction.Solution);
    }

    [Fact]
    public void ShouldAddWarnings() {
      analyseFunction.Section = new SectionParameter() {
        Value = new SectionDesign() {
          DesignCode = IS456.Edition_2000,
          Section = SectionBuilder.InvalidSection()
        }
      };
      analyseFunction.Compute();
      Assert.Single(analyseFunction.WarningMessages);
    }
  }
}
