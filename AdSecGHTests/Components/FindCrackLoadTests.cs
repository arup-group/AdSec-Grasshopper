using System.Collections.Generic;
using System.ComponentModel;

using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;
using AdSecGH.Parameters;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.GH.Helpers;

using OasysUnits;

using Rhino.Geometry;
using Rhino.NodeInCode;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class FindCrackLoadTests {

    private readonly FindCrackLoad component;

    public FindCrackLoadTests() {
      component = new FindCrackLoad();
      ComponentTestHelper.SetInput(component, "yy", 2);
      ComponentTestHelper.SetInput(component, 10, 3);
    }

    static SectionSolutionParameter SolveSection() {
      var analyseFunction = new AnalyseFunction();
      analyseFunction.Section = new SectionParameter() {
        Value = new SectionDesign() {
          DesignCode = IS456.Edition_2000,
          Section = CreateSTDRectangularSection()
        }
      };
      analyseFunction.Compute();
      return analyseFunction.Solution;
    }

    private static ISection CreateSTDRectangularSection() {
      var BottomRight = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Position(13, -28)).Build();
      var BottomLeft = new BuilderReinforcementGroup().WithSize(2).CreateSingleBar().AtPosition(Geometry.Position(-13, -28)).Build();
      return new SectionBuilder().WithWidth(30).WithHeight(60).CreateRectangularSection().WithReinforcementGroups(new List<IGroup>() { BottomLeft, BottomRight }).Build();
    }

    private void SetSolutionAsParameter() {
      ComponentTestHelper.SetInput(component, SolveSection(), 0);
    }

    private void SetSolutionAsValue() {
      ComponentTestHelper.SetInput(component, SolveSection().Value, 0);
    }

    private void SetInvalidSolution() {
      ComponentTestHelper.SetInput(component, "", 0);
    }

    private void SetLoadInputAsParameter() {
      var parameter = new LoadParameter();
      parameter.Value = ILoad.Create(Force.FromNewtons(100), Moment.Zero, Moment.Zero);
      ComponentTestHelper.SetInput(component, parameter, 1);
    }

    private void SetLoadInputAsValue() {
      ComponentTestHelper.SetInput(component, ILoad.Create(Force.FromNewtons(100), Moment.Zero, Moment.Zero), 1);
    }

    private void SetMaximumCracking() {
      ComponentTestHelper.SetInput(component, 5e-8, 4);
    }

    private void SetInvalidCracking() {
      ComponentTestHelper.SetInput(component, "", 4);
    }

    private void SetMaximumCrackingAsParameter() {
      var parameter = new LengthParameter();
      parameter.Value = Length.FromMillimeters(0.00005);
      ComponentTestHelper.SetInput(component, parameter, 4);
    }

    private void SetMaximumCrackingAsValue() {
      ComponentTestHelper.SetInput(component, Length.FromMillimeters(0.00005), 4);
    }

    [Fact]
    public void ShouldHaveFiveInput() {
      Assert.Equal(5, component.Params.Input.Count);
    }

    [Fact]
    public void ShouldHaveTwoOutputs() {
      SetSolutionAsValue();
      SetLoadInputAsValue();
      SetMaximumCracking();

      Assert.NotNull(ComponentTestHelper.GetOutput(component, 0));
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 1));
    }

    [Fact]
    public void ShouldHaveResultWhenInputSetByValue() {
      SetSolutionAsValue();
      SetLoadInputAsValue();
      SetMaximumCrackingAsValue();

      Assert.NotNull(ComponentTestHelper.GetOutput(component, 0));
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 1));
    }

    [Fact]
    public void ShouldHaveResultWhenInputSetByParameter() {
      SetSolutionAsParameter();
      SetLoadInputAsParameter();
      SetMaximumCrackingAsParameter();
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 0));
      Assert.NotNull(ComponentTestHelper.GetOutput(component, 1));
    }

    [Fact]
    public void ShouldHaveErrors() {
      SetInvalidSolution();
      SetLoadInputAsValue();
      SetMaximumCracking();
      ComponentTestHelper.GetOutput(component);
      Assert.Single(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }

    [Fact]
    public void ShouldHaveErrorWhenCrackingIsInvalid() {
      SetInvalidSolution();
      SetLoadInputAsValue();
      SetInvalidCracking();
      ComponentTestHelper.GetOutput(component);
      Assert.Single(component.RuntimeMessages(GH_RuntimeMessageLevel.Error));
    }
  }
}
