
using System;
using System.Collections.Generic;

using AdSecCore.Extensions;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits.Units;

namespace AdSecCore.Functions {


  public class SlsResultFunction : ResultFunction {

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Serviceability Result",
      NickName = "SLS",
      Description = "Performs serviceability analysis (SLS), for a given Load or Deformation.",
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

    private bool ComputeServiceabilityResult(SectionSolution solution, out IServiceabilityResult sls) {
      sls = null;
      switch (LoadInput.Value) {
        case ILoad load:
          if (!ILoadExtensions.IsValid(load, this)) {
            return false;
          }

          sls = solution.Serviceability.Check(load);
          break;
        case IDeformation deformation:
          if (!IDeformationExtensions.IsValid(deformation, this)) {
            return false;
          }
          sls = solution.Serviceability.Check(deformation);
          break;
        default:
          ErrorMessages.Add("Invalid Load Input");
          return false;
      }
      return true;
    }

    private void ProcessResults(IServiceabilityResult sls, OasysPlane localPlane) {
      LoadOutput.Value = sls.Load;
      DeformationOutput.Value = sls.Deformation;

      ProcessCracks(sls, localPlane);

      MaximumCrackOutput.Value = new CrackLoad {
        Load = sls.MaximumWidthCrack,
        Plane = localPlane
      };

      ProcessCrackUtilisation(sls);

      SecantStiffnessOutput.Value = sls.SecantStiffness;

      ProcessMomentRanges(sls);
    }


    private void ProcessCracks(IServiceabilityResult sls, OasysPlane localPlane) {
      var cracks = new List<CrackLoad>();
      foreach (var crack in sls.Cracks) {
        cracks.Add(new CrackLoad {
          Load = crack,
          Plane = localPlane
        });
      }
      CrackOutput.Value = cracks.ToArray();
    }

    private void ProcessCrackUtilisation(IServiceabilityResult sls) {
      var utilisation = sls.CrackingUtilisation.As(RatioUnit.DecimalFraction);
      CrackUtilOutput.Value = utilisation;

      if (utilisation > 1) {
        if (CrackOutput.Value.Length == 0) {
          WarningMessages.Add(
              "The section is failing and the cracks are so large we can't even compute them!");
        } else {
          RemarkMessages.Add("The section is cracked");
        }
      }
    }

    private void ProcessMomentRanges(IServiceabilityResult sls) {
      var momentRanges = new List<Tuple<double, double>>();
      var momentUnit = sls.Load.YY.Unit;

      foreach (var range in sls.UncrackedMomentRanges) {
        momentRanges.Add(new Tuple<double, double>(
            range.Min.As(momentUnit),
            range.Max.As(momentUnit)));
      }

      UncrackedMomentRangesOutput.Value = momentRanges.ToArray();
    }

    public override void Compute() {
      if (!ValidateInputs()) {
        return;
      }

      var solution = SolutionInput.Value;
      if (!ComputeServiceabilityResult(solution, out var sls)) {
        return;
      }

      ProcessResults(sls, solution.SectionDesign.LocalPlane);
    }
  }

}
