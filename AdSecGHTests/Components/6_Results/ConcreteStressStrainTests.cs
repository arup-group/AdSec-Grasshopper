using AdSecCore;
using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Components;

using AdSecGHTests.Helpers;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Oasys.AdSec;
using Oasys.Profiles;

using OasysGH.Parameters;
using OasysGH.Units;

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
    public void ShouldRefreshComponent() {
      var originalStrainUnit = DefaultUnits.StrainUnitResult;
      var originalStressUnit = DefaultUnits.StressUnitResult;
      DefaultUnits.StrainUnitResult = StrainUnit.MicroStrain;
      DefaultUnits.StressUnitResult = PressureUnit.Pascal;
      ComponentTestHelper.ComputeData(_component);
      Assert.Contains("[µε]", _component.Params.Output[0].Description);
      Assert.Contains("[µε]", _component.Params.Output[2].Description);
      Assert.Contains("[Pa]", _component.Params.Output[1].Description);
      Assert.Contains("[Pa]", _component.Params.Output[3].Description);
      DefaultUnits.StrainUnitResult = originalStrainUnit;
      DefaultUnits.StressUnitResult = originalStressUnit;
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
  }
}
