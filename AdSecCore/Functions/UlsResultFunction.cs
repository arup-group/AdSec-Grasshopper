using System;
using System.Collections.Generic;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public class UlsResultFunction : Function {
    public SectionSolutionParameter SolutionInput { get; set; } = new SectionSolutionParameter {
      Name = "Results",
      NickName = "Res",
      Description = "AdSec Results to perform strength check on",
      Access = Access.Item,
      Optional = false,
    };

    public GenericParameter LoadInput { get; set; } = new GenericParameter {
      Name = "Load",
      NickName = "Ld",
      Description = "AdSec Load (Load or Deformation) for which the strength results are to be calculated.",
      Access = Access.Item,
      Optional = false,
    };

    public LoadParameter LoadOutput { get; set; } = new LoadParameter {
      Name = "Load",
      NickName = "Ld",
      Description = $"The section load under the applied action.{Environment.NewLine}If the applied deformation is outside the capacity range of the section, the returned load will be zero.",
      Access = Access.Item,
    };

    public DoubleParameter LoadUtilOutput { get; set; } = new DoubleParameter {
      Name = "LoadUtil",
      NickName = "Ul",
      Description = $"The strength load utilisation is the ratio of the applied load to the load in the same direction that would cause the section to reach its capacity. Utilisation > 1 means the applied load exceeds the section capacity.{Environment.NewLine}If the applied load is outside the capacity range of the section, the utilisation will be greater than 1. Whereas, if the applied deformation exceeds the capacity, the load utilisation will be zero.",
      Access = Access.Item,
    };

    public DeformationParameter DeformationOutput { get; set; } = new DeformationParameter {
      Name = "Deformation",
      NickName = "Def",
      Description = "The section deformation under the applied action",
      Access = Access.Item,
    };

    public DoubleParameter DeformationUtilOutput { get; set; } = new DoubleParameter {
      Name = "DeformationUtil",
      NickName = "Ud",
      Description = $"The strength deformation utilisation is the ratio of the applied deformation to the deformation in the same direction that would cause the section to reach its capacity. Utilisation > 1 means capacity has been exceeded.{Environment.NewLine}Capacity has been exceeded when the utilisation is greater than 1. If the applied load is outside the capacity range of the section, the deformation utilisation will be the maximum double value.",
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

    public LengthParameter NeutralAxisOffsetOutput { get; set; } = new LengthParameter {
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

    public LengthParameter FailureNeutralAxisOffsetOutput { get; set; } = new LengthParameter {
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
        DeformationOutput,
        LoadOutput,
        LoadUtilOutput,
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

    public void DeformationDescription(StrainUnit strainUnit, CurvatureUnit curvatureUnit) {
      var strainAbbreviation = Strain.GetAbbreviation(strainUnit);
      var curvatureAbbreviation = $"{strainAbbreviation}{Curvature.GetAbbreviation(curvatureUnit)}";
      DeformationOutput.Description = $"The section deformation under the applied action. The output is a vector representing:{Environment.NewLine}X: Strain [{strainAbbreviation}]{Environment.NewLine}Y: Curvature around zz (so in local y-direction) [{curvatureAbbreviation}]{Environment.NewLine}Z: Curvature around yy (so in local z-direction) [{curvatureAbbreviation}]";
    }

    public override void Compute() {
      var momentUnit = ContextUnits.Instance.MomentUnit;
      var solution = SolutionInput.Value;
      IStrengthResult uls = null;
      IStrengthResult failure = null;
      switch (LoadInput.Value) {
        case ILoad load:
          uls = solution.Strength.Check(load);
          var failureLoad = ILoad.Create(
            load.X / uls.LoadUtilisation.DecimalFractions * 0.999,
            load.YY / uls.LoadUtilisation.DecimalFractions * 0.999,
            load.ZZ / uls.LoadUtilisation.DecimalFractions * 0.999);
          failure = solution.Strength.Check(failureLoad);
          break;
        case IDeformation def:
          uls = solution.Strength.Check(def);
          var failureDeformation = IDeformation.Create(
            def.X / uls.LoadUtilisation.DecimalFractions * 0.999,
            def.YY / uls.LoadUtilisation.DecimalFractions * 0.999,
            def.ZZ / uls.LoadUtilisation.DecimalFractions * 0.999);
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

      var tempOffset = new Length(offsetSI, ContextUnits.Instance.LengthUnitGeometry);

      // offset in user selected unit
      return tempOffset;
    }

  }
}
