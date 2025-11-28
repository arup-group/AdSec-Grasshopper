using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGHCore.Constants;

using Oasys.AdSec.DesignCode;

namespace AdSecCoreTests.Functions {
  public class AnalyseFunctionTests {

    private readonly AnalyseSectionFunction analyseFunction;

    public AnalyseFunctionTests() {
      analyseFunction = new AnalyseSectionFunction();
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
    public void ShouldIncludeAParameterNamedSection() {
      Assert.Equal("Section", analyseFunction.Section.Name);
    }

    [Fact]
    public void ShouldIncludeAParameterNamedResults() {
      Assert.Equal("Results", analyseFunction.Solution.Name);
    }

    [Fact]
    public void ShouldIncludeAParameterNamedFailureSurface() {
      Assert.Equal("FailureSurface", analyseFunction.LoadSurface.Name);
    }

    [Fact]
    public void ShouldHaveName() {
      Assert.Equal("Analyse Section", analyseFunction.Metadata.Name);
    }

    [Fact]
    public void ShouldHaveNickName() {
      Assert.Equal("Analyse", analyseFunction.Metadata.NickName);
    }

    [Fact]
    public void ShouldHaveDescription() {
      Assert.Equal("Analyse an AdSec Section", analyseFunction.Metadata.Description);
    }

    [Fact]
    public void ShouldHaveCategory() {
      Assert.Equal(CategoryName.Name(), analyseFunction.Organisation.Category);
    }

    [Fact]
    public void ShouldHaveSubCategory() {
      Assert.Equal(SubCategoryName.Cat6(), analyseFunction.Organisation.SubCategory);
    }

    [Fact]
    public void ShouldComputeOutputs() {
      var sectionBuilder = new SectionBuilder();
      var section = sectionBuilder.WithHeight(0.0001).WithWidth(0.01).CreateRectangularSection().Build();
      analyseFunction.Section = new SectionParameter() {
        Value = new SectionDesign() {
          DesignCode = new DesignCode() {
            IDesignCode = IS456.Edition_2000,
          },
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
          DesignCode = new DesignCode() {
            IDesignCode = IS456.Edition_2000,
          },
          Section = SectionBuilder.InvalidSection()
        }
      };
      analyseFunction.Compute();
      Assert.Single(analyseFunction.WarningMessages);
    }

    [Fact]
    public void ShouldMaintainThePlaneFromSection() {
      var sectionBuilder = new SectionBuilder();
      var section = sectionBuilder.WithHeight(0.0001).WithWidth(0.01).CreateRectangularSection().Build();
      analyseFunction.Section = new SectionParameter() {
        Value = new SectionDesign() {
          DesignCode = new DesignCode() {
            IDesignCode = IS456.Edition_2000,
          },
          Section = section,
          LocalPlane = OasysPlane.PlaneXY
        }
      };

      analyseFunction.Compute();
      Assert.Equal(OasysPlane.PlaneXY, analyseFunction.LoadSurface.Value.LocalPlane);
    }
  }
}
