using AdSecCore.Builders;

using Oasys.AdSec;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;


namespace AdSecCoreTests.Builder {

  public class SectionBuilderTests {

    private static readonly IReinforcement DefaultSteel = Reinforcement.Steel.IS456.Edition_2000.S415;

    [Fact]
    public void WithReinforcementGroupsOffset_ShouldAddOffsetToiSingleBars() {
      var originalBars = CreateSingleBarsWithTwoPositions(25, 100, 200, 300, 400);
      var groups = new List<IGroup> { originalBars };
      var section = BuildSectionWithOffset(groups, deltaY: 50, deltaZ: 75);
      var offsetGroup = GetSingleBarsFromSection(section);

      Assert.Equal(2, offsetGroup.Positions.Count);
      AssertPositionEquals(offsetGroup.Positions[0], expectedY: 150, expectedZ: 275);
      AssertPositionEquals(offsetGroup.Positions[1], expectedY: 350, expectedZ: 475);
    }

    [Fact]
    public void WithReinforcementGroupsOffset_ShouldAddOffsetLineGroupToReinforcementGroups() {

      var lineGroup = CreateLineGroup(0, 0, 100, 100);
      var groups = new List<IGroup> { lineGroup };

      var section = BuildSectionWithOffset(groups, deltaY: 25, deltaZ: 35);
      var offsetGroup = GetLineGroupFromSection(section);

      AssertPositionEquals(offsetGroup.FirstBarPosition, expectedY: 25, expectedZ: 35);
      AssertPositionEquals(offsetGroup.LastBarPosition, expectedY: 125, expectedZ: 135);
    }

    [Fact]
    public void WithReinforcementGroupsOffset_ShouldHandleMultipleGroupTypes() {
      var singleBars = CreateSingleBars(16, 50, 60);
      var lineGroup = CreateLineGroup(10, 20, 80, 90);
      var groups = new List<IGroup> { singleBars, lineGroup };
      var section = BuildSectionWithOffset(groups, deltaY: 15, deltaZ: 25);
      Assert.Equal(2, section.ReinforcementGroups.Count);
    }

    [Fact]
    public void WithReinforcementGroupsOffset_ShouldHandleEmptyGroupsList() {
      var section = BuildSectionWithOffset(new List<IGroup>(), deltaY: 10, deltaZ: 20);
      Assert.Empty(section.ReinforcementGroups);
    }


    [Fact]
    public void WithReinforcementGroupsOffset_ShouldHandleNegativeOffsets() {
      var originalBars = CreateSingleBars(16, 200, 300);
      var groups = new List<IGroup> { originalBars };
      var section = BuildSectionWithOffset(groups, deltaY: -50, deltaZ: -75);
      var offsetGroup = GetSingleBarsFromSection(section);
      Assert.Single(offsetGroup.Positions);
      AssertPositionEquals(offsetGroup.Positions[0], expectedY: 150, expectedZ: 225);
    }

    [Fact]
    public void WithReinforcementGroupsOffset_ShouldPreserveSingleBarsPreload() {
      var originalBars = CreateSingleBars(16, 200, 300);
      originalBars.Preload = IPreForce.Create(Force.FromKilonewtons(10));

      var section = BuildSectionWithOffset(new List<IGroup> { originalBars }, deltaY: 25, deltaZ: 50);
      var offsetGroup = GetSingleBarsFromSection(section);

      var preload = Assert.IsAssignableFrom<IPreForce>(offsetGroup.Preload);
      Assert.Equal(10, preload.Force.Kilonewtons, 6);
    }

    [Fact]
    public void WithReinforcementGroupsOffset_ShouldAddOffsetToArcGroupCentre() {
      var arcGroup = CreateArcGroup(100, 200, 250);

      var section = BuildSectionWithOffset(new List<IGroup> { arcGroup }, deltaY: 40, deltaZ: -20);
      var offsetGroup = GetArcGroupFromSection(section);

      AssertPositionEquals(offsetGroup.Centre, expectedY: 140, expectedZ: 180);
      Assert.Equal(250, offsetGroup.Radius.Millimeters, 6);
    }

    [Fact]
    public void WithReinforcementGroupsOffset_ShouldAddOffsetToCircleGroupCentre() {
      var circleGroup = CreateCircleGroup(120, 180, 300);

      var section = BuildSectionWithOffset(new List<IGroup> { circleGroup }, deltaY: -30, deltaZ: 45);
      var offsetGroup = GetCircleGroupFromSection(section);

      AssertPositionEquals(offsetGroup.Centre, expectedY: 90, expectedZ: 225);
      Assert.Equal(300, offsetGroup.Radius.Millimeters, 6);
    }


