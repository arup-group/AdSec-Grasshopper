using System;

using AdSecCore;
using AdSecCore.Builders;

using AdSecGH;
using AdSecGH.Components;
using AdSecGH.Parameters;
using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Xunit;
namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class CreateRebarLayoutTests {
    private readonly CreateRebarLayout _component;
    private readonly DoubleComparer _comparer = new DoubleComparer();
    private static AdSecRebarLayerGoo _layerGoo = null;
    public CreateRebarLayoutTests() {
      _layerGoo ??= new AdSecRebarLayerGoo((new BuilderLayer()).Build());
      _component = new CreateRebarLayout();
    }

    private void SetLineRebarInputs() {
      ComponentTestHelper.SetInput(_component, _layerGoo, 0);
      ComponentTestHelper.SetInput(_component, new AdSecPointGoo(IPoint.Create(Length.Zero, Length.Zero)), 1);
      ComponentTestHelper.SetInput(_component, new AdSecPointGoo(IPoint.Create(Length.FromMillimeters(100), Length.FromMillimeters(150))), 2);
    }

    private void SetPointRebarInputs() {
      _component.SetSelected(0, 1);
      ComponentTestHelper.SetInput(_component, _layerGoo.Value.BarBundle, 0);
      ComponentTestHelper.SetInput(_component, new AdSecPointGoo(IPoint.Create(Length.FromMillimeters(100), Length.FromMillimeters(150))), 1);
    }

    private void SetCircularRebarInputs() {
      _component.SetSelected(0, 2);
      ComponentTestHelper.SetInput(_component, _layerGoo, 0);
      ComponentTestHelper.SetInput(_component, new AdSecPointGoo(IPoint.Create(Length.FromMillimeters(10), Length.FromMillimeters(11))), 1);
      ComponentTestHelper.SetInput(_component, 2, 2);
      ComponentTestHelper.SetInput(_component, 1.5, 3);
    }

    private void SetArcRebarInputs() {
      _component.SetSelected(0, 3);
      ComponentTestHelper.SetInput(_component, _layerGoo, 0);
      ComponentTestHelper.SetInput(_component, new AdSecPointGoo(IPoint.Create(Length.FromMillimeters(10), Length.FromMillimeters(11))), 1);
      ComponentTestHelper.SetInput(_component, 2, 2);
      ComponentTestHelper.SetInput(_component, Math.PI / 4, 3);
      ComponentTestHelper.SetInput(_component, Math.PI / 2, 4);
    }


    [Fact]
    public void CanCreateSingleBarsLayout() {
      SetPointRebarInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = (AdSecRebarGroupGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      var singleBarGroup = (ISingleBars)output.Value.Group;
      Assert.Equal(1, singleBarGroup.BarBundle.CountPerBundle);
      Assert.Equal(0.02, singleBarGroup.BarBundle.Diameter.As(LengthUnit.Meter), _comparer);
      Assert.Equal(100, singleBarGroup.Positions[0].Y.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(150, singleBarGroup.Positions[0].Z.As(LengthUnit.Millimeter), _comparer);
    }

    [Fact]
    public void CanCreatesLineLayout() {
      SetLineRebarInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = (AdSecRebarGroupGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      var lineGroup = (ILineGroup)output.Value.Group;
      Assert.NotNull(lineGroup);
      var layer = (ILayerByBarCount)lineGroup.Layer;
      Assert.NotNull(layer);
      Assert.Equal(2, layer.Count);
      Assert.Equal(1, layer.BarBundle.CountPerBundle);
      Assert.Equal(20, layer.BarBundle.Diameter.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(100, lineGroup.LastBarPosition.Y.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(150, lineGroup.LastBarPosition.Z.As(LengthUnit.Millimeter), _comparer);
    }

    [Fact]
    public void CanCreatesCircularLayout() {
      SetCircularRebarInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = (AdSecRebarGroupGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      var circleBarGroup = (ICircleGroup)output.Value.Group;
      Assert.NotNull(circleBarGroup);
      var layer = circleBarGroup.Layer as ILayerByBarCount;
      Assert.NotNull(layer);
      Assert.Equal(2, layer.Count);
      Assert.Equal(1, layer.BarBundle.CountPerBundle);
      Assert.Equal(20, layer.BarBundle.Diameter.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(10, circleBarGroup.Centre.Y.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(11, circleBarGroup.Centre.Z.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(1.5, circleBarGroup.StartAngle.As(AngleUnit.Radian), _comparer);
    }

    [Fact]
    public void CanCreatesArcLayout() {
      SetArcRebarInputs();
      ComponentTestHelper.ComputeData(_component);
      var output = (AdSecRebarGroupGoo)ComponentTestHelper.GetOutput(_component, 0);
      Assert.NotNull(output);
      var circleBarGroup = (IArcGroup)output.Value.Group;
      Assert.NotNull(circleBarGroup);
      var layer = circleBarGroup.Layer as ILayerByBarCount;
      Assert.NotNull(layer);
      Assert.Equal(2, layer.Count);
      Assert.Equal(1, layer.BarBundle.CountPerBundle);
      Assert.Equal(20, layer.BarBundle.Diameter.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(10, circleBarGroup.Centre.Y.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(11, circleBarGroup.Centre.Z.As(LengthUnit.Millimeter), _comparer);
      Assert.Equal(Math.PI / 4, circleBarGroup.StartAngle.As(AngleUnit.Radian), _comparer);
      Assert.Equal(Math.PI / 2, circleBarGroup.SweepAngle.As(AngleUnit.Radian), _comparer);
    }


    [Fact]
    public void ShouldThrowExceptionWhenRebarGroupIsNull() {
      SetLineRebarInputs();
      ComponentTestHelper.SetInput(_component, new AdSecRebarGroupGoo(), 0);
      Assert.Throws<NullReferenceException>(() => ComponentTestHelper.ComputeData(_component));
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.Prestress));
    }

    [Fact]
    public void ShouldHavePluginInfoReferenced() {
      Assert.Equal(PluginInfo.Instance, _component.PluginInfo);
    }

    [Fact]
    public void ShouldBeHidden() {
      Assert.False(_component.Hidden);
    }
  }
}
