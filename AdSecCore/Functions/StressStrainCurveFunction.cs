using System;
using System.Collections.Generic;

using AdSecGHCore.Constants;

using Oasys.AdSec.Materials.StressStrainCurves;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public enum StressStrainCurveType {
    Bilinear,
    Explicit,
    FibModelCode,
    Linear,
    ManderConfined,
    Mander,
    ParabolaRectangle,
    Park,
    Popovics,
    Rectangular,
  }
  public class StressStrainCurveFunction : Function, IVariableInput, IDropdownOptions, ILocalUnits, IDynamicDropdown {

    const string representingText = "AdSec Stress Strain Point representing the ";
    const string failurePointText = "Failure Point";
    const string fibModelCodeText = "FIB model code";
    const string manderModelText = "Mander model";
    const string manderConfinedModelText = "Mander Confined Model";
    const string yieldPointText = "Yield Point";
    private StressStrainCurveType selectedCurveType = StressStrainCurveType.Bilinear;
    public event Action OnVariableInputChanged;
    public event Action OnDropdownChanged;

    public StressStrainCurveType SelectedCurveType {
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
      Access = Access.List,
    };

    public StressStrainPointParameter PeakPoint { get; set; } = new StressStrainPointParameter {
      NickName = "SPt",
      Optional = false,
    };

    public DoubleParameter InitialModulus { get; set; } = new DoubleParameter {
      NickName = "Ei",
      Optional = false,
    };

    public DoubleParameter FailureStrain { get; set; } = new DoubleParameter {
      NickName = "εu",
      Optional = false,
    };

    public DoubleParameter UnconfinedStrength { get; set; } = new DoubleParameter {
      NickName = "σU",
      Optional = false,
    };

    public DoubleParameter ConfinedStrength { get; set; } = new DoubleParameter {
      NickName = "σC",
      Optional = false,
    };

    public StressStrainCurveParameter OutputCurve { get; set; } = new StressStrainCurveParameter {
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
        case StressStrainCurveType.Bilinear:
          YieldPoint.Name = yieldPointText;
          YieldPoint.Description = $"{representingText}{YieldPoint.Name}";
          FailurePoint.Name = failurePointText;
          FailurePoint.Description = $"{representingText}{failurePointText}";
          break;
        case StressStrainCurveType.Explicit:
          StressStrainPoints.Name = "StressStrainPoints";
          StressStrainPoints.Description = $"{representingText}StressStrainCurve as a Polyline";
          break;
        case StressStrainCurveType.FibModelCode:
          PeakPoint.Name = "Peak Point";
          PeakPoint.Description = $"{representingText}FIB model's Peak Point";
          InitialModulus.Name = $"Initial Modulus [{unitStressAbbreviation}]";
          InitialModulus.Description = $"Initial Modulus from {fibModelCodeText}";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = $"Failure strain from {fibModelCodeText}";
          break;
        case StressStrainCurveType.Linear:
          FailurePoint.Name = failurePointText;
          FailurePoint.Description = $"{representingText}{failurePointText}";
          break;
        case StressStrainCurveType.ManderConfined:
          UnconfinedStrength.Name = $"Unconfined Strength [{unitStressAbbreviation}]";
          UnconfinedStrength.Description = $"Unconfined strength for {manderConfinedModelText}";
          ConfinedStrength.Name = $"Confined Strength [{unitStressAbbreviation}]";
          ConfinedStrength.Description = $"Confined strength for {manderConfinedModelText}";
          InitialModulus.Name = $"Initial Modulus [{unitStressAbbreviation}]";
          InitialModulus.Description = $"Initial Modulus from {manderConfinedModelText}";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = $"Failure strain from {manderConfinedModelText}";
          break;
        case StressStrainCurveType.Mander:
          PeakPoint.Name = "Peak Point";
          PeakPoint.Description = $"{representingText}{manderModelText}'s Peak Point";
          InitialModulus.Name = $"Initial Modulus [{unitStressAbbreviation}]";
          InitialModulus.Description = $"Initial Modulus from {manderModelText}";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = $"Failure strain from {manderModelText}";
          break;
        case StressStrainCurveType.ParabolaRectangle:
        case StressStrainCurveType.Park:
        case StressStrainCurveType.Rectangular:
          YieldPoint.Name = yieldPointText;
          YieldPoint.Description = $"{representingText}{yieldPointText}";
          FailureStrain.Name = $"Failure Strain [{strainUnitAbbreviation}]";
          FailureStrain.Description = $"Failure strain";
          break;
        case StressStrainCurveType.Popovics:
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
        case StressStrainCurveType.Bilinear:
          attrs.Add(YieldPoint);
          attrs.Add(FailurePoint);
          break;
        case StressStrainCurveType.Explicit:
          attrs.Add(StressStrainPoints);
          break;
        case StressStrainCurveType.FibModelCode:
        case StressStrainCurveType.Mander:
          attrs.Add(PeakPoint);
          attrs.Add(InitialModulus);
          attrs.Add(FailureStrain);
          break;
        case StressStrainCurveType.Linear:
          attrs.Add(FailurePoint);
          break;
        case StressStrainCurveType.ManderConfined:
          attrs.Add(UnconfinedStrength);
          attrs.Add(ConfinedStrength);
          attrs.Add(InitialModulus);
          attrs.Add(FailureStrain);
          break;
        case StressStrainCurveType.ParabolaRectangle:
        case StressStrainCurveType.Rectangular:
          attrs.Add(YieldPoint);
          attrs.Add(FailureStrain);
          break;
        case StressStrainCurveType.Park:
          attrs.Add(YieldPoint);
          break;
        case StressStrainCurveType.Popovics:
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
      IStressStrainCurve curve = null;
      var failureStrain = Strain.From(FailureStrain.Value, StrainUnitResult);
      var initialModulus = Pressure.From(InitialModulus.Value, StressUnitResult);
      var unconfinedStrength = Pressure.From(UnconfinedStrength.Value, StressUnitResult);
      var confinedStrength = Pressure.From(ConfinedStrength.Value, StressUnitResult);
      switch (SelectedCurveType) {
        case StressStrainCurveType.Bilinear:
          curve = IBilinearStressStrainCurve.Create(YieldPoint.Value, FailurePoint.Value);
          break;
        case StressStrainCurveType.Explicit:
          var explicitCurve = IExplicitStressStrainCurve.Create();
          foreach (var point in StressStrainPoints.Value) {
            explicitCurve.Points.Add(point);
          }
          if (!ValidateFailureStrainOfExplicitCurve(explicitCurve)) {
            return;
          }
          curve = explicitCurve;
          break;
        case StressStrainCurveType.FibModelCode:
          curve = IFibModelCodeStressStrainCurve.Create(initialModulus, PeakPoint.Value, failureStrain);
          break;
        case StressStrainCurveType.Linear:
          curve = ILinearStressStrainCurve.Create(FailurePoint.Value);
          break;
        case StressStrainCurveType.ManderConfined:
          curve = IManderConfinedStressStrainCurve.Create(unconfinedStrength, confinedStrength, initialModulus, failureStrain);
          break;
        case StressStrainCurveType.Mander:
          curve = IManderStressStrainCurve.Create(initialModulus, PeakPoint.Value, failureStrain);
          break;
        case StressStrainCurveType.ParabolaRectangle:
          curve = IParabolaRectangleStressStrainCurve.Create(YieldPoint.Value, failureStrain);
          break;
        case StressStrainCurveType.Park:
          curve = IParkStressStrainCurve.Create(YieldPoint.Value);
          break;
        case StressStrainCurveType.Popovics:
          curve = IPopovicsStressStrainCurve.Create(PeakPoint.Value, failureStrain);
          break;
        case StressStrainCurveType.Rectangular:
          curve = IRectangularStressStrainCurve.Create(YieldPoint.Value, failureStrain);
          break;
      }

      OutputCurve.Value = new StressStrainCurve() { IStressStrainCurve = curve, IsCompression = true };
    }

    private static bool ValidateFailureStrainOfExplicitCurve(IExplicitStressStrainCurve explicitCurve) {
      //explict curve throw exception only when It is accessed
      return Math.Abs(explicitCurve.FailureStrain.Value) > 0;
    }

    public IOptions[] Options() {
      UpdateUnits();
      var options = new List<IOptions> {
        new EnumOptions() {
          EnumType = typeof(StressStrainCurveType),
          Description = "Curve Type",
          Selected= SelectedCurveType,
        }
      };
      switch (SelectedCurveType) {
        case StressStrainCurveType.FibModelCode:
        case StressStrainCurveType.ManderConfined:
        case StressStrainCurveType.Mander:
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
        case StressStrainCurveType.ParabolaRectangle:
        case StressStrainCurveType.Popovics:
        case StressStrainCurveType.Rectangular:
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

    public static string GetCurveTypeFromInterface(IStressStrainCurve curve) {
      switch (curve) {
        case IBilinearStressStrainCurve _: return "Bilinear";
        case IExplicitStressStrainCurve _: return "Explicit";
        case IFibModelCodeStressStrainCurve _: return "FibModelCode";
        case ILinearStressStrainCurve _: return "Linear";
        case IManderConfinedStressStrainCurve _: return "ManderConfined";
        case IManderStressStrainCurve _: return "Mander";
        case IParabolaRectangleStressStrainCurve _: return "ParabolaRectangle";
        case IParkStressStrainCurve _: return "Park";
        case IPopovicsStressStrainCurve _: return "Popovics";
        case IRectangularStressStrainCurve _: return "Rectangular";
        default: return "DefaultCurve";
      }
    }

  }
}