    private static ISingleBars CreateSingleBars(double barDiameterMm, double y, double z) {
      var barBundle = IBarBundle.Create(DefaultSteel, new Length(barDiameterMm, LengthUnit.Millimeter));
      var singleBars = ISingleBars.Create(barBundle);
      singleBars.Positions.Add(IPoint.Create(
        new Length(y, LengthUnit.Millimeter),
        new Length(z, LengthUnit.Millimeter)
      ));
      return singleBars;
    }

    private static ISingleBars CreateSingleBarsWithTwoPositions(double barDiameterMm, double y1, double z1, double y2, double z2) {
      var barBundle = IBarBundle.Create(DefaultSteel, new Length(barDiameterMm, LengthUnit.Millimeter));
      var singleBars = ISingleBars.Create(barBundle);
      singleBars.Positions.Add(IPoint.Create(new Length(y1, LengthUnit.Millimeter), new Length(z1, LengthUnit.Millimeter)));
      singleBars.Positions.Add(IPoint.Create(new Length(y2, LengthUnit.Millimeter), new Length(z2, LengthUnit.Millimeter)));
      return singleBars;
    }

    private static ILineGroup CreateLineGroup(double startY, double startZ, double endY, double endZ) {
      var layer = new BuilderLayer().Build();
      return ILineGroup.Create(
        IPoint.Create(new Length(startY, LengthUnit.Millimeter), new Length(startZ, LengthUnit.Millimeter)),
        IPoint.Create(new Length(endY, LengthUnit.Millimeter), new Length(endZ, LengthUnit.Millimeter)),
        layer
      );
    }

    private static IArcGroup CreateArcGroup(double centerY, double centerZ, double radius) {
      var layer = new BuilderLayer().Build();
      return IArcGroup.Create(
        IPoint.Create(new Length(centerY, LengthUnit.Millimeter), new Length(centerZ, LengthUnit.Millimeter)),
        new Length(radius, LengthUnit.Millimeter),
        Angle.FromDegrees(0),
        Angle.FromDegrees(90),
        layer
      );
    }

    private static ICircleGroup CreateCircleGroup(double centerY, double centerZ, double radius) {
      var layer = new BuilderLayer().Build();
      return ICircleGroup.Create(
        IPoint.Create(new Length(centerY, LengthUnit.Millimeter), new Length(centerZ, LengthUnit.Millimeter)),
        new Length(radius, LengthUnit.Millimeter),
        Angle.FromDegrees(0),
        layer
      );
    }

    private static ISection BuildSectionWithOffset(List<IGroup> groups, double deltaY, double deltaZ) {
      return new SectionBuilder()
        .WithReinforcementGroupsOffset(groups, new Length(deltaY, LengthUnit.Millimeter), new Length(deltaZ, LengthUnit.Millimeter))
        .WithWidth(500)
        .CreateSquareSection()
        .Build();
    }

    private static ISingleBars GetSingleBarsFromSection(ISection section) {
      Assert.Single(section.ReinforcementGroups);
      var singleBars = section.ReinforcementGroups[0] as ISingleBars;
      Assert.NotNull(singleBars);
      return singleBars;
    }

    private static ILineGroup GetLineGroupFromSection(ISection section) {
      Assert.Single(section.ReinforcementGroups);
      var lineGroup = section.ReinforcementGroups[0] as ILineGroup;
      Assert.NotNull(lineGroup);
      return lineGroup;
    }

    private static IArcGroup GetArcGroupFromSection(ISection section) {
      Assert.Single(section.ReinforcementGroups);
      var arcGroup = section.ReinforcementGroups[0] as IArcGroup;
      Assert.NotNull(arcGroup);
      return arcGroup;
    }

    private static ICircleGroup GetCircleGroupFromSection(ISection section) {
      Assert.Single(section.ReinforcementGroups);
      var circleGroup = section.ReinforcementGroups[0] as ICircleGroup;
      Assert.NotNull(circleGroup);
      return circleGroup;
    }

    private static void AssertPositionEquals(IPoint position, double expectedY, double expectedZ) {
      Assert.Equal(expectedY, position.Y.Millimeters, 1);
      Assert.Equal(expectedZ, position.Z.Millimeters, 1);
    }
  }
}
