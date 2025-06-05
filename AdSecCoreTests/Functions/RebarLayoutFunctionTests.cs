using AdSecCore;
using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class RebarLayoutFunctionTests {
    public RebarLayoutFunction function;
    private static List<ILayer> _spacedRebars = new List<ILayer>();
    public RebarLayoutFunctionTests() {
      function = new RebarLayoutFunction();
      if (_spacedRebars.Count == 0) {
        _spacedRebars = new List<ILayer>() { (new BuilderLayer()).Build() };
      }
    }

    [Fact]
    public void LineLayout_ValidInputs_PointShouldNotNull() {
      function.RebarLayoutOption = RebarLayoutOption.Line;
      function.SpacedRebars.Value = _spacedRebars[0];
      function.Position1.Value = null;
      function.Position2.Value = null;
      function.Compute();
      Assert.Single(function.ErrorMessages);
    }

    [Fact]
    public void CanCreatesLineLayout() {
      function.RebarLayoutOption = RebarLayoutOption.Line;
      function.SpacedRebars.Value = _spacedRebars[0];
      function.Position1.Value = IPoint.Create(Length.FromMillimeters(0), Length.FromMillimeters(0));
      function.Position2.Value = IPoint.Create(Length.FromMillimeters(100), Length.FromMillimeters(0));
      function.Compute();
      var group = function.RebarGroup.Value;
      var lineGroup = group.Group as ILineGroup;
      Assert.NotNull(group);
      Assert.NotNull(lineGroup);
      var layer = lineGroup.Layer as ILayerByBarCount;
      Assert.NotNull(layer);
      Assert.IsType<AdSecRebarGroup>(group);
      Assert.Empty(function.ErrorMessages);
    }


    [Fact]
    public void CanCreateSingleBarsLayout() {
      function.RebarLayoutOption = RebarLayoutOption.SingleBars;
      function.RebarBundle.Value = _spacedRebars[0].BarBundle;
      function.Positions.Value = new[] {
      IPoint.Create(Length.FromMillimeters(100), Length.FromMillimeters(100)),
      IPoint.Create(Length.FromMillimeters(150), Length.FromMillimeters(150))};

      function.Compute();
      var group = function.RebarGroup.Value;
      Assert.NotNull(group);
      var singleBarGroup = group.Group as ISingleBars;
      Assert.NotNull(singleBarGroup);
      Assert.IsType<AdSecRebarGroup>(function.RebarGroup.Value);
      Assert.Empty(function.ErrorMessages);
    }

    [Fact]
    public void CanCreatesCircularLayout() {

      function.RebarLayoutOption = RebarLayoutOption.Circle;
      function.SpacedRebars.Value = _spacedRebars[0];
      function.CentreOfCircle.Value = IPoint.Create(Length.FromMillimeters(10), Length.FromMillimeters(11));
      function.RadiusOfCircle.Value = 2;
      function.StartAngle.Value = 1.5;
      function.Compute();
      var group = function.RebarGroup.Value;
      Assert.NotNull(group);
      var circleBarGroup = group.Group as ICircleGroup;
      Assert.NotNull(circleBarGroup);
      var layer = circleBarGroup.Layer as ILayerByBarCount;
      Assert.NotNull(layer);
      Assert.IsType<AdSecRebarGroup>(function.RebarGroup.Value);
      Assert.Empty(function.ErrorMessages);
    }

    [Fact]
    public void CanCreatesArcLayout() {
      function.RebarLayoutOption = RebarLayoutOption.Arc;
      function.SpacedRebars.Value = _spacedRebars[0];
      function.CentreOfCircle.Value = IPoint.Create(Length.FromMillimeters(10), Length.FromMillimeters(11));
      function.RadiusOfCircle.Value = 2;
      function.StartAngle.Value = Math.PI / 4;
      function.SweepAngle.Value = Math.PI / 2;
      function.Compute();
      var group = function.RebarGroup.Value;
      Assert.NotNull(group);
      var circleBarGroup = group.Group as IArcGroup;
      Assert.NotNull(circleBarGroup);
      var layer = circleBarGroup.Layer as ILayerByBarCount;
      Assert.NotNull(layer);
      Assert.Empty(function.ErrorMessages);
    }

    [Fact]
    public void RadiusShouldBePositiveValue() {
      function.RebarLayoutOption = RebarLayoutOption.Circle;
      function.SpacedRebars.Value = _spacedRebars[0];
      function.CentreOfCircle.Value = IPoint.Create(Length.FromMillimeters(10), Length.FromMillimeters(11));
      function.RadiusOfCircle.Value = -2;
      function.StartAngle.Value = Math.PI / 4;
      function.SweepAngle.Value = Math.PI / 2;
      function.Compute();
      Assert.Single(function.ErrorMessages);
    }

    [Fact]
    public void ShoulHaveValidNameAndDescription() {
      function.LocalAngleUnit = AngleUnit.Degree;
      function.LocalLengthUnit = LengthUnit.Millimeter;
      function.UpdateUnits();
      Assert.Contains("[mm]", function.RadiusOfCircle.Name);
      Assert.Contains("[°]", function.SweepAngle.Name);
      Assert.Contains("°", function.SweepAngle.Description);
      Assert.Contains("[°]", function.StartAngle.Name);
      Assert.Contains("°", function.StartAngle.Description);
    }
  }
}
