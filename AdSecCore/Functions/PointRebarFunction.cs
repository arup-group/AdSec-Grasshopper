using AdSecGHCore.Constants;

using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Functions {

  public class PointRebarFunction : Function {

    public DoubleParameter Y { get; set; } = new DoubleParameter {
      Name = "Y",
      NickName = "Y",
      Description = "The local Y coordinate in yz-plane",
    };
    public DoubleParameter Z { get; set; } = new DoubleParameter {
      Name = "Z",
      NickName = "Z",
      Description = "The local Z coordinate in yz-plane",
    };

    public PointParameter Point { get; set; } = new PointParameter {
      Name = "Vertex Point",
      NickName = "Vx",
      Description = "A 2D vertex in the yz-plane for AdSec Profile and Reinforcement",
    };
    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Vertex Point",
      NickName = "Vertex Point",
      Description = "Create a 2D vertex in local yz-plane for AdSec Profile and Reinforcement",
    };
    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3(),
      Hidden = false,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Y,
        Z,
      };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Point,
      };
    }

    public override void Compute() {
      Point.Value = IPoint.Create(new Length(Y.Value, LengthUnit), new Length(Z.Value, LengthUnit));
    }
  }
}
