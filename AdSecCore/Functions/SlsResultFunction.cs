
using System;
using System.Collections.Generic;

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
