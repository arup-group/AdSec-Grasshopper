using AdSecCore.Parameters;

using AdSecGHCore.Constants;

using Oasys.Profiles;

using OasysUnits;
using OasysUnits.Units;

namespace AdSecCore.Functions {
  public class EditProfileFunction : Function, IDropdownOptions, ILocalUnits {
    public AngleUnit LocalAngleUnit { get; set; } = AngleUnit.Radian;

    public EditProfileFunction() {
      UpdateUnits();
      UpdateParameter();
    }

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute() {
      Name = "Edit Profile",
      NickName = "ProfileEdit",
      Description = "Modify an AdSec Profile",
    };
    public override Organisation Organisation { get; set; } = new Organisation() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat2()
    };
    public ProfileParameter Profile { get; set; } = Default.Profile(description: "AdSec Profile to Edit or get information from");
    public ProfileParameter ProfileOut { get; set; } = Default.Profile(description: "Modified AdSet Profile");
    public DoubleParameter Rotation { get; set; } = new DoubleParameter() {
      Name = "Rotation", // Need to include the unit
      NickName = "R",
      Description
        = "[Optional] The angle at which the profile is rotated. Positive rotation is anti-clockwise around the x-axis in the local coordinate system.",
      Access = Access.Item,
      Optional = true,
    };
    public BooleanParameter ReflectedY { get; set; } = new BooleanParameter() {
      Name = "isReflectedY",
      NickName = "rY",
      Description = "[Optional] Reflects the profile over the y-axis in the local coordinate system.",
      Access = Access.Item,
      Optional = true,
    };
    public BooleanParameter ReflectedZ { get; set; } = new BooleanParameter() {
      Name = "isReflectedZ",
      NickName = "rZ",
      Description = "[Optional] Reflects the profile over the z-axis in the local coordinate system.",
      Access = Access.Item,
      Optional = true,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Profile,
        Rotation,
        ReflectedY,
        ReflectedZ
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { Profile };
    }

    public IOptions[] Options() {
      return new IOptions[] {
        new UnitOptions() {
          Description = "Measure",
          UnitType = typeof(AngleUnit),
          UnitValue = (int)AngleUnit,
        }
      };
    }

    public override void Compute() {
      ProfileOut.Value = Profile.Value;
      ProfileOut.Value.Profile.Rotation = Angle.From(Rotation.Value, AngleUnit);
      if (ReflectedY.Value.HasValue) {
        ProfileOut.Value.Profile.IsReflectedY = ReflectedY.Value.Value;
      }

      if (ReflectedZ.Value.HasValue) {
        ProfileOut.Value.Profile.IsReflectedZ = ReflectedZ.Value.Value;
      }
    }

    protected sealed override void UpdateParameter() {
      Rotation.Name = UnitExtensions.NameWithUnits("Rotation", AngleUnit);
    }

    public void UpdateUnits() {
      AngleUnit = LocalAngleUnit;
    }
  }
}
