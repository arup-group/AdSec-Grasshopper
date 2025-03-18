using System;
using System.Collections.Generic;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {

  public class UlsResultFunction : SlsResultFunction {

    public DoubleParameter LoadUtilOutput { get; set; } = new DoubleParameter {
      Name = "LoadUtil",
      NickName = "Ul",
      Description = CommonDescriptions.DefaultUtilizationDescription,
      Access = Access.Item,
    };

    public DoubleParameter DeformationUtilOutput { get; set; } = new DoubleParameter {
      Name = "DeformationUtil",
      NickName = "Ud",
      Description = CommonDescriptions.DefaultUtilizationDescription,
      Access = Access.Item,
    };

    public IntervalArrayParameter MomentRangesOutput { get; set; } = new IntervalArrayParameter {
      Name = "Moment Ranges",
      NickName = "MRs",
      Description = "The range of moments within the section's capacity",
      Access = Access.List,
    };

    public GenericParameter NeutralAxisLineOutput { get; set; } = new GenericParameter {
      Name = "Neutral Axis",
      NickName = "NaL",
      Description = "Line of Neutral Axis",
      Access = Access.Item,
    };

    public DisplacementParameter NeutralAxisOffsetOutput { get; set; } = new DisplacementParameter {
      Name = "Neutral Axis Offset",
      NickName = "NaO",
      Description = "The Offset of the Neutral Axis from the Sections centroid",
      Access = Access.Item,
    };

    public DoubleParameter NeutralAxisAngleOutput { get; set; } = new DoubleParameter {
      Name = "Neutral Axis Angle",
      NickName = "NaA",
      Description = "The Angle [rad] of the Neutral Axis from the Sections centroid",
      Access = Access.Item,
    };

    public DeformationParameter FailureDeformationOutput { get; set; } = new DeformationParameter {
      Name = "Failure Deformation",
      NickName = "DU",
      Description = "The section deformation at failure",
      Access = Access.Item,
    };

    public GenericParameter FailureNeutralAxisLineOutput { get; set; } = new GenericParameter {
      Name = "Failure Neutral Axis",
      NickName = "FaL",
      Description = "Line of Neutral Axis at failure",
      Access = Access.Item,
    };

    public DisplacementParameter FailureNeutralAxisOffsetOutput { get; set; } = new DisplacementParameter {
      Name = "Failure Neutral Axis Offset",
      NickName = "FaO",
      Description = "The Offset of the Neutral Axis at failure from the Sections centroid",
      Access = Access.Item,
    };

    public DoubleParameter FailureNeutralAxisAngleOutput { get; set; } = new DoubleParameter {
      Name = "Failure Neutral Axis Angle",
      NickName = "FaA",
      Description = "The Angle [rad] of the Neutral Axis at failure from the Sections centroid",
      Access = Access.Item,
    };

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
      var momentUnit = ContextUnits.Instance.MomentUnit;
      var solution = SolutionInput.Value;
      IStrengthResult uls = null;
      IStrengthResult failure = null;
      double adjustmentFactor = 0.999;
      switch (LoadInput.Value) {
        case ILoad load:
          uls = solution.Strength.Check(load);
          var failureLoad = ILoad.Create(
            load.X / uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
            load.YY / uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
            load.ZZ / uls.LoadUtilisation.DecimalFractions * adjustmentFactor);
          failure = solution.Strength.Check(failureLoad);
          break;
        case IDeformation def:
          uls = solution.Strength.Check(def);
          var failureDeformation = IDeformation.Create(
            def.X / uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
            def.YY / uls.LoadUtilisation.DecimalFractions * adjustmentFactor,
            def.ZZ / uls.LoadUtilisation.DecimalFractions * adjustmentFactor);
          failure = solution.Strength.Check(failureDeformation);
          break;
        default:
          ErrorMessages.Add("Invalid Load Input");
          return;
      }

      LoadOutput.Value = uls.Load;
      double util = uls.LoadUtilisation.As(RatioUnit.DecimalFraction);
      LoadUtilOutput.Value = util;
      if (util > 1) {
        WarningMessages.Add("Load utilisation is above 1!");
      }

      DeformationOutput.Value = uls.Deformation;
      double defUtil = uls.DeformationUtilisation.As(RatioUnit.DecimalFraction);
      DeformationUtilOutput.Value = defUtil;
      if (defUtil > 1) {
        WarningMessages.Add("Deformation utilisation is above 1!");
      }

      var momentRanges = new List<Tuple<double, double>>();
      foreach (var range in uls.MomentRanges) {
        momentRanges.Add(new Tuple<double, double>(
          range.Min.As(momentUnit),
          range.Max.As(momentUnit)));
      }
      var offset = CalculateOffset(uls.Deformation);
      var angle = CalculateAngle(uls.Deformation);
      MomentRangesOutput.Value = momentRanges.ToArray();
      NeutralAxisLineOutput.Value = new NeutralLine { Angle = angle, Offset = offset, Solution = solution };
      NeutralAxisOffsetOutput.Value = offset;
      NeutralAxisAngleOutput.Value = angle;

      FailureDeformationOutput.Value = failure.Deformation;
      var failureOffset = CalculateOffset(failure.Deformation);
      var failureAngle = CalculateAngle(failure.Deformation);
      FailureNeutralAxisLineOutput.Value = new NeutralLine { Angle = failureAngle, Offset = failureOffset, Solution = solution };
      FailureNeutralAxisOffsetOutput.Value = failureOffset;
      FailureNeutralAxisAngleOutput.Value = failureAngle;
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
      if (double.IsNaN(offsetSI)) {
        offsetSI = 0.0;
      }

      // temp length in SI units
      var tempOffset = new Length(offsetSI, LengthUnit.Meter);

      return tempOffset;
    }

  }
}
