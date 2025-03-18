
using System;
using System.Collections.Generic;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {

  public static class CommonDescriptions {
    public static readonly string DefaultUtilizationDescription = $"The strength load utilisation is the ratio of the applied load to the load in the same direction that would cause the section to reach its capacity. Utilisation > 1 means the applied load exceeds the section capacity.{Environment.NewLine}If the applied load is outside the capacity range of the section, the utilisation will be greater than 1. Whereas, if the applied deformation exceeds the capacity, the load utilisation will be zero.";
    public static readonly string DefaultCrackDescription = $"Crack results are calculated at bar positions or section surfaces depending on the Design Code specifications.{Environment.NewLine}If the applied action is outside the capacity range of the section, the returned list will be empty. See MaximumCrack output for the crack result corresponding to the maximum crack width.";
    public static readonly string DefaultLoadOutputDescription = $"The section load under the applied action.{Environment.NewLine}If the applied deformation is outside the capacity range of the section, the returned load will be zero.";
    public static readonly string DefaultLoadInputDescription = "AdSec Load (Load or Deformation) for which the strength results are to be calculated.";
    public static readonly string DefaultDeformationDescription = "The section deformation under the applied action";
    public static readonly string DefaultSolutionInputDescription = "AdSec Results to perform strength check on";
  }

  public class SlsResultFunction : Function {

    public SectionSolutionParameter SolutionInput { get; set; } = new SectionSolutionParameter {
      Name = "Results",
      NickName = "Res",
      Description = CommonDescriptions.DefaultSolutionInputDescription,
      Access = Access.Item,
      Optional = false,
    };

    public GenericParameter LoadInput { get; set; } = new GenericParameter {
      Name = "Load",
      NickName = "Ld",
      Description = CommonDescriptions.DefaultLoadInputDescription,
      Access = Access.Item,
      Optional = false,
    };

    public LoadParameter LoadOutput { get; set; } = new LoadParameter {
      Name = "Load",
      NickName = "Ld",
      Description = CommonDescriptions.DefaultLoadOutputDescription,
      Access = Access.Item,
      Optional = false,
    };

    public CrackArrayParameter CrackOutput { get; set; } = new CrackArrayParameter {
      Name = "Cracks",
      NickName = "Crks",
      Description = CommonDescriptions.DefaultCrackDescription,
      Access = Access.List,
    };

    public CrackParameter MaximumCrackOutput { get; set; } = new CrackParameter {
      Name = "MaximumCrack",
      NickName = "MaxCrk",
      Description = CommonDescriptions.DefaultCrackDescription,
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
      Description = CommonDescriptions.DefaultDeformationDescription,
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

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Find Crack Load",
      NickName = "CrackLd",
      Description = "Increases the load until set crack width is reached",
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
       CrackOutput,
       MaximumCrackOutput,
       CrackUtilOutput,
       DeformationOutput,
       SecantStiffnessOutput,
       UncrackedMomentRangesOutput
      };
    }

    public void DeformationDescription(StrainUnit strainUnit, CurvatureUnit curvatureUnit) {
      var strainAbbreviation = Strain.GetAbbreviation(strainUnit);
      var curvatureAbbreviation = $"{strainAbbreviation}{Curvature.GetAbbreviation(curvatureUnit)}";
      DeformationOutput.Description = $"The section deformation under the applied action. The output is a vector representing:{Environment.NewLine}X: Strain [{strainAbbreviation}]{Environment.NewLine}Y: Curvature around zz (so in local y-direction) [{curvatureAbbreviation}]{Environment.NewLine}Z: Curvature around yy (so in local z-direction) [{curvatureAbbreviation}]";
    }

    public void SecantStiffnessDescription(AxialStiffnessUnit axialUnit, BendingStiffnessUnit bendingUnit) {
      SecantStiffnessOutput.Description = $"The secant stiffness under the applied action. The output is a vector representing:{Environment.NewLine}X: Axial stiffness [{AxialStiffness.GetAbbreviation(axialUnit)}],{Environment.NewLine}Y: The bending stiffness about the y-axis in the local coordinate system [{BendingStiffness.GetAbbreviation(bendingUnit)}],{Environment.NewLine}Z: The bending stiffness about the z-axis in the local coordinate system [{BendingStiffness.GetAbbreviation(bendingUnit)}]";
    }

    public void UncrackedMomentRangesDescription(MomentUnit momentUnit) {
      UncrackedMomentRangesOutput.Description = $"The range of moments (in the direction of the applied moment, assuming constant axial force) over which the section remains uncracked. Moment values are in [{Moment.GetAbbreviation(momentUnit)}]";
    }

    public override void Compute() {
      var momentUnit = ContextUnits.Instance.MomentUnit;
      // get solution input
      var solution = SolutionInput.Value;
      IServiceabilityResult sls = null;
      switch (LoadInput.Value) {
        case ILoad load:
          sls = solution.Serviceability.Check(load);
          break;
        case IDeformation deformation:
          sls = solution.Serviceability.Check(deformation);
          break;
        default:
          ErrorMessages.Add("Invalid Load Input");
          return;
      }

      LoadOutput.Value = sls.Load;
      DeformationOutput.Value = sls.Deformation;


      var cracks = new List<CrackLoad>();
      foreach (var crack in sls.Cracks) {
        cracks.Add(new CrackLoad() { Load = crack, Plane = solution.SectionDesign.LocalPlane });
      }
      CrackOutput.Value = cracks.ToArray();

      MaximumCrackOutput.Value = new CrackLoad() { Load = sls.MaximumWidthCrack, Plane = solution.SectionDesign.LocalPlane };
      CrackUtilOutput.Value = sls.CrackingUtilisation.As(RatioUnit.DecimalFraction);
      if (CrackUtilOutput.Value > 1) {
        if (CrackOutput.Value.Length == 0) {
          WarningMessages.Add("The section is failing and the cracks are so large we can't even compute them!");
        } else {
          RemarkMessages.Add("The section is cracked");
        }
      }
      DeformationOutput.Value = sls.Deformation;
      SecantStiffnessOutput.Value = sls.SecantStiffness;

      var momentRanges = new List<Tuple<double, double>>();
      foreach (var range in sls.UncrackedMomentRanges) {
        var interval = new Tuple<double, double>(range.Min.As(momentUnit), range.Max.As(momentUnit));
        momentRanges.Add(interval);
      }
      UncrackedMomentRangesOutput.Value = momentRanges.ToArray();
    }
  }

}
