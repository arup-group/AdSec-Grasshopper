using System;
using System.Linq;

using AdSecCore.Extensions;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {

  public class UlsResultFunction : ResultFunction {

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
      var solution = SolutionInput.Value;

      if (!CalculateStrengthResults(solution, out var uls, out var failure)) {
        return;
      }

      ProcessLoadResults(uls);

      ProcessDeformationResults(uls);

      ProcessMomentRanges(uls);

      ProcessNeutralAxisResults(uls, solution);

      ProcessFailureResults(failure, solution);
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


    private bool CalculateStrengthResults(SectionSolution solution, out IStrengthResult uls, out IStrengthResult failure) {
      uls = null;
      failure = null;
      const double adjustmentFactor = 0.999;
      switch (LoadInput.Value) {
        case ILoad load:
          if (!ILoadExtensions.IsValid(load, this)) {
            return false;
          }
          uls = solution.Strength.Check(load);
          var failureLoad = ILoad.Create(
              load.X / uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
              load.YY / uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
              load.ZZ / uls.LoadUtilisation.DecimalFractions * adjustmentFactor);
          failure = solution.Strength.Check(failureLoad);
          break;
        case IDeformation def:
          if (!IDeformationExtensions.IsValid(def, this)) {
            return false;
          }
          uls = solution.Strength.Check(def);
          var failureDeformation = IDeformation.Create(
              def.X / uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
              def.YY / uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
              def.ZZ / uls.LoadUtilisation.DecimalFractions * adjustmentFactor);
          failure = solution.Strength.Check(failureDeformation);
          break;
        default:
          ErrorMessages.Add("Invalid Load Input");
          return false;
      }
      return true;
    }

    private void ProcessLoadResults(IStrengthResult uls) {
      LoadOutput.Value = uls.Load;
      double util = uls.LoadUtilisation.As(RatioUnit.DecimalFraction);
      LoadUtilOutput.Value = util;
      if (util > 1) {
        WarningMessages.Add("Load utilisation is above 1!");
      }
    }

    private void ProcessDeformationResults(IStrengthResult uls) {
      DeformationOutput.Value = uls.Deformation;
      double defUtil = uls.DeformationUtilisation.As(RatioUnit.DecimalFraction);
      DeformationUtilOutput.Value = defUtil;
      if (defUtil > 1) {
        WarningMessages.Add("Deformation utilisation is above 1!");
      }
    }

    private void ProcessMomentRanges(IStrengthResult uls) {

      var momentRanges = uls.MomentRanges.Select(range =>
          new Tuple<double, double>(
              range.Min.As(MomentUnit),
              range.Max.As(MomentUnit)))
          .ToList();
      MomentRangesOutput.Value = momentRanges.ToArray();
    }

    private void ProcessNeutralAxisResults(IStrengthResult uls, SectionSolution solution) {
      var offset = CalculateOffset(uls.Deformation).ToUnit(LengthUnitResult);
      var angle = CalculateAngle(uls.Deformation);

      NeutralAxisLineOutput.Value = new NeutralAxis {
        Angle = angle,
        Offset = offset,
        Solution = solution
      };
      NeutralAxisOffsetOutput.Value = offset;
      NeutralAxisAngleOutput.Value = angle;
    }

    private void ProcessFailureResults(IStrengthResult failure, SectionSolution solution) {
      FailureDeformationOutput.Value = failure.Deformation;
      var failureOffset = CalculateOffset(failure.Deformation).ToUnit(LengthUnitResult);
      var failureAngle = CalculateAngle(failure.Deformation);

      FailureNeutralAxisLineOutput.Value = new NeutralAxis {
        Angle = failureAngle,
        Offset = failureOffset,
        Solution = solution
      };
      FailureNeutralAxisOffsetOutput.Value = failureOffset;
      FailureNeutralAxisAngleOutput.Value = failureAngle;
    }

  }
}
