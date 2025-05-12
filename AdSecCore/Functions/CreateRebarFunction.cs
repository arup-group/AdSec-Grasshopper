using System;

using AdSecCore.Parameters;

using AdSecGHCore.Constants;

using OasysUnits.Units;

namespace AdSecCore.Functions {
  public class CreateRebarFunction : Function, IDropdownOptions, IVariableInput {

    public enum RebarMode {
      Single,
      Bundle,
    }

    public MaterialParameter MaterialParameter { get; set; } = Default.Material();
    public DoubleParameter DiameterParameter { get; set; } = new DoubleParameter() {
      Name = "Diameter",
      NickName = "Ø",
      Description = "Bar Diameter",
    };

    public IntegerParameter CountParameter { get; set; } = new IntegerParameter() {
      Name = "Count",
      NickName = "N",
      Description = "Count per bundle (1, 2, 3 or 4)",
    };

    public RebarBundleParameter RebarBundleParameter { get; set; } = Default.RebarBundle();

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Create Rebar",
      NickName = "Rebar",
      Description = "Create Rebar (single or bundle) for an AdSec Section",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      SubCategory = SubCategoryName.Cat3(),
      Category = CategoryName.Name()
    };

    public override Attribute[] GetAllInputAttributes() {
      Attribute[] attributes = { };
      switch (Mode) {
        case RebarMode.Single:
          attributes = new Attribute[] { MaterialParameter, DiameterParameter };
          break;
        case RebarMode.Bundle:
          attributes = new Attribute[] { MaterialParameter, DiameterParameter, CountParameter };
          break;
      }

      return attributes;
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { RebarBundleParameter };
    }

    public override void Compute() { }

    public void SetMode(RebarMode template) {
      if (Mode == template) {
        return;
      }

      Mode = template;
      OnVariableInputChanged?.Invoke();
    }

    public RebarMode Mode { get; set; } = RebarMode.Single;

    public IOptions[] Options { get; set; } = {
      new EnumOptions() {
        Description = "Rebar Type",
        EnumType = typeof(RebarMode),
      },
      new UnitOptions() {
        UnitType = typeof(LengthUnit),
      }
    };
    public event Action OnVariableInputChanged;

    protected override void UpdateParameter() {
      DiameterParameter.Name = UnitExtensions.NameWithUnits("Diameter", LengthUnitGeometry);

    }
  }
}
