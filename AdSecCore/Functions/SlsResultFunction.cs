
using System;
using System.Collections.Generic;

using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits.Units;

namespace AdSecCore.Functions {


  public class SlsResultFunction : ResultFunction {
    private IServiceabilityResult Sls { get; set; }
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

    private void ProcessInput() {
      switch (LoadInput.Value) {
        case ILoad load:
          Sls = SolutionInput.Value.Serviceability.Check(load);
          break;
        case IDeformation deformation:
          Sls = SolutionInput.Value.Serviceability.Check(deformation);
          break;
        default:
          break;
      }
    }

    private void ProcessOutput() {
      // load and deformation
      LoadOutput.Value = Sls.Load;
      DeformationOutput.Value = Sls.Deformation;

      var plane = SolutionInput.Value.SectionDesign.LocalPlane;

      // crack load
      var cracks = new List<CrackLoad>();
      foreach (var crack in Sls.Cracks) {
        cracks.Add(new CrackLoad {
          Load = crack,
          Plane = plane
        });
      }
      CrackOutput.Value = cracks.ToArray();

      //maximum crck width
      MaximumCrackOutput.Value = new CrackLoad {
        Load = Sls.MaximumWidthCrack,
        Plane = plane
      };

      //crack utilisation
      var utilisation = Sls.CrackingUtilisation.As(RatioUnit.DecimalFraction);
      CrackUtilOutput.Value = utilisation;
      if (utilisation > 1) {
        if (CrackOutput.Value.Length == 0) {
          WarningMessages.Add(
              "The section is failing and the cracks are so large we can't even compute them!");
        } else {
          RemarkMessages.Add("The section is cracked");
        }
      }

      //secant stiffness
      SecantStiffnessOutput.Value = Sls.SecantStiffness;

      //uncracked moment ranges
      var momentRanges = new List<Tuple<double, double>>();
      var momentUnit = Sls.Load.YY.Unit;
      foreach (var range in Sls.UncrackedMomentRanges) {
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

      ProcessInput();

      ProcessOutput();
    }
  }

}
