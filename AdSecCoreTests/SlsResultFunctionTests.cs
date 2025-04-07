using AdSecCore;
using AdSecCore.Builders;
using AdSecCore.Extensions;
using AdSecCore.Functions;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class SlsResultFunctionTest {
    private readonly SlsResultFunction _component;
    private static SectionSolution? Solution { get; set; } = null;
    private readonly DoubleComparer comparer = new DoubleComparer();
    public SlsResultFunctionTest() {
      _component = new SlsResultFunction();
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      _component.SolutionInput.Value = Solution;
      _component.LoadInput.Value = ILoad.Create(Force.FromKilonewtons(100), Moment.FromKilonewtonMeters(100), Moment.Zero);
    }

    [Fact]
    public void ShouldHaveCorrectMetadata() {
      Assert.Equal("Serviceability Result", _component.Metadata.Name);
      Assert.Equal("SLS", _component.Metadata.NickName);
      Assert.Equal("Performs serviceability analysis (SLS), for a given Load or Deformation.", _component.Metadata.Description);
    }


    [Fact]
    public void ShouldHaveCorrectInputAttributes() {
      var inputs = _component.GetAllInputAttributes();
      Assert.Equal(2, inputs.Length);
      Assert.IsType<SectionSolutionParameter>(inputs[0]);
      Assert.IsType<GenericParameter>(inputs[1]);
    }

    [Fact]
    public void ShouldHaveCorrectOutputAttributes() {
      var outputs = _component.GetAllOutputAttributes();
      Assert.Equal(7, outputs.Length);
      Assert.IsType<LoadParameter>(outputs[0]);
      Assert.IsType<CrackArrayParameter>(outputs[1]);
      Assert.IsType<CrackParameter>(outputs[2]);
      Assert.IsType<DoubleParameter>(outputs[3]);
      Assert.IsType<DeformationParameter>(outputs[4]);
      Assert.IsType<SecantStiffnessParameter>(outputs[5]);
      Assert.IsType<IntervalArrayParameter>(outputs[6]);
    }

    [Fact]
    public void ShoulHaveValidDeformationDescription() {
      _component.StrainUnitResult = StrainUnit.MicroStrain;
      _component.StressUnitResult = PressureUnit.Pascal;
      _component.UpdateOutputDescription();
      var description = _component.DeformationOutput.Description;
      Assert.Contains("[µε]", description);
      Assert.Contains("[Pam⁻¹]", description);
    }

    [Fact]
    public void ShouldHaveValidSecantStiffnessDescription() {
      _component.UpdateOutputDescription();
      var description = _component.SecantStiffnessOutput.Description;
      Assert.Contains("[N]", description);
      Assert.Contains("[N·m²]", description);
      Assert.Contains("[N·m²]", description);
    }

    [Fact]
    public void ShouldHaveValidUncrackedMomentRangesDescription() {
      _component.UpdateOutputDescription();
      Assert.Contains("[N·m]", _component.UncrackedMomentRangesOutput.Description);
    }

    [Fact]
    public void ShouldComputeCorrectly() {
      _component.Compute();
      var expectedLoad = ILoad.Create(Force.FromKilonewtons(100), Moment.FromKilonewtonMeters(100), Moment.Zero);
      var expectedDeformation = IDeformation.Create(Strain.FromRatio(0.0014), Curvature.FromPerMeters(0.0064), Curvature.FromPerMeters(0.0029));
      Assert.True(IsLoadEqual(expectedLoad, _component.LoadOutput.Value));
      Assert.True(IsDeformationEqual(expectedDeformation, _component.DeformationOutput.Value));
      Assert.Equal(0.00208, _component.MaximumCrackOutput.Value.Load.Width.Value, comparer);
      Assert.Equal(5.8156, _component.CrackUtilOutput.Value, comparer);
      Assert.Single(_component.RemarkMessages);
      Assert.Equal(69, _component.CrackOutput.Value.Length);
    }

    public static bool IsLoadEqual(ILoad expected, ILoad calculated) {
      return expected.X.Value.Equals(calculated.X.Value) && expected.YY.Value.Equals(calculated.YY.Value) && expected.ZZ.Value.Equals(calculated.ZZ.Value);
    }

    public static bool IsDeformationEqual(IDeformation expected, IDeformation calculated) {
      var tolernaceStrain = Strain.FromRatio(0.00001);
      var tolernaceCurvature = Curvature.FromPerMeters(0.0001);
      return expected.X.Equals(calculated.X, tolernaceStrain) && expected.YY.Equals(calculated.YY, tolernaceCurvature) && expected.ZZ.Equals(calculated.ZZ, tolernaceCurvature);
    }

    [Fact]
    public void ShouldHaveWarningForInvalidLoad() {
      _component.LoadInput.Value = ILoad.Create(Force.Zero, Moment.Zero, Moment.Zero);
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveWarningForInvalidDeformation() {
      _component.LoadInput.Value = IDeformation.Create(Strain.Zero, Curvature.Zero, Curvature.Zero);
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveErrorMessageForNullSolution() {
      _component.SolutionInput.Value = null;
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void ShouldHaveErrorMessageForNullLoad() {
      _component.LoadInput.Value = null;
      _component.Compute();
      Assert.Single(_component.ErrorMessages);
    }

    [Fact]
    public void NullDeformationShouldBeInvalid() {
      ILoad? load = null;
      Assert.False(LoadExtensions.IsValid(load));
    }

    [Fact]
    public void NullLoadShouldBeInvalid() {
      IDeformation? deformation = null;
      Assert.False(LoadExtensions.IsValid(deformation));
    }
  }
}
