
using System.Collections.Generic;

using AdSecGHCore.Constants;

using Oasys.AdSec;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Functions {
  public class RebarStressStrainFunction : ConcreteStressStrainFunction {
    private ISection FlatSection { get; set; }
    public List<IPoint> Points { get; private set; } = new List<IPoint>();
    public List<Strain> StrainsULS { get; private set; } = new List<Strain>();
    public List<Pressure> StressesULS { get; private set; } = new List<Pressure>();
    public List<Strain> StrainsSLS { get; private set; } = new List<Strain>();
    public List<Pressure> StressesSLS { get; private set; } = new List<Pressure>();

    public PointArrayParameter PositionOutput { get; set; } = new PointArrayParameter {
      Name = "Position",
      NickName = "Vx",
      Description = "Rebar positions as 2D vertices in the section's local yz-plane",
      Access = Access.List,
    };

    public new StrainArrayParameter UlsStrainOutput { get; set; } = new StrainArrayParameter {
      Name = "ULS Strain",
      NickName = "εd",
      Description = "ULS strain for each rebar position",
      Access = Access.List,
    };

    public new PressureArrayParameter UlsStressOutput { get; set; } = new PressureArrayParameter {
      Name = "ULS Stress",
      NickName = "σd",
      Description = "ULS stress for each rebar position",
      Access = Access.List,
    };

    public new StrainArrayParameter SlsStrainOutput { get; set; } = new StrainArrayParameter {
      Name = "SLS Strain",
      NickName = "εk",
      Description = "SLS strain for each rebar position",
      Access = Access.List,
    };

    public new PressureArrayParameter SlsStressOutput { get; set; } = new PressureArrayParameter {
      Name = "SLS Stress",
      NickName = "σk",
      Description = "SLS stress for each rebar position",
      Access = Access.List,
    };

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Rebar Stress/Strain",
      NickName = "RSS",
      Description = "Calculate the Rebar Stress/Strains in the Section for a given Load or Deformation.",
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
        PositionOutput,
        UlsStrainOutput,
        UlsStressOutput,
        SlsStrainOutput,
        SlsStressOutput,
      };
    }

    protected override void UpdateParameter() {
      base.UpdateParameter();
      UlsStrainOutput.Name = UnitExtensions.NameWithUnits("ULS", StrainUnitResult);
      UlsStressOutput.Name = UnitExtensions.NameWithUnits("ULS", StressUnitResult);
      SlsStrainOutput.Name = UnitExtensions.NameWithUnits("SLS", StrainUnitResult);
      SlsStressOutput.Name = UnitExtensions.NameWithUnits("SLS", StressUnitResult);
      PositionOutput.Name = UnitExtensions.NameWithUnits("Position", LengthUnitResult);
    }

    public override void Compute() {

      if (!ValidateInputs()) {
        return;
      }

      ProcessInput();
      ProcessOutput();
    }

    private void ProcessOutput() {
      ClearResults();

      // Get flattened section
      FlatSection = SolutionInput.Value.SectionDesign.FlattenSection();
      foreach (var rebarGroup in FlatSection.ReinforcementGroups) {
        if (rebarGroup is ISingleBars singleBars) {
          foreach (var pos in singleBars.Positions) {
            Points.Add(pos);

            // Calculate ULS results
            var strainULS = Uls.Deformation.StrainAt(pos);
            StrainsULS.Add(strainULS);
            StressesULS.Add(singleBars.BarBundle.Material.Strength.StressAt(strainULS));

            // Calculate SLS results
            var strainSLS = Sls.Deformation.StrainAt(pos);
            StrainsSLS.Add(strainSLS);
            StressesSLS.Add(singleBars.BarBundle.Material.Serviceability.StressAt(strainSLS));
          }
        }
      }

      // Set output parameters
      PositionOutput.Value = Points.ToArray();
      UlsStrainOutput.Value = StrainsULS.ToArray();
      UlsStressOutput.Value = StressesULS.ToArray();
      SlsStrainOutput.Value = StrainsSLS.ToArray();
      SlsStressOutput.Value = StressesSLS.ToArray();
    }

    private void ClearResults() {
      Points.Clear();
      StrainsULS.Clear();
      StressesULS.Clear();
      StrainsSLS.Clear();
      StressesSLS.Clear();
    }
  }
}
