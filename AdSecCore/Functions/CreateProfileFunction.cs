using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  internal class CreateProfileFunction : Function {
    public CreateProfileFunction() {
      UpdateParameter();
    }

    public override FuncAttribute Metadata { get; set; } = new() {
      Description = "Create a Profile for an AdSec Section",
      Name = "Create Profile",
      NickName = "Profile",
    };
    public override Organisation Organisation { get; set; } = new() {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat2(),
    };

    public LengthParameter Width { get; set; } = new() {
      Name = "Width",
      NickName = "B",
      Description = "Profile width",
      Access = Access.Item,
    };

    public LengthParameter Depth { get; set; } = new() {
      Name = "Depth",
      NickName = "H",
      Description = "Profile depth",
      Access = Access.Item,
    };

    public PlaneParameter Plane { get; set; } = new() {
      Name = "LocalPlane",
      NickName = "P",
      Description = "[Optional] Plane representing local coordinate system, by default a YZ - plane is used",
      Access = Access.Item,
      Default = new[] { OasysPlane.PlaneYZ, },
      Optional = true,
    };

    public ProfileParameter ProfileOutput { get; set; } = new() {
      Name = "Profile",
      NickName = "Pf",
      Description = "Profile for AdSec Section",
      Access = Access.Item,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] { Width, Depth, Plane, };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] { ProfileOutput, };
    }

    protected sealed override void UpdateParameter() {
      base.UpdateParameter();
      Width.Name = UnitExtensions.NameWithUnits("Width", LengthUnitGeometry);
      Depth.Name = UnitExtensions.NameWithUnits("Depth", LengthUnitGeometry);
    }

    public override void Compute() {
      // No computation needed, just creating a profile
    }
  }
}
