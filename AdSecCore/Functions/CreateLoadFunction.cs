using AdSecGHCore.Constants;

using Oasys.AdSec;

using OasysUnits;

namespace AdSecCore.Functions {
  public class CreateLoadFunction : Function {
    public CreateLoadFunction() {
      UpdateParameter();
    }

    public ForceParameter ForceInput { get; set; } = new ForceParameter {
      Name = "Fx",
      NickName = "X",
      Description = "The axial force. Positive x is tension.",
      Access = Access.Item,
      Optional = true,
    };

    public MomentParameter MomentYInput { get; set; } = new MomentParameter {
      Name = "Myy",
      NickName = "YY",
      Description = "The moment about local y-axis. Positive yy is anti - clockwise moment about local y-axis.",
      Access = Access.Item,
      Optional = true,
    };

    public MomentParameter MomentZInput { get; set; } = new MomentParameter {
      Name = "Mzz",
      NickName = "ZZ",
      Description = "The moment about local z-axis. Positive zz is anti - clockwise moment about local z-axis.",
      Access = Access.Item,
      Optional = true,
    };

    public LoadParameter LoadOutput { get; set; } = new LoadParameter {
      Name = "Load",
      NickName = "Ld",
      Description = "AdSec Load",
      Access = Access.Item,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        ForceInput,
        MomentYInput,
        MomentZInput,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        LoadOutput,
      };
    }

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Load",
      NickName = "Load",
      Description = "Create an AdSec Load from an axial force and biaxial moments",
    };

    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat5(),
    };

    protected override void UpdateParameter() {
      base.UpdateParameter();
      ForceInput.Name = UnitExtensions.NameWithUnits("Fx", ForceUnit);
      MomentYInput.Name = UnitExtensions.NameWithUnits("Myy", MomentUnit);
      MomentZInput.Name = UnitExtensions.NameWithUnits("Mzz", MomentUnit);
    }

    public override void Compute() {
      var force = Force.From(ForceInput.Value.Value, ForceUnit);

      var momentY = Moment.From(MomentYInput.Value.Value, MomentUnit);

      var momentZ = Moment.From(MomentZInput.Value.Value, MomentUnit);

      var load = ILoad.Create(force, momentY, momentZ);

      LoadOutput.Value = load;
    }
  }
}
