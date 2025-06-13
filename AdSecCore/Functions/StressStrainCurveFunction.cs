using System;
using System.Collections.Generic;

using AdSecGHCore.Constants;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public class StressStrainCurveFunction : Function, IVariableInput, IDropdownOptions, ILocalUnits, IDynamicDropdown {
    public enum CurveType {
      Bilinear,
      Explicit,
      FibModelCode,
      Linear,
      ManderConfined,
      Mander,
      ParabolaRectangle,
      Park,
      Popovics,
      Rectangular
    }
    const string representingText = "AdSec Stress Strain Point representing the ";
    const string failurePointText = "Failure Point";
    const string fibModelCodeText = "FIB model code";
    const string manderModelText = "Mander model";
    const string manderConfinedModelText = "Mander Confined Model";
    const string yieldPointText = "Yield Point";
    private CurveType selectedCurveType = CurveType.Bilinear;
    public event Action OnVariableInputChanged;
    public event Action OnDropdownChanged;

    public CurveType SelectedCurveType {
      get { return selectedCurveType; }
      set {
        if (selectedCurveType == value) {
          return;
        }
        selectedCurveType = value;
        UpdateParameter();
        OnVariableInputChanged?.Invoke();
        OnDropdownChanged?.Invoke();
      }
    }

    public StrainUnit LocalStrainUnit { get; set; } = StrainUnit.MicroStrain;
    public PressureUnit LocalStressUnit { get; set; } = PressureUnit.Megapascal;

    public StressStrainPointParameter YieldPoint { get; set; } = new StressStrainPointParameter {
      NickName = "SPy",
      Optional = false,
    };

    public StressStrainPointParameter FailurePoint { get; set; } = new StressStrainPointParameter {
      NickName = "SPu",
      Optional = false,
    };

    public StressStrainPointArrayParameter StressStrainPoints { get; set; } = new StressStrainPointArrayParameter {
      NickName = "SPs",
      Optional = false,
    };

    public StressStrainPointParameter PeakPoint { get; set; } = new StressStrainPointParameter {
      NickName = "SPt",
      Optional = false,
    };

    public PressureParameter InitialModulus { get; set; } = new PressureParameter {
      NickName = "Ei",
      Optional = false,
    };

    public StrainParameter FailureStrain { get; set; } = new StrainParameter {
      NickName = "εu",
      Optional = false,
    };

    public PressureParameter UnconfinedStrength { get; set; } = new PressureParameter {
      NickName = "σU",
      Optional = false,
    };

    public PressureParameter ConfinedStrength { get; set; } = new PressureParameter {
      NickName = "σC",
      Optional = false,
    };

    public GenericParameter OutputCurve { get; set; } = new GenericParameter {
      Name = "StressStrainCrv",
      NickName = "SCv",
      Description = "AdSec Stress Strain Curve",
      Optional = false,
    };
    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create StressStrainCrv",
      NickName = "StressStrainCrv",
      Description = "Create a Stress Strain Curve for AdSec Material"
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat1()
    };

    protected override void UpdateParameter() {
      string unitStressAbbreviation = Pressure.GetAbbreviation(StressUnitResult);
      string strainUnitAbbreviation = Strain.GetAbbreviation(StrainUnitResult);

      switch (SelectedCurveType) {
        case CurveType.Bilinear:
          YieldPoint.Name = yieldPointText;
          YieldPoint.Description = $"{representingText}{YieldPoint.Name}";
          FailurePoint.Name = failurePointText;
          FailurePoint.Description = $"{representingText}{failurePointText}";
          break;
        case CurveType.Explicit:
          StressStrainPoints.Name = "StressStrainPoints";
          StressStrainPoints.Description = $"{representingText}StressStrainCurve as a Polyline";
          break;
        case CurveType.FibModelCode:
          PeakPoint.Name = "Peak Point";
          PeakPoint.Description = $"{representingText}FIB model's Peak Point";
          InitialModulus.Name = $"Initial Modulus [{unitStressAbbreviation}]";
          InitialModulus.Description = $"Initial Modulus from {fibModelCodeText}";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = $"Failure strain from {fibModelCodeText}";
          break;
        case CurveType.Linear:
          FailurePoint.Name = failurePointText;
          FailurePoint.Description = $"{representingText}{failurePointText}";
          break;
        case CurveType.ManderConfined:
          UnconfinedStrength.Name = $"Unconfined Strength [{unitStressAbbreviation}]";
          UnconfinedStrength.Description = $"Unconfined strength for {manderConfinedModelText}";
          ConfinedStrength.Name = $"Confined Strength [{unitStressAbbreviation}]";
          ConfinedStrength.Description = $"Confined strength for {manderConfinedModelText}";
          InitialModulus.Name = $"Initial Modulus [{unitStressAbbreviation}]";
          InitialModulus.Description = $"Initial Modulus from {manderConfinedModelText}";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = $"Failure strain from {manderConfinedModelText}";
          break;
        case CurveType.Mander:
          PeakPoint.Name = "Peak Point";
          PeakPoint.Description = $"{representingText}{manderModelText}'s Peak Point";
          InitialModulus.Name = $"Initial Modulus [{unitStressAbbreviation}]";
          InitialModulus.Description = $"Initial Modulus from {manderModelText}";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = $"Failure strain from {manderModelText}";
          break;
        case CurveType.ParabolaRectangle:
        case CurveType.Park:
        case CurveType.Rectangular:
          YieldPoint.Name = yieldPointText;
          YieldPoint.Description = $"{representingText}{yieldPointText}";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = $"Failure strain";
          break;
        case CurveType.Popovics:
          PeakPoint.Name = "Peak Point";
          PeakPoint.Description = $"{representingText}Peak Point";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = "Failure strain from Popovic model";
          break;
      }
    }

    public override Attribute[] GetAllInputAttributes() {
      var attrs = new List<Attribute>();
      switch (SelectedCurveType) {
        case CurveType.Bilinear:
          attrs.Add(YieldPoint);
          attrs.Add(FailurePoint);
          break;
        case CurveType.Explicit:
          attrs.Add(StressStrainPoints);
          break;
        case CurveType.FibModelCode:
        case CurveType.Mander:
          attrs.Add(PeakPoint);
          attrs.Add(InitialModulus);
          attrs.Add(FailureStrain);
          break;
        case CurveType.Linear:
          attrs.Add(FailurePoint);
          break;
        case CurveType.ManderConfined:
          attrs.Add(UnconfinedStrength);
          attrs.Add(ConfinedStrength);
          attrs.Add(InitialModulus);
          attrs.Add(FailureStrain);
          break;
        case CurveType.ParabolaRectangle:
        case CurveType.Rectangular:
          attrs.Add(YieldPoint);
          attrs.Add(FailureStrain);
          break;
        case CurveType.Park:
          attrs.Add(YieldPoint);
          break;
        case CurveType.Popovics:
          attrs.Add(PeakPoint);
          attrs.Add(FailureStrain);
          break;
      }
      return attrs.ToArray();
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
       OutputCurve
      };
    }

    public override void Compute() {

      try {
        switch (SelectedCurveType) {
          case CurveType.Bilinear:
            OutputCurve.Value = IBilinearStressStrainCurve.Create(YieldPoint.Value, FailurePoint.Value);
            break;
          case CurveType.Explicit:
            var explicitCurve = IExplicitStressStrainCurve.Create();
            foreach (var point in StressStrainPoints.Value) {
              explicitCurve.Points.Add(point);
            }
            OutputCurve.Value = explicitCurve;
            break;
          case CurveType.FibModelCode:
            OutputCurve.Value = IFibModelCodeStressStrainCurve.Create(InitialModulus.Value, PeakPoint.Value, FailureStrain.Value);
            break;
          case CurveType.Linear:
            OutputCurve.Value = ILinearStressStrainCurve.Create(FailurePoint.Value);
            break;
          case CurveType.ManderConfined:
            OutputCurve.Value = IManderConfinedStressStrainCurve.Create(UnconfinedStrength.Value, ConfinedStrength.Value, InitialModulus.Value, FailureStrain.Value);
            break;
          case CurveType.Mander:
            OutputCurve.Value = IManderStressStrainCurve.Create(InitialModulus.Value, PeakPoint.Value, FailureStrain.Value);
            break;
          case CurveType.ParabolaRectangle:
            OutputCurve.Value = IParabolaRectangleStressStrainCurve.Create(YieldPoint.Value, FailureStrain.Value);
            break;
          case CurveType.Park:
            OutputCurve.Value = IParkStressStrainCurve.Create(YieldPoint.Value);
            break;
          case CurveType.Popovics:
            OutputCurve.Value = IPopovicsStressStrainCurve.Create(PeakPoint.Value, FailureStrain.Value);
            break;
          case CurveType.Rectangular:
            OutputCurve.Value = IRectangularStressStrainCurve.Create(YieldPoint.Value, FailureStrain.Value);
            break;
        }
      } catch (ArgumentNullException) {
        ErrorMessages.Add("Input value can not be null");
      }

    }

    public IOptions[] Options() {
      var options = new List<IOptions> {
        new EnumOptions() {
          EnumType = typeof(CurveType),
          Description = "Curve Type",
        }
      };
      switch (SelectedCurveType) {
        case CurveType.FibModelCode:
        case CurveType.ManderConfined:
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

          break;
        case CurveType.ParabolaRectangle:
        case CurveType.Popovics:
        case CurveType.Rectangular:
          options.Add(new UnitOptions() {
            Description = "Strain Unit",
            UnitType = typeof(StrainUnit),
            UnitValue = (int)StrainUnitResult,
          });
          break;
      }
      return options.ToArray();
    }

    public void UpdateUnits() {
      StressUnitResult = LocalStressUnit;
      StrainUnitResult = LocalStrainUnit;
    }
  }
}
