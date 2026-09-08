using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class CreateSectionFunctionTests {

    readonly CreateSectionFunction function;

    public CreateSectionFunctionTests() {
      function = new CreateSectionFunction();

      function.Profile.Value = new ProfileDesign() {
        Profile = new ProfileBuilder().WidthDepth(1).WithWidth(2).Build()
      };
      function.Material.Value = new MaterialDesign() {
        Material = Concrete.IS456.Edition_2000.M10,
        DesignCode = new DesignCode() {
          IDesignCode = IS456.Edition_2000,
        },
      };
    }

    [Fact]
    public void ShouldHaveFourInputs() {
      Assert.Equal(4, function.GetAllInputAttributes().Length);
    }

    [Fact]
    public void ShouldHaveOneOutput() {
      Assert.Single(function.GetAllOutputAttributes());
    }

    [Fact]
    public void ShouldProduceAValidSection() {
      function.Compute();
      Assert.NotNull(function.Section.Value);
    }

    [Fact]
    public void ShouldComputeWithRebarGroup() {
      function.Profile.Value = new ProfileDesign() {
        Profile = SectionBuilder.SimplePerimeterProfile(20, 20)
      };
      var singleBars = new BuilderSingleBar().AtPosition(Geometry.Zero()).WithSize(2)
       .AtPosition(Geometry.Zero()).Build();
      var rebarOriginal = new List<AdSecRebarGroup> {
        new() {
          Group = singleBars,
        },
      };
      function.RebarGroup.Value = rebarOriginal.ToArray();
      function.Compute();
      Assert.NotNull(function.Section.Value);
    }

    [Fact]
    public void ShouldOffsetRebarGroupsUsingProfileOffsets() {
      function.Profile.Value = new ProfileDesign() {
        Profile = SectionBuilder.SimplePerimeterProfile(20, 20),
        OffsetY = new Length(25, LengthUnit.Millimeter),
        OffsetZ = new Length(-10, LengthUnit.Millimeter),
      };

      var singleBars = new BuilderSingleBar().AtPosition(Geometry.Zero()).WithSize(2)
       .AtPosition(Geometry.Zero()).Build();
      function.RebarGroup.Value = new[] {
        new AdSecRebarGroup {
          Group = singleBars,
        },
      };

      function.Compute();

      var resultBars = Assert.IsAssignableFrom<ISingleBars>(function.Section.Value.Section.ReinforcementGroups[0]);
      Assert.Equal(25, resultBars.Positions[0].Y.Millimeters, 6);
      Assert.Equal(-10, resultBars.Positions[0].Z.Millimeters, 6);
    }

    [Fact]
    public void ShouldPropagateGlobalPlaneToSectionDesign() {
      function.Profile.Value = new ProfileDesign() {
        Profile = new ProfileBuilder().WidthDepth(1).WithWidth(2).Build(),
        GlobalPlane = OasysPlane.PlaneXY,
      };

      function.Compute();

      Assert.Equal(OasysPlane.PlaneXY, function.Section.Value.GlobalPlane);
    }

    [Fact]
    public void ShouldUseTheMaterial() {
      function.Compute();
      Assert.Equal(function.Section.Value.Section.Material.GetType(), Concrete.IS456.Edition_2000.M10.GetType());
    }
  }
}
