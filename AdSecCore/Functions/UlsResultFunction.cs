using System;
using System.Linq;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {

  public class UlsResultFunction : ResultFunction {
    private IStrengthResult Uls { get; set; }
    private IStrengthResult Failure { get; set; }
    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Strength Result",
      NickName = "ULS",
      Description = "Performs strength checks (ULS), for a given Load or Deformation",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat7(),
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        SolutionInput,
        LoadInput,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        LoadOutput,
        LoadUtilOutput,
        DeformationOutput,
        DeformationUtilOutput,
        MomentRangesOutput,
        NeutralAxisLineOutput,
        NeutralAxisOffsetOutput,
        NeutralAxisAngleOutput,
        FailureDeformationOutput,
        FailureNeutralAxisLineOutput,
        FailureNeutralAxisOffsetOutput,
        FailureNeutralAxisAngleOutput,
      };
    }

    public override void Compute() {
      if (!ValidateInputs()) {
        return;
      }

      ProcessInput();

      ProcessOutput();
    }

    private static double CalculateAngle(IDeformation ulsDeformationResult) {
      double kYY = ulsDeformationResult.YY.As(CurvatureUnit.PerMeter);
      double kZZ = ulsDeformationResult.ZZ.As(CurvatureUnit.PerMeter);
      return Math.Atan2(kZZ, kYY);
    }

    private static Length CalculateOffset(IDeformation ulsDeformationResult) {
      // neutral line
      double defX = ulsDeformationResult.X.As(StrainUnit.Ratio);
      double kYY = ulsDeformationResult.YY.As(CurvatureUnit.PerMeter);
      double kZZ = ulsDeformationResult.ZZ.As(CurvatureUnit.PerMeter);

      // compute offset
      double offsetSI = -defX / Math.Sqrt(Math.Pow(kYY, 2) + Math.Pow(kZZ, 2));

      // temp length in SI units
      var tempOffset = new Length(offsetSI, LengthUnit.Meter);

      return tempOffset;
    }


    private void ProcessInput() {
      const double adjustmentFactor = 0.999;
      switch (LoadInput.Value) {
        case ILoad load:
          Uls = SolutionInput.Value.Strength.Check(load);
          var failureLoad = ILoad.Create(
              load.X / Uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
              load.YY / Uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
              load.ZZ / Uls.LoadUtilisation.DecimalFractions * adjustmentFactor);
          Failure = SolutionInput.Value.Strength.Check(failureLoad);
          break;
        case IDeformation def:
          Uls = SolutionInput.Value.Strength.Check(def);
          var failureDeformation = IDeformation.Create(
              def.X / Uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
              def.YY / Uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
              def.ZZ / Uls.LoadUtilisation.DecimalFractions * adjustmentFactor);
          Failure = SolutionInput.Value.Strength.Check(failureDeformation);
          break;
        default:
          break;
      }
    }

    private void ProcessOutput() {
      //load output
      LoadOutput.Value = Uls.Load;

      //load utilisation
      double util = Uls.LoadUtilisation.As(RatioUnit.DecimalFraction);
      LoadUtilOutput.Value = util;
      if (util > 1) {
        WarningMessages.Add("Load utilisation is above 1!");
      }

      //deformation output
      DeformationOutput.Value = Uls.Deformation;

      //deformation utilisation
      double defUtil = Uls.DeformationUtilisation.As(RatioUnit.DecimalFraction);
      DeformationUtilOutput.Value = defUtil;
      if (defUtil > 1) {
        WarningMessages.Add("Deformation utilisation is above 1!");
      }

      // uls moment ranges
      var momentRanges = Uls.MomentRanges.Select(range =>
         new Tuple<double, double>(
             range.Min.As(MomentUnit),
             range.Max.As(MomentUnit)))
         .ToList();
      MomentRangesOutput.Value = momentRanges.ToArray();

      //uls neutral axis
      NeutralAxisLineOutput.Value = GetNeutralAxisResults(Uls.Deformation);
      NeutralAxisOffsetOutput.Value = NeutralAxisLineOutput.Value.Offset;
      NeutralAxisAngleOutput.Value = NeutralAxisLineOutput.Value.Angle;

      //failure deformation
      FailureDeformationOutput.Value = Failure.Deformation;

      // failure neutral Line
      FailureNeutralAxisLineOutput.Value = GetNeutralAxisResults(Failure.Deformation);
      FailureNeutralAxisOffsetOutput.Value = FailureNeutralAxisLineOutput.Value.Offset;
      FailureNeutralAxisAngleOutput.Value = FailureNeutralAxisLineOutput.Value.Angle;

    }


    private NeutralAxis GetNeutralAxisResults(IDeformation deformationResult) {
      var ulsDeformationResult = deformationResult;
      var offset = CalculateOffset(ulsDeformationResult).ToUnit(LengthUnitResult);
      var angle = CalculateAngle(ulsDeformationResult);

      return new NeutralAxis {
        Angle = angle,
        Offset = offset,
        Solution = SolutionInput.Value
      };
    }

  }
}
