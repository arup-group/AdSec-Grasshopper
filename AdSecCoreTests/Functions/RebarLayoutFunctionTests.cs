using AdSecCore;
using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

using static System.Runtime.InteropServices.JavaScript.JSType;

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

    void SetArcInput() {
      function.RebarLayoutOption = RebarLayoutOption.Arc;
      function.SpacedRebars.Value = _spacedRebars[0];
      function.CentreOfCircle.Value = IPoint.Create(Length.FromMillimeters(10), Length.FromMillimeters(11));
      function.RadiusOfCircle.Value = 2;
      function.StartAngle.Value = Math.PI / 4;
      function.SweepAngle.Value = Math.PI / 2;
    }

    [Fact]
    public void CanCreatesArcLayout() {
      SetArcInput();
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
    public void HaveWarningForZeroSweepAngle() {
      SetArcInput();
      function.SweepAngle.Value = 0;
      function.Compute();
      Assert.Single(function.WarningMessages);
    }

    [Fact]
    public void RadiusShouldBePositiveValue() {
      function.RebarLayoutOption = RebarLayoutOption.Circle;
      function.SpacedRebars.Value = _spacedRebars[0];
      function.CentreOfCircle.Value = IPoint.Create(Length.FromMillimeters(10), Length.FromMillimeters(11));
      function.RadiusOfCircle.Value = -2;
      function.StartAngle.Value = Math.PI / 4;
      function.SweepAngle.Value = Math.PI / 2;
      Assert.Throws<ArgumentOutOfRangeException>(() => function.Compute());
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

    [Theory]
    [InlineData(3, (int)RebarLayoutOption.Line)]
    [InlineData(2, (int)RebarLayoutOption.SingleBars)]
    [InlineData(4, (int)RebarLayoutOption.Circle)]
    [InlineData(5, (int)RebarLayoutOption.Arc)]
    public void GetAllInputAttributesReturnsTwoParameters(int count, int layoutOption) {
      function.RebarLayoutOption = (RebarLayoutOption)layoutOption;
      var inputs = function.GetAllInputAttributes();
      Assert.Equal(count, inputs.Length);
    }

    [Fact]
    public void GetAllOutputAttributesReturnsOneParameter() {
      var outputs = function.GetAllOutputAttributes();
      Assert.Single(outputs);
    }

    [Fact]
    public void ParametersHaveCorrectNames() {
      Assert.Equal("Spaced Rebars", function.SpacedRebars.Name);
      Assert.Equal("Rebar", function.RebarBundle.Name);
      Assert.Equal("Centre", function.CentreOfCircle.Name);
      Assert.Equal("Radius", function.RadiusOfCircle.Name);
      Assert.Equal("StartAngle", function.StartAngle.Name);
      Assert.Equal("SweepAngle", function.SweepAngle.Name);
      Assert.Equal("Position(s)", function.Positions.Name);
      Assert.Equal("Position 1", function.Position1.Name);
      Assert.Equal("Position 2", function.Position2.Name);
    }

    [Fact]
    public void ParametersHaveCorrectNickNames() {
      Assert.Equal("RbS", function.SpacedRebars.NickName);
      Assert.Equal("Rb", function.RebarBundle.NickName);
      Assert.Equal("CVx", function.CentreOfCircle.NickName);
      Assert.Equal("r", function.RadiusOfCircle.NickName);
      Assert.Equal("s°", function.StartAngle.NickName);
      Assert.Equal("e°", function.SweepAngle.NickName);
      Assert.Equal("Vxs", function.Positions.NickName);
      Assert.Equal("Vx1", function.Position1.NickName);
      Assert.Equal("Vx2", function.Position2.NickName);
    }

    [Fact]
    public void ParametersHaveCorrectDescriptionNames() {
      Assert.Contains("AdSec Rebars Spaced in a Layer", function.SpacedRebars.Description);
      Assert.Contains("AdSec Rebar (single or bundle)", function.RebarBundle.Description);
      Assert.Contains("Vertex Point representing the centre of the circle", function.CentreOfCircle.Description);
      Assert.Contains("Distance representing the radius of the circle", function.RadiusOfCircle.Description);
      Assert.Contains("The starting angle of the circle", function.StartAngle.Description);
      Assert.Contains("The angle sweeped by the arc", function.SweepAngle.Description);
      Assert.Contains("List of bar positions", function.Positions.Description);
      Assert.Contains("First bar position", function.Position1.Description);
      Assert.Contains("Last bar position", function.Position2.Description);
    }

    [Fact]
    public void ParametersHaveCorrecOptionalValue() {
      Assert.False(function.SpacedRebars.Optional);
      Assert.False(function.RebarBundle.Optional);
      Assert.True(function.CentreOfCircle.Optional);
      Assert.False(function.RadiusOfCircle.Optional);
      Assert.True(function.StartAngle.Optional);
      Assert.True(function.SweepAngle.Optional);
      Assert.False(function.Positions.Optional);
      Assert.False(function.Position1.Optional);
      Assert.False(function.Position2.Optional);
    }


    [Fact]
    public void OrganisationHasCorrectValues() {
      Assert.Equal("AdSec", function.Organisation.Category);
      Assert.Equal("Rebar", function.Organisation.SubCategory.Trim());
    }

    [Fact]
    public void MetaDataHasCorrectValues() {
      Assert.Equal("Create Reinforcement Layout", function.Metadata.Name);
      Assert.Equal("Reinforcement Layout", function.Metadata.NickName);
      Assert.Contains("Create a Reinforcement Layout for an AdSec Section", function.Metadata.Description);
    }
  }
}
