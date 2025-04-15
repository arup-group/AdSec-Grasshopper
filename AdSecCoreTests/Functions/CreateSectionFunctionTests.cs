using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Parameters;

using AdSecGHCore;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

namespace AdSecCoreTests.Functions {
  public class CreateSectionFunctionTests {

    readonly CreateSectionFunction function;
    readonly IReinforcement rebarGrade = Reinforcement.Steel.IS456.Edition_2000.S415;
    public CreateSectionFunctionTests() {
      function = new CreateSectionFunction();

      function.Profile.Value = new ProfileDesign() {
        Profile = new ProfileBuilder().WidthDepth(1).WithWidth(2).Build()
      };
      function.Material.Value = new MaterialDesign() {
        Material = Concrete.IS456.Edition_2000.M10,
        DesignCode = IS456.Edition_2000,
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
      var singleBars = new BuilderSingleBar().AtPosition(Geometry.Zero()).WithSize(2).CreateSingleBar()
       .AtPosition(Geometry.Zero()).Build();

      var rebarOriginal = new List<AdSecRebarGroup> {
        new AdSecRebarGroup(singleBars, MaterialHelper.FindPath(rebarGrade)),
      };
      function.RebarGroup.Value = rebarOriginal.ToArray();
      function.Compute();
      Assert.NotNull(function.Section.Value);
    }

    [Fact]
    public void ShouldUseTheMaterial() {
      function.Compute();
      Assert.Equal(function.Section.Value.Section.Material.GetType(), Concrete.IS456.Edition_2000.M10.GetType());
    }
  }
}
