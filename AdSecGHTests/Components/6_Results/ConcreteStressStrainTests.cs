
using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Properties;

using AdSecGHTests.Helpers;

using Oasys.AdSec;
using Oasys.GH.Helpers;
using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

using Xunit;

namespace AdSecGHTests.Components {
  [Collection("GrasshopperFixture collection")]
  public class ConcreteStressStrainTests {
    private readonly ConcreteStressStrain _component;
    private static SectionSolution Solution { get; set; } = null;
    public ConcreteStressStrainTests() {
      _component = new ConcreteStressStrain();
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      ComponentTestHelper.SetInput(_component, Solution, 0);
      ComponentTestHelper.SetInput(_component, ILoad.Create(Force.FromKilonewtons(-600), Moment.FromKilonewtonMeters(50), Moment.Zero), 1);
      ComponentTestHelper.SetInput(_component, IPoint.Create(new Length(120, LengthUnit.Millimeter), new Length(280, LengthUnit.Millimeter)), 2);

    }

    private void SetLoad() {
      ComponentTestHelper.SetInput(_component, ILoad.Create(Force.FromKilonewtons(-600), Moment.FromKilonewtonMeters(50), Moment.Zero), 1);
      ComponentTestHelper.ComputeData(_component);
    }

    private void SetDeformation() {
      ComponentTestHelper.SetInput(_component, IDeformation.Create(Strain.FromRatio(-0.003), Curvature.Zero, Curvature.Zero), 1);
      ComponentTestHelper.ComputeData(_component);
    }

    [Fact]
    public void ShouldComputeCorrectlyForLoad() {
      SetLoad();
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 0));
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 1));
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 2));
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 3));
    }

    [Fact]
    public void ShouldComputeCorrectlyForDeformation() {
      SetDeformation();
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 0));
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 1));
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 2));
      Assert.NotNull(ComponentTestHelper.GetOutput(_component, 3));
    }

    [Fact]
    public void ShouldHaveIconReferenced() {
      Assert.True(_component.MatchesExpectedIcon(Resources.StressStrainRebar));
    }

  }
}
