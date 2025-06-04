using System;

using AdSecCore.Parameters;

using AdSecGHCore.Constants;

using Oasys.AdSec.Reinforcement.Layers;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public class CreateRebarSpacingFunction : Function, IDropdownOptions, ILocalUnits, IVariableInput, IDynamicDropdown {
    public LengthUnit LocalLengthUnitGeometry { get; set; } = LengthUnit.Meter;
    public event Action OnVariableInputChanged;
    public event Action OnDropdownChanged;
    private FoldMode _mode = FoldMode.Distance;

    public CreateRebarSpacingFunction() {
      UpdateUnits();
      UpdateParameter();
    }

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Create Rebar Spacing",
      NickName = "Spacing",
      Description = "Create Rebar spacing (by Count or Pitch) for an AdSec Section",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3()
    };

    public RebarBundleParameter Rebar { get; set; } = Default.RebarBundle();

    public NullableDoubleParameter Spacing { get; set; } = new NullableDoubleParameter() {
      Name = "Spacing",
      NickName = "S",
      Description
        = "Number of bars is calculated based on the available length and the given bar pitch. The bar pitch is re-calculated to place the bars at equal spacing, with a maximum final pitch of the given value. Example: If the available length for the bars is 1000mm and the given bar pitch is 300mm, then the number of spacings that can fit in the available length is calculated as 1000 / 300 i.e. 3.333. The number of spacings is rounded up (3.333 rounds up to 4) and the bar pitch re-calculated (1000mm / 4), resulting in a final pitch of 250mm."
    };

    public IntegerParameter Count { get; set; } = new IntegerParameter() {
      Name = "Count",
      NickName = "N",
      Description
        = "The number of bundles or single bars. The bundles or single bars are spaced out evenly over the available space."
    };

    public RebarLayerParameter SpacedRebars { get; set; } = new RebarLayerParameter() {
      Name = "Spaced Rebars",
      NickName = "RbS",
      Description = "Rebars Spaced in a Layer for AdSec Reinforcement"
    };

    public override Attribute[] GetAllInputAttributes() {
      var attributes = new Attribute[] { Rebar, Spacing };
      if (_mode == FoldMode.Count) {
        attributes = new Attribute[] { Rebar, Count };
      }

      return attributes;
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { SpacedRebars };
    }

    public override void Compute() {
      switch (_mode) {
        case FoldMode.Distance:
          if (Spacing.Value.HasValue) {
            var length = Length.From(Spacing.Value.Value, LengthUnitGeometry);
            var layerByBarPitch = ILayerByBarPitch.Create(Rebar.Value, length);
            SpacedRebars.Value = new[] { layerByBarPitch as ILayer };
          }

          break;

        case FoldMode.Count:
          if (Count.Value > 0) {
            SpacedRebars.Value = new[] { ILayerByBarCount.Create(Count.Value, Rebar.Value) };
          }

          break;
        default: throw new ArgumentOutOfRangeException();
      }
    }

    public void UpdateUnits() {
      LengthUnitGeometry = LocalLengthUnitGeometry;
    }

    protected sealed override void UpdateParameter() {
      Spacing.Name = UnitExtensions.NameWithUnits("Spacing", LengthUnitGeometry);
    }

    public IOptions[] Options() {
      var enumOptions = new EnumOptions() {
        Description = "Spacing method",
        EnumType = typeof(FoldMode)
      };

      if (_mode == FoldMode.Count) {
        return new IOptions[] {
          enumOptions
        };
      }

      return new IOptions[] {
        enumOptions,
        new UnitOptions() {
          UnitType = typeof(LengthUnit),
          UnitValue = (int)LengthUnitGeometry,
        }
      };
    }

    public void SetMode(FoldMode mode) {
      _mode = mode;
      UpdateParameter();
      OnVariableInputChanged?.Invoke();
      OnDropdownChanged?.Invoke();
    }

    public enum FoldMode {
      Distance,
      Count,
    }

  }
}
