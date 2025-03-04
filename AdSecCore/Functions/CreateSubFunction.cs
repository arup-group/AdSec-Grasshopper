using System;

using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class CreateSubComponentFunction : IFunction {

    public SectionParameter Section { get; set; } = new SectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to create a subcomponent from",
      Access = Access.Item,
      Value = new SectionDesign(),
    };
    public PointParameter Offset { get; set; } = new PointParameter {
      Name = "Offset",
      NickName = "Off",
      Description
        = $"[Optional] Section offset (Vertex Point).{Environment.NewLine}Offset is applied between origins of containing section and sub-component. The offset of the profile is in the containing section's Profile Coordinate System. Any rotation applied to the containing section's profile will be applied to its sub-components. Sub-components can also have an additional rotation for their profiles.",
      Access = Access.Item,
      // Optional = true,
    };

    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "SubComponent",
      NickName = "SubComponent",
      Description = "Create an AdSec Subcomponent from a Section",
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };
    public virtual Attribute[] GetAllInputAttributes() { return new Attribute[] { Section, Offset, }; }

    public virtual Attribute[] GetAllOutputAttributes() { throw new NotImplementedException(); }

    public void Compute() { throw new NotImplementedException(); }
  }
}
