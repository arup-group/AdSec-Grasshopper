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
    private readonly DoubleComparer comparer = new DoubleComparer();
    private readonly List<ILayer> _spacedRebars;
    public RebarLayoutFunctionTests() {
      function = new RebarLayoutFunction();
      BuilderLayer builder = new BuilderLayer();
      _spacedRebars = new List<ILayer>() { builder.Build() };
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
    public void LineLayout_ValidInputs_CreatesLineGroup() {
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
      Assert.Equal(2, layer.Count);
      Assert.Equal(1, layer.BarBundle.CountPerBundle);
      Assert.Equal(0.02, layer.BarBundle.Diameter.As(LengthUnit.Meter), comparer);
      Assert.Equal(100, lineGroup.LastBarPosition.Y.As(LengthUnit.Millimeter), comparer);
      Assert.Equal(0, lineGroup.LastBarPosition.Z.As(LengthUnit.Millimeter), comparer);
      Assert.IsType<AdSecRebarGroup>(group);
      Assert.Empty(function.ErrorMessages);
    }


    [Fact]
    public void SingleBarsLayout_ValidInputs_CreatesSingleBarsGroup() {

      function.RebarLayoutOption = RebarLayoutOption.SingleBars;
      function.RebarBundle.Value = _spacedRebars[0].BarBundle;
      function.Positions.Value = new[] {
      IPoint.Create(Length.FromMillimeters(100), Length.FromMillimeters(100)),
      IPoint.Create(Length.FromMillimeters(150), Length.FromMillimeters(150))
    };

      function.Compute();

      var group = function.RebarGroup.Value;
      Assert.NotNull(group);

      var singleBarGroup = group.Group as ISingleBars;
      Assert.NotNull(singleBarGroup);
      Assert.Equal(2, singleBarGroup.BarBundle.CountPerBundle);
      Assert.Equal(0.02, singleBarGroup.BarBundle.Diameter.As(LengthUnit.Meter), comparer);
      Assert.Equal(100, singleBarGroup.Positions[0].Y.As(LengthUnit.Millimeter), comparer);
      Assert.Equal(100, singleBarGroup.Positions[0].Z.As(LengthUnit.Millimeter), comparer);
      Assert.Equal(150, singleBarGroup.Positions[1].Y.As(LengthUnit.Millimeter), comparer);
      Assert.Equal(150, singleBarGroup.Positions[1].Z.As(LengthUnit.Millimeter), comparer);
      Assert.IsType<AdSecRebarGroup>(function.RebarGroup.Value);
      Assert.Empty(function.ErrorMessages);
    }


    [Fact]
    public void CircleLayout_ValidInputs_CreatesCircleGroup() {

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
      Assert.Equal(2, layer.Count);
      Assert.Equal(1, layer.BarBundle.CountPerBundle);
      Assert.Equal(0.02, layer.BarBundle.Diameter.As(LengthUnit.Meter), comparer);

      Assert.Equal(10, circleBarGroup.Centre.Y.As(LengthUnit.Millimeter), comparer);
      Assert.Equal(11, circleBarGroup.Centre.Z.As(LengthUnit.Millimeter), comparer);
      Assert.Equal(1.5, circleBarGroup.StartAngle.As(AngleUnit.Radian), comparer);
      Assert.IsType<AdSecRebarGroup>(function.RebarGroup.Value);
      Assert.Empty(function.ErrorMessages);
    }

    [Fact]
    public void ArcLayout_ValidInputs_CreatesArcGroup() {
      function.RebarLayoutOption = RebarLayoutOption.Circle;
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
      Assert.Equal(2, layer.Count);
      Assert.Equal(1, layer.BarBundle.CountPerBundle);
      Assert.Equal(0.02, layer.BarBundle.Diameter.As(LengthUnit.Meter), comparer);

      Assert.Equal(10, circleBarGroup.Centre.Y.As(LengthUnit.Millimeter), comparer);
      Assert.Equal(11, circleBarGroup.Centre.Z.As(LengthUnit.Millimeter), comparer);
      Assert.Equal(Math.PI / 4, circleBarGroup.StartAngle.As(AngleUnit.Radian), comparer);
      Assert.Equal(Math.PI / 2, circleBarGroup.SweepAngle.As(AngleUnit.Radian), comparer);
      Assert.IsType<AdSecRebarGroup>(function.RebarGroup.Value);
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
  }
}
