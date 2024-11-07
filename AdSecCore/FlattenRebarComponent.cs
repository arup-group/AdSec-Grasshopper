using System;

using AdSecCore.Parameters;

using AdSecGHCore.Constants;

using Oasys.Business;

using Attribute = Oasys.Business.Attribute;

namespace AdSecGH.Components {
  public class FlattenRebarComponent : IBusinessComponent {

    public ISectionParameter Section { get; set; } = new ISectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to get single rebars from",
      Access = Access.Item,
    };

    public IPointArrayParameter Position { get; set; } = new IPointArrayParameter {
      Name = "Position",
      NickName = "Vx",
      Description = "Rebar position as 2D vertex in the section's local yz-plane",
      Access = Access.List,
    };

    public DoubleArrayParameter Diameter { get; set; } = new DoubleArrayParameter {
      Name = "Diameter",
      NickName = "Ø",
      Description = "Bar Diameter",
      Access = Access.List,
    };

    public IntegerArrayParameter BundleCount { get; set; } = new IntegerArrayParameter {
      Name = "Bundle Count",
      NickName = "N",
      Description = "Count per bundle (1, 2, 3 or 4)",
      Access = Access.List,
    };

    public DoubleArrayParameter PreLoad { get; set; } = new DoubleArrayParameter {
      Name = "PreLoad",
      NickName = "P",
      Description = "The pre-load per reinforcement bar. Positive value is tension.",
      Access = Access.List,
    };

    public StringArrayParam Material { get; set; } = new StringArrayParam {
      Name = "Material",
      NickName = "Mat",
      Description = "Material Type",
    };
    public ComponentAttribute Metadata { get; set; } = new ComponentAttribute {
      Name = "FlattenRebar",
      NickName = "FRb",
      Description = "Flatten all rebars in a section into single bars.",
    };
    public ComponentOrganisation Organisation { get; set; } = new ComponentOrganisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };

    public virtual Attribute[] GetAllInputAttributes() {
      return new Attribute[] {
        Section,
      };
    }

    public virtual Attribute[] GetAllOutputAttributes() {
      return new Attribute[] {
        Position,
        Diameter,
        BundleCount,
        PreLoad,
        Material,
      };
    }

    public void UpdateInputValues(params object[] values) { throw new NotImplementedException(); }

    public void Compute() { throw new NotImplementedException(); }
  }
}
