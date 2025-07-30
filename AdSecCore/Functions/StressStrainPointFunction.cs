
using System.Collections.Generic;

using AdSecGHCore.Constants;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public class StressStrainPointFunction : Function, IDropdownOptions, ILocalUnits {

    public DoubleParameter StrainInput { get; set; } = new DoubleParameter {
      NickName = "ε",
      Name = "Strain",
      Description = "Value for strain (X-axis)",
      Optional = false,
    };

    public DoubleParameter StressInput { get; set; } = new DoubleParameter {
      NickName = "σ",
      Name = "Stress",
      Description = "Value for stress (Y-axis)",
      Optional = false,
    };

    public StressStrainPointParameter StressAndStrainOutput { get; set; } = new StressStrainPointParameter {
      NickName = "SPt",
      Name = "StressStrainPt",
      Description = "AdSec Stress Strain Point",
    };

    public StrainUnit LocalStrainUnit { get; set; } = StrainUnit.MilliStrain;
    public PressureUnit LocalStressUnit { get; set; } = PressureUnit.Megapascal;

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Stress-Strain Point",
      NickName = "SSPoint",
      Description = "Create a stress-strain point",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat1()
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        StrainInput,
        StressInput,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        StressAndStrainOutput,
      };
    }

    public override void Compute() {
      var strainValue = new Strain(StrainInput.Value, LocalStrainUnit);
      var stressValue = new Pressure(StressInput.Value, LocalStressUnit);
      StressAndStrainOutput.Value = IStressStrainPoint.Create(stressValue, strainValue);
    }

    public IOptions[] Options() {
      var options = new List<IOptions>();
      options.Add(new UnitOptions() {
        Description = "Strain Unit",
        UnitType = typeof(StrainUnit),
        UnitValue = (int)StrainUnitResult,
      });
      options.Add(new UnitOptions() {
        Description = "Stress Unit",
        UnitType = typeof(PressureUnit),
        UnitValue = (int)StressUnitResult,
      });
      return options.ToArray();
    }

    public void UpdateUnits() {
      StressUnitResult = LocalStressUnit;
      StrainUnitResult = LocalStrainUnit;
    }

    protected override void UpdateParameter() {
      string unitStressAbbreviation = Pressure.GetAbbreviation(StressUnitResult);
      string strainUnitAbbreviation = Strain.GetAbbreviation(StrainUnitResult);
      StrainInput.Name = $"Strain [{strainUnitAbbreviation}]";
      StressInput.Name = $"Stress [{unitStressAbbreviation}]";
    }
  }
}
