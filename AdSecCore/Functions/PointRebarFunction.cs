using System;

using AdSecGHCore.Constants;

using Oasys.Profiles;

namespace AdSecCore.Functions {
  public class AnalyseFunction : IFunction {
    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Description = "Analyse an AdSec Section",
      Name = "Analyse Section",
      NickName = "Analyse",
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat6(),
    };
    public SectionParameter Section { get; set; } = new SectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to analyse",
    };

    public virtual Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Section,
      };
    }

    public Attribute[] GetAllOutputAttributes() { throw new NotImplementedException(); }

    public void Compute() { throw new NotImplementedException(); }
  }

  public class PointRebarFunction : IFunction {

    public LengthParameter Y { get; set; } = new LengthParameter {
      Name = "Y",
      NickName = "Y",
      Description = "The local Y coordinate in yz-plane",
    };
    public LengthParameter Z { get; set; } = new LengthParameter {
      Name = "Z",
      NickName = "Z",
      Description = "The local Z coordinate in yz-plane",
    };

    public PointParameter Point { get; set; } = new PointParameter {
      Name = "Vertex Point",
      NickName = "Vx",
      Description = "A 2D vertex in the yz-plane for AdSec Profile and Reinforcement",
    };
    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Vertex Point",
      NickName = "Vertex Point",
      Description = "Create a 2D vertex in local yz-plane for AdSec Profile and Reinforcement",
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat3(),
      Hidden = false,
    };

    public virtual Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Y,
        Z,
      };
    }

    public virtual Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Point,
      };
    }

    public void Compute() {
      Point.Value = IPoint.Create(Y.Value, Z.Value);
    }
  }
}
