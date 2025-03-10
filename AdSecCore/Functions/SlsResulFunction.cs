
using System;
using System.Collections.Generic;
using System.Linq;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public class SlsResultFunction : Function {

    public SectionSolutionParameter SolutionInput { get; set; } = new SectionSolutionParameter {
      Name = "Results",
      NickName = "Res",
      Description = "AdSec Results to perform serviceability check",
      Access = Access.Item,
      Optional = false,
    };

    public LoadGenericParameter LoadInput { get; set; } = new LoadGenericParameter {
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
      NickName = "Crk",
      Description = $"Crack results are calculated at bar positions or section surfaces depending on the Design Code specifications.{Environment.NewLine}If the applied action is outside the capacity range of the section, the returned list will be empty. See MaximumCrack output for the crack result corresponding to the maximum crack width.",
      Access = Access.Item,
    };

    public CrackParameter MaximumCrackOutput { get; set; } = new CrackParameter {
      Name = "MaximumCrack",
      NickName = "Crk",
      Description = $"Crack results are calculated at bar positions or section surfaces depending on the Design Code specifications.{Environment.NewLine}If the applied action is outside the capacity range of the section, the returned list will be empty. See MaximumCrack output for the crack result corresponding to the maximum crack width.",
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
      Access = Access.Item,
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

    private static string StrainUnitAbbreviation(StrainUnit unit) {
      return Strain.GetAbbreviation(unit);
    }

    private static string CurvatureUnitAbbreviation(CurvatureUnit unit) {
      var curvature = new Curvature(0, unit);
      return string.Concat(curvature.ToString().Where(char.IsLetter));
    }

    private static string AxialUnitAbbreviation(AxialStiffnessUnit unit) {
      var axial = new AxialStiffness(0, unit);
      return string.Concat(axial.ToString().Where(char.IsLetter));

    }

    private static string BendingUnitAbbreviation(BendingStiffnessUnit unit) {
      var bending = new BendingStiffness(0, unit);
      return string.Concat(bending.ToString().Where(char.IsLetter));
    }

    public void RefreshDeformation(StrainUnit strainUnit, CurvatureUnit curvatureUnit) {
      DeformationOutput.Description = $"The section deformation under the applied action. The output is a vector representing:{Environment.NewLine}X: Strain [{StrainUnitAbbreviation(strainUnit)}]{Environment.NewLine}Y: Curvature around zz (so in local y-direction) [{CurvatureUnitAbbreviation(curvatureUnit)}]{Environment.NewLine}Z: Curvature around yy (so in local z-direction) [{CurvatureUnitAbbreviation(curvatureUnit)}]";
    }

    public void RefreshSecantStiffness(AxialStiffnessUnit axialUnit, BendingStiffnessUnit bendingUnit) {
      SecantStiffnessOutput.Description = $"The secant stiffness under the applied action. The output is a vector representing:{Environment.NewLine}X: Axial stiffness [{AxialUnitAbbreviation(axialUnit)}],{Environment.NewLine}Y: The bending stiffness about the y-axis in the local coordinate system [{BendingUnitAbbreviation(bendingUnit)}],{Environment.NewLine}Z: The bending stiffness about the z-axis in the local coordinate system [{BendingUnitAbbreviation(bendingUnit)}]";
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
          throw new ArgumentException("Invalid Load Input");
      }

      LoadOutput.Value = sls.Load;
      DeformationOutput.Value = sls.Deformation;


      var cracks = new List<ICrack>();
      foreach (var crack in sls.Cracks) {
        cracks.Add(crack);
      }
      CrackOutput.Value = cracks.ToArray();

      MaximumCrackOutput.Value = sls.MaximumWidthCrack;
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
      foreach (var mrng in sls.UncrackedMomentRanges) {
        var interval = new Tuple<double, double>(mrng.Min.As(momentUnit), mrng.Max.As(momentUnit));
        momentRanges.Add(interval);
      }
    }
  }

}
