using AdSecCore.Builders;

using AdSecGH.Parameters;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Reinforcement.Groups;

using OasysUnits.Units;

namespace AdSecCoreTests.Builder {

  public class SectionBuilderTests {

    [Fact]
    public void ShouldNotDoAnythingForCenteredBars() {
      var designCode = IS456.Edition_2000;

      var singleBars = new BuilderSingleBar().AtPosition(Geometry.Zero()).WithSize(2).AtPosition(Geometry.Zero()).Build();
      var rebarOriginal = new List<AdSecRebarGroup> {
        new() {
          Group = singleBars,
        },
      };

      var sectionBuilder = new SectionBuilder();
      var section = sectionBuilder.WithWidth(10).WithHeight(10).CreatePerimeterSection()
       .WithReinforcementGroup(singleBars).Build();

      var rebar = SectionBuilder.CalibrateReinforcementGroupsForSection(rebarOriginal, designCode, section);

      Assert.Single(rebar);
      var group = rebar[0].Group as ISingleBars;
      Assert.NotNull(group);
      var position = group.Positions.First();
      Assert.Equal(0, position.Y.ToUnit(LengthUnit.Centimeter).Value);
      Assert.Equal(0, position.Z.ToUnit(LengthUnit.Centimeter).Value);
    }

    [Fact]
    public void ShouldNotHaveAnyChangeIfRebarIsOnZeroZero() {
      var designCode = IS456.Edition_2000;

      var lineBars = new BuilderLineGroup().Build();
      var rebarOriginal = new List<AdSecRebarGroup> {
        new() {
          Group = lineBars,
        },
      };

      var sectionBuilder = new SectionBuilder();
      var section = sectionBuilder.WithWidth(20).WithHeight(20).CreatePerimeterSection()
       .WithReinforcementGroup(lineBars).Build();

      var rebar = SectionBuilder.CalibrateReinforcementGroupsForSection(rebarOriginal, designCode, section);

      var group = rebar.First().Group as ILineGroup;
      Assert.Equal(lineBars, group);
    }

    [Theory]
    [InlineData("GEO P(m) M(1|2) L(3|4)", 2)]
    [InlineData("GEO P(m) M(-1|-2) L(-3|-4)", 2)]
    [InlineData("GEO P(m)", 0)]
    [InlineData("", 0)]
    public void ShouldHandleDifferentInputPatterns(string input, int expectedCount) {
      var coordinates = SectionBuilder.ParseCoordinates(input);
      Assert.Equal(expectedCount, coordinates.Count);
      Assert.NotNull(coordinates);
    }
  }
}
