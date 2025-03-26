using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;

namespace AdSecCore.Functions {
  public class ConcreteStressStrainFunction : Function {
    public SectionSolutionParameter SolutionInput { get; set; } = new SectionSolutionParameter {
      Name = "Results",
      NickName = "Res",
      Description = "AdSec Results to perform serviceability check on",
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

    public PointParameter VertexInput { get; set; } = new PointParameter {
      Name = "Vertex Point",
      NickName = "Vx",
      Description = "A 2D vertex in the section's local yz-plane for where to calculate strain.",
      Access = Access.Item,
      Optional = false,
    };

    public StrainParameter UlsStrainOutput { get; set; } = new StrainParameter {
      Name = "ULS Strain",
      NickName = "εd",
      Description = "ULS strain at Vertex Point",
      Access = Access.Item,
    };

    public PressureParameter UlsStressOutput { get; set; } = new PressureParameter {
      Name = "ULS Stress",
      NickName = "σd",
      Description = "ULS stress at Vertex Point",
      Access = Access.Item,
    };

    public StrainParameter SlsStrainOutput { get; set; } = new StrainParameter {
      Name = "SLS Strain",
      NickName = "εk",
      Description = "SLS strain at Vertex Point",
      Access = Access.Item,
    };

    public PressureParameter SlsStressOutput { get; set; } = new PressureParameter {
      Name = "SLS Stress",
      NickName = "σk",
      Description = "SLS stress at Vertex Point",
      Access = Access.Item,
    };

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Concrete Stress/Strain",
      NickName = "CSS",
      Description = "Calculate the Concrete Stress/Strain at a point on the Section for a given Load or Deformation.",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat7(),
    };

    public void UpdateOutputDescription() {
      string strainUnitAbbreviation = "";// Strain.GetAbbreviation(DefaultUnits.StrainUnitResult);
      string stressUnitAbbreviation = "";//Pressure.GetAbbreviation(DefaultUnits.StressUnitResult);
      UlsStrainOutput.Description = $"ULS strain at Vertex Point [{strainUnitAbbreviation}]";
      UlsStressOutput.Description = $"ULS stress at Vertex Point [{stressUnitAbbreviation}]";
      SlsStrainOutput.Description = $"SLS strain at Vertex Point [{strainUnitAbbreviation}]";
      SlsStressOutput.Description = $"SLS stress at Vertex Point [{stressUnitAbbreviation}]";
    }

    public override void Compute() {
      if (!ValidateInputs()) {
        return;
      }

      var solution = SolutionInput.Value;
      var point = VertexInput.Value;

      var (uls, sls) = CalculateResults(solution.Solution);
      if (uls == null || sls == null) {
        return;
      }

      // Calculate ULS results
      var strainULS = uls.Deformation.StrainAt(point);
      UlsStrainOutput.Value = strainULS;
      UlsStressOutput.Value = solution.SectionDesign.Section.Material.Strength.StressAt(strainULS);

      // Calculate SLS results
      var strainSLS = sls.Deformation.StrainAt(point);
      SlsStrainOutput.Value = strainSLS;
      SlsStressOutput.Value = solution.SectionDesign.Section.Material.Serviceability.StressAt(strainSLS);
    }

    private bool ValidateInputs() {
      if (SolutionInput?.Value == null) {
        ErrorMessages.Add("Solution input is null");
        return false;
      }
      if (LoadInput?.Value == null) {
        ErrorMessages.Add("Load input is null");
        return false;
      }
      if (VertexInput?.Value == null) {
        ErrorMessages.Add("Vertex point is null");
        return false;
      }
      return true;
    }

    private (IStrengthResult uls, IServiceabilityResult sls) CalculateResults(ISolution solution) {
      switch (LoadInput.Value) {
        case ILoad load:
          return (
            solution.Strength.Check(load),
            solution.Serviceability.Check(load)
          );
        case IDeformation def:
          return (
            solution.Strength.Check(def),
            solution.Serviceability.Check(def)
          );
        default:
          ErrorMessages.Add("Invalid Load Input");
          return (null, null);
      }
    }
  }
}
