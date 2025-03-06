using System.Collections.Generic;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Reinforcement.Groups;

using OasysUnits;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class FindCrackLoadTests {

    private readonly FindCrackLoad component;

    public FindCrackLoadTests() {
      component = new FindCrackLoad();
      ComponentTestHelper.SetInput(component, 10, 3);
      ComponentTestHelper.SetInput(component, ILoad.Create(Force.FromNewtons(100), Moment.Zero, Moment.Zero), 1);
    }

    static SectionSolution SolveSection() {
      var analyseFunction = new AnalyseFunction();
      analyseFunction.Section = new SectionParameter() {
        Value = new SectionDesign() {
          DesignCode = IS456.Edition_2000,
          Section = CreateSTDRectangularSection()
        }
      };
      analyseFunction.Compute();
      return analyseFunction.Solution.Value;
    }

    private static ISection CreateSTDRectangularSection() {
      var BottomRight = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Position(13, -28)).Build();
      var BottomLeft = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Position(-13, -28)).Build();
      return new SectionBuilder().WithWidth(30).WithHeight(60).CreateRectangularSection().WithReinforcementGroups(new List<IGroup>() { BottomLeft, BottomRight }).Build();
    }

    private void SetLoadDirectionXX() {
      ComponentTestHelper.SetInput(component, "xx", 2);
    }

    private void SetLoadDirectionZZ() {
      ComponentTestHelper.SetInput(component, "zz", 2);
    }

    private void SetSolution() {
      ComponentTestHelper.SetInput(component, SolveSection(), 0);
    }

    private void SetInvalidSolution() {
      ComponentTestHelper.SetInput(component, "", 0);
    }

    private void SetMaximumCracking() {
      ComponentTestHelper.SetInput(component, 5e-8, 4);
    }

    private void SetInvalidCracking() {
      ComponentTestHelper.SetInput(component, "", 4);
    }

    [Fact]
    public void ShouldHaveFiveInput() {
      Assert.Equal(5, component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveTwoOutputs() {
      SetSolution();
      SetMaximumCracking();
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 0));
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 1));
    }

    [Fact]
    public void ShouldHaveResultWhenLoadDirectionIsZZ() {
      SetSolution();
      SetMaximumCracking();
      SetLoadDirectionZZ();
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 0));
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 1));
    }

    [Fact]
    public void ShouldHaveResultWhenLoadDirectionIsXX() {
      SetSolution();
      SetMaximumCracking();
      SetLoadDirectionXX();
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 0));
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 1));
    }

    [Fact]
    public void ShouldHaveErrors() {
      SetInvalidSolution();
      SetMaximumCracking();
      ComponentTestHelper.GetOutput(component);
      Assert.Single(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveErrorWhenCrackingIsInvalid() {
      SetInvalidSolution();
      SetInvalidCracking();
      ComponentTestHelper.GetOutput(component);
      Assert.Single(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }
  }
}
