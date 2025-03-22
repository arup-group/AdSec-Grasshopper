using AdSecCore;
using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCoreTests.Functions {
  public class UlsResultFunctionTest {
    private readonly UlsResultFunction _component;
    private static SectionSolution? Solution { get; set; } = null;
    public UlsResultFunctionTest() {
      _component = new UlsResultFunction();
      if (Solution == null) {
        Solution = new SolutionBuilder().Build();
      }
      _component.SolutionInput.Value = Solution;
      _component.LoadInput.Value = ILoad.Create(Force.FromKilonewtons(-700), Moment.FromKilonewtonMeters(10), Moment.Zero);
    }

    [Fact]
    public void ShouldHaveCorrectMetadata() {
      Assert.Equal("Strength Result", _component.Metadata.Name);
      Assert.Equal("ULS", _component.Metadata.NickName);
      Assert.Equal("Performs strength checks (ULS), for a given Load or Deformation", _component.Metadata.Description);
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
      Assert.Equal(12, outputs.Length);
      Assert.IsType<LoadParameter>(outputs[0]);
      Assert.IsType<DoubleParameter>(outputs[1]);
      Assert.IsType<DeformationParameter>(outputs[2]);
      Assert.IsType<DoubleParameter>(outputs[3]);
      Assert.IsType<IntervalArrayParameter>(outputs[4]);
      Assert.IsType<NeutralLineParameter>(outputs[5]);
      Assert.IsType<DisplacementParameter>(outputs[6]);
      Assert.IsType<DoubleParameter>(outputs[7]);
      Assert.IsType<DeformationParameter>(outputs[8]);
      Assert.IsType<NeutralLineParameter>(outputs[9]);
      Assert.IsType<DisplacementParameter>(outputs[10]);
      Assert.IsType<DoubleParameter>(outputs[11]);
    }

    [Fact]
    public void ShoulHaveValidDeformationDescription() {
      _component.DeformationDescription(StrainUnit.Ratio, CurvatureUnit.PerMeter);
      var description = _component.DeformationOutput.Description;
      Assert.Contains("[εm⁻¹]", description);
      Assert.Contains("[εm⁻¹]", description);
    }

    [Fact]
    public void ShoulHaveValidFailureDeformationDescription() {
      _component.FailureDeformationDescription(StrainUnit.Ratio, CurvatureUnit.PerMeter);
      var description = _component.FailureDeformationOutput.Description;
      Assert.Contains("[εm⁻¹]", description);
      Assert.Contains("[εm⁻¹]", description);
    }

    [Fact]
    public void ShouldHaveValidMomentRangesDescription() {
      _component.MomentRangesDescription(MomentUnit.NewtonMeter);
      Assert.Contains("[N·m]", _component.MomentRangesOutput.Description);
    }

    [Fact]
    public void ShouldComputeCorrectly() {
      _component.Compute();
      var expectedLoad = ILoad.Create(Force.FromKilonewtons(-700), Moment.FromKilonewtonMeters(10), Moment.Zero);
      var expectedDeformation = IDeformation.Create(Strain.FromRatio(-0.00105), Curvature.FromPerMeters(-0.00125), Curvature.FromPerMeters(-8e-07));
      var expectedFailureDeformation = IDeformation.Create(Strain.FromRatio(-0.001742), Curvature.FromPerMeters(-0.0035), Curvature.FromPerMeters(9.07e-09));
      Assert.True(SlsResultFunctionTest.IsLoadEqual(expectedLoad, _component.LoadOutput.Value));
      Assert.True(SlsResultFunctionTest.IsDeformationEqual(expectedDeformation, _component.DeformationOutput.Value));
      Assert.True(SlsResultFunctionTest.IsDeformationEqual(expectedFailureDeformation, _component.FailureDeformationOutput.Value));
      Assert.Equal(0.8451, _component.LoadUtilOutput.Value, new DoubleComparer());
      Assert.Equal(0.6165, _component.DeformationUtilOutput.Value, new DoubleComparer());
      Assert.Equal(-20324.2652, _component.MomentRangesOutput.Value[0].Item1, new DoubleComparer());
      Assert.Equal(118601.9170, _component.MomentRangesOutput.Value[0].Item2, new DoubleComparer());
      Assert.Equal(-3.140, _component.NeutralAxisLineOutput.Value.Angle, new DoubleComparer());
      Assert.Equal(0.84071697296545067, _component.NeutralAxisLineOutput.Value.Offset.As(LengthUnit.Meter), new DoubleComparer());
      Assert.Equal(0.84071697296545067, _component.NeutralAxisOffsetOutput.Value.As(LengthUnit.Meter), new DoubleComparer());
      Assert.Equal(-3.140, _component.NeutralAxisAngleOutput.Value, new DoubleComparer());
      Assert.Equal(0.49967085191224453, _component.FailureNeutralAxisOffsetOutput.Value.As(LengthUnit.Meter), new DoubleComparer());
      Assert.Equal(3.140, _component.FailureNeutralAxisAngleOutput.Value, new DoubleComparer());
    }

    [Fact]
    public void ShouldLogWarningMessages() {
      _component.LoadInput.Value = ILoad.Create(Force.FromKilonewtons(-1000), Moment.FromKilonewtonMeters(10), Moment.Zero);
      _component.Compute();
      Assert.Equal(2, _component.WarningMessages.Count);
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
  }
}
