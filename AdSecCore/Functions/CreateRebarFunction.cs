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
      return new Attribute[] { MaterialParameter, DiameterParameter };
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
        Description = "Measure",
        UnitType = typeof(LengthUnit),
      }
    };
    public event Action OnVariableInputChanged;
  }
}
