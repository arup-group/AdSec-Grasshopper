﻿using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;

namespace AdSecCore.Functions {
  public class ConcreteStressStrainFunction : ResultFunction {
    protected IServiceabilityResult Sls { get; set; }
    protected IStrengthResult Uls { get; set; }

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

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
       SolutionInput,
       LoadInput,
       VertexInput,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
       UlsStrainOutput,
       UlsStressOutput,
       SlsStrainOutput,
       SlsStressOutput,
      };
    }

    protected override void UpdateParameter() {
      base.UpdateParameter();
      UlsStrainOutput.Name = UnitExtensions.NameWithUnits("ULS Strain", StrainUnitResult);
      UlsStressOutput.Name = UnitExtensions.NameWithUnits("ULS Stress", StressUnitResult);
      SlsStrainOutput.Name = UnitExtensions.NameWithUnits("SLS Strain", StrainUnitResult);
      SlsStressOutput.Name = UnitExtensions.NameWithUnits("SLS Stress", StressUnitResult);
    }

    public override void Compute() {

      if (!ValidateInputs()) {
        return;
      }

      ProcessInput();

      ProcessOutput();
    }

    protected virtual void ProcessInput() {
      switch (LoadInput.Value) {
        case ILoad load:
          Uls = SolutionInput.Value.Solution.Strength.Check(load);
          Sls = SolutionInput.Value.Serviceability.Check(load);
          break;
        case IDeformation def:
          Uls = SolutionInput.Value.Solution.Strength.Check(def);
          Sls = SolutionInput.Value.Serviceability.Check(def);
          break;
      }
    }
    private void ProcessOutput() {
      // Calculate ULS results
      var strainULS = Uls.Deformation.StrainAt(VertexInput.Value);
      UlsStrainOutput.Value = strainULS;
      UlsStressOutput.Value = SolutionInput.Value.SectionDesign.Section.Material.Strength.StressAt(strainULS);

      // Calculate SLS results
      var strainSLS = Sls.Deformation.StrainAt(VertexInput.Value);
      SlsStrainOutput.Value = strainSLS;
      SlsStressOutput.Value = SolutionInput.Value.SectionDesign.Section.Material.Serviceability.StressAt(strainSLS);
    }
  }
}
