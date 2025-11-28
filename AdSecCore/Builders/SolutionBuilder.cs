using System.Collections.Generic;

using AdSecCore.Functions;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Reinforcement.Groups;

namespace AdSecCore.Builders {

  public class SolutionBuilder : IBuilder<SectionSolution> {
    private ISection Section { get; set; } = CreateRectangularSection();
    private IDesignCode DesignCode { get; set; } = IS456.Edition_2000;

    public SectionSolution Build() {
      var analyseFunction = new AnalyseSectionFunction();
      analyseFunction.Section = new SectionParameter {
        Value = new SectionDesign {
          DesignCode = new DesignCode() {
            IDesignCode = DesignCode,
          },
          Section = Section,
        },
      };
      analyseFunction.Compute();
      return analyseFunction.Solution.Value;
    }

    private static ISection CreateRectangularSection() {
      var BottomRight = new BuilderSingleBar().WithSize(2).AtPosition(Geometry.Position(13, -28))
       .Build();
      var BottomLeft = new BuilderSingleBar().WithSize(2).AtPosition(Geometry.Position(-13, -28))
       .Build();
      return new SectionBuilder().WithWidth(30).WithHeight(60).CreateRectangularSection()
       .WithReinforcementGroups(new List<IGroup> { BottomLeft, BottomRight, }).Build();
    }

    public SolutionBuilder WithSection(ISection section) {
      Section = section;
      return this;
    }

    public SolutionBuilder WithDesignCode(IDesignCode code) {
      DesignCode = code;
      return this;
    }
  }
}
