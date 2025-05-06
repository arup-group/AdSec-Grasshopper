using System;

using AdSecCore.Extensions;

using Oasys.AdSec;

using OasysUnits;

namespace AdSecCore.Functions {

  public abstract class ResultFunction : Function {

    public SectionSolutionParameter SolutionInput { get; set; } = new SectionSolutionParameter {
      Name = "Results",
      NickName = "Res",
      Description = "AdSec Results to perform strength check",
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
      Optional = false,
    };

    public CrackArrayParameter CrackOutput { get; set; } = new CrackArrayParameter {
      Name = "Cracks",
      NickName = "Crks",
      Description = $"Crack results are calculated at bar positions or section surfaces depending on the Design Code specifications.{Environment.NewLine}If the applied action is outside the capacity range of the section, the returned list will be empty.",
      Access = Access.List,
    };

    public CrackParameter MaximumCrackOutput { get; set; } = new CrackParameter {
      Name = "MaximumCrack",
      NickName = "MaxCrk",
      Description = "MaximumCrack output for the crack result corresponding to the maximum crack width.",
      Access = Access.Item,
    };

    public DoubleParameter CrackUtilOutput { get; set; } = new DoubleParameter {
      Name = "CrackUtil",
      NickName = "Uc",
      Description = $"The ratio of the applied load (moment and axial) to the load (moment and axial) in the same direction that would cause the section to crack. Ratio > 1 means section is cracked.{Environment.NewLine}The section is cracked when the cracking utilisation ratio is greater than 1. If the applied load is outside the capacity range of the section, the cracking utilisation will be maximum double value.",
      Access = Access.Item,
    };

    public DeformationParameter DeformationOutput { get; set; } = new DeformationParameter {
      Name = "Deformation",
      NickName = "Def",
      Description = "The section deformation under the applied action",
      Access = Access.Item,
    };

    public SecantStiffnessParameter SecantStiffnessOutput { get; set; } = new SecantStiffnessParameter {
      Name = "SecantStiffness",
      NickName = "Es",
      Description = "The secant stiffness under the applied action",
      Access = Access.Item,
    };

    public IntervalArrayParameter UncrackedMomentRangesOutput { get; set; } = new IntervalArrayParameter {
      Name = "Uncracked Moment Ranges",
      NickName = "Mrs",
      Description = "The range of moments",
      Access = Access.List,
    };

    public DoubleParameter LoadUtilOutput { get; set; } = new DoubleParameter {
      Name = "LoadUtil",
      NickName = "Ul",
      Description = $"The strength load utilisation is the ratio of the applied load to the load in the same direction that would cause the section to reach its capacity. Utilisation > 1 means the applied load exceeds the section capacity.{Environment.NewLine}If the applied load is outside the capacity range of the section, the utilisation will be greater than 1. Whereas, if the applied deformation exceeds the capacity, the load utilisation will be zero.",
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

    public NeutralLineParameter NeutralAxisLineOutput { get; set; } = new NeutralLineParameter {
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

    public NeutralLineParameter FailureNeutralAxisLineOutput { get; set; } = new NeutralLineParameter {
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
      Name = "Find Crack Load",
      NickName = "CrackLd",
      Description = "Increases the load until set crack width is reached",
    };

    protected override void UpdateParameter() {
      base.UpdateParameter();
      string strainUnitAbbreviation = Strain.GetAbbreviation(StrainUnitResult);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(StressUnitResult);
      var curvatureAbbreviation = $"{stressUnitAbbreviation}{Curvature.GetAbbreviation(CurvatureUnit)}";
      DeformationOutput.Description = $"The section deformation under the applied action. The output is a vector representing:{Environment.NewLine}X: Strain [{strainUnitAbbreviation}]{Environment.NewLine}Y: Curvature around zz (so in local y-direction) [{curvatureAbbreviation}]{Environment.NewLine}Z: Curvature around yy (so in local z-direction) [{curvatureAbbreviation}]";
      FailureDeformationOutput.Description = $"The section deformation at failure. The output is a vector representing:{Environment.NewLine}X: Strain [{strainUnitAbbreviation}],{Environment.NewLine}Y: Curvature around zz (so in local y-direction) [{curvatureAbbreviation}],{Environment.NewLine}Z: Curvature around yy (so in local z-direction) [{curvatureAbbreviation}]";
      SecantStiffnessOutput.Description = $"The secant stiffness under the applied action. The output is a vector representing:{Environment.NewLine}X: Axial stiffness [{AxialStiffness.GetAbbreviation(AxialStiffnessUnit)}],{Environment.NewLine}Y: The bending stiffness about the y-axis in the local coordinate system [{BendingStiffness.GetAbbreviation(BendingStiffnessUnit)}],{Environment.NewLine}Z: The bending stiffness about the z-axis in the local coordinate system [{BendingStiffness.GetAbbreviation(BendingStiffnessUnit)}]";
      UncrackedMomentRangesOutput.Description = $"The range of moments (in the direction of the applied moment, assuming constant axial force) over which the section remains uncracked. Moment values are in [{Moment.GetAbbreviation(MomentUnit)}]";
      MomentRangesOutput.Description = UncrackedMomentRangesOutput.Description;
    }

    protected virtual bool ValidateLoad() {
      switch (LoadInput.Value) {
        case ILoad load:
          if (!LoadExtensions.IsValid(load)) {
            ErrorMessages.Add("Load Input should be finite number. Zero load has no boundary");
            return false;
          }
          break;
        case IDeformation def:
          if (!LoadExtensions.IsValid(def)) {
            ErrorMessages.Add("Deformation Input should be finite number. Zero deformation has no boundary");
            return false;
          }
          break;
        default:
          ErrorMessages.Add("Invalid Load Input");
          return false;
      }
      return true;
    }

    public override bool ValidateInputs() {
      if (!base.ValidateInputs() || !ValidateLoad()) {
        return false;
      }
      return true;
    }
  }
}
