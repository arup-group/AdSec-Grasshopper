using System;

using AdSecGHCore.Constants;

using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public enum PreLoadType {
    Force,
    Strain,
    Stress,
  }
  public class CreatePreLoadFunction : Function, IDropdownOptions, ILocalUnits {
    public object LocalUnit { get; set; } = ForceUnit.Newton;
    private PreLoadType _preLoadType = PreLoadType.Force;
    public PreLoadType PreLoadType {
      get { return _preLoadType; }
      set {
        if (_preLoadType == value) {
          return;
        }
        _preLoadType = value;
        UpdateParameter();
      }
    }

    public void UpdateUnits() {
      switch (PreLoadType) {
        case PreLoadType.Force:
          ForceUnit = (ForceUnit)LocalUnit;
          break;
        case PreLoadType.Strain:
          MaterialStrainUnit = (StrainUnit)LocalUnit;
          break;
        case PreLoadType.Stress:
          StressUnitResult = (PressureUnit)LocalUnit;
          break;
      }
    }

    public RebarGroupParameter RebarGroupInput { get; set; } = new RebarGroupParameter {
      Name = "RebarGroup",
      NickName = "RbG",
      Description = "AdSec Reinforcement Group to apply Preload to",
      Access = Access.Item,
    };

    public GenericParameter PreloadInput { get; set; } = new GenericParameter {
      Name = "Force",
      NickName = "P",
      Description = "The preload value (Force, Strain, or Stress) to apply to the reinforcement group",
      Access = Access.Item,
    };

    public RebarGroupParameter PreloadedRebarGroupOutput { get; set; } = new RebarGroupParameter {
      Name = "Prestressed RebarGroup",
      NickName = "RbG",
      Description = "Preloaded Rebar Group for AdSec Section",
      Access = Access.Item,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        RebarGroupInput,
        PreloadInput,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        PreloadedRebarGroupOutput,
      };
    }

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Prestress Load",
      NickName = "Prestress",
      Description = "Create an AdSec Prestress Load for Reinforcement Layout as either Preforce, Prestrain, or Prestress",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat5(),
    };

    protected override void UpdateParameter() {
      string forceUnitAbbreviation = Force.GetAbbreviation(ForceUnit);
      string strainUnitAbbreviation = Strain.GetAbbreviation(MaterialStrainUnit);
      string stressUnitAbbreviation = Pressure.GetAbbreviation(StressUnitResult);

      switch (PreLoadType) {
        case PreLoadType.Force:
          PreloadInput.Name = $"{PreLoadType} [{forceUnitAbbreviation}]";
          PreloadInput.NickName = "P";
          break;
        case PreLoadType.Strain:
          PreloadInput.Name = $"{PreLoadType} [{strainUnitAbbreviation}]";
          PreloadInput.NickName = "ε";
          break;
        case PreLoadType.Stress:
          PreloadInput.Name = $"{PreLoadType} [{stressUnitAbbreviation}]";
          PreloadInput.NickName = "σ";
          break;
      }
    }

    private IPreload ParsePreLoad() {
      try {
        if (PreLoadType == PreLoadType.Force) {
          return IPreForce.Create(UnitHelpers.ParseToQuantity<Force>(PreloadInput.Value, ForceUnit));
        } else if (PreLoadType == PreLoadType.Strain) {
          return IPreStrain.Create(UnitHelpers.ParseToQuantity<Strain>(PreloadInput.Value, MaterialStrainUnit));
        }
        return IPreStress.Create(UnitHelpers.ParseToQuantity<Pressure>(PreloadInput.Value, StressUnitResult));
      } catch (InvalidCastException ex) {
        ErrorMessages.Add(ex.Message);
        return null;
      }
    }


    public override void Compute() {
      // Get the rebar groupZz
      var rebarGroup = RebarGroupInput.Value;
      var outRebarGroup = rebarGroup;
      if (outRebarGroup?.Group == null) {
        ErrorMessages.Add("Invalid RebarGroup input.");
        return;
      }

      //parse preload
      var preload = ParsePreLoad();
      if (preload == null) {
        return;
      }

      // Apply the preload to the rebar group
      var longitudinalGroup = outRebarGroup.Group as ILongitudinalGroup;
      if (longitudinalGroup == null) {
        ErrorMessages.Add("RebarGroup must be a longitudinal group.");
        return;
      }

      longitudinalGroup.Preload = preload;

      // Set the output
      PreloadedRebarGroupOutput.Value = outRebarGroup;
    }

    private int UnitValue() {
      if (PreLoadType == PreLoadType.Force) {
        return (int)ForceUnit;
      } else if (PreLoadType == PreLoadType.Strain) {
        return (int)MaterialStrainUnit;
      }
      return (int)StressUnitResult;
    }

    private Type TypeOfUnit() {
      if (PreLoadType == PreLoadType.Force) {
        return typeof(ForceUnit);
      } else if (PreLoadType == PreLoadType.Strain) {
        return typeof(StrainUnit);
      }
      return typeof(PressureUnit);
    }

    public IOptions[] Options() {
      return new IOptions[] {
        new EnumOptions() {
        EnumType = typeof(PreLoadType),
        Selected= PreLoadType,
        Description = "Force",
      },
      new UnitOptions() {
        Description = "Measure",
        UnitType = TypeOfUnit(),
        UnitValue = UnitValue(),
      }
      };
    }
  }
}
