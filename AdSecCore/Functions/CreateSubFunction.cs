using System;

using AdSecGHCore.Constants;

using Oasys.AdSec;
using Oasys.Profiles;

using OasysUnits;

namespace AdSecCore.Functions {
  public class CreateSubComponentFunction : Function {

    public SectionParameter Section { get; set; } = new SectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to create a subcomponent from",
      Access = Access.Item,
      Value = new SectionDesign(),
    };
    public SubComponentParameter SubComponent { get; set; } = new SubComponentParameter {
      Name = "SubComponent",
      NickName = "Sub",
      Description = "AdSec Sub Component",
      Access = Access.Item,
    };
    public PointParameter Offset { get; set; } = new PointParameter {
      Name = "Offset",
      NickName = "Off",
      Description
        = $"[Optional] Section offset (Vertex Point).{Environment.NewLine}Offset is applied between origins of containing section and sub-component. The offset of the profile is in the containing section's Profile Coordinate System. Any rotation applied to the containing section's profile will be applied to its sub-components. Sub-components can also have an additional rotation for their profiles.",
      Access = Access.Item,
      Optional = true,
    };

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "SubComponent",
      NickName = "SubComponent",
      Description = "Create an AdSec Subcomponent from a Section",
    };
    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };
    public override Attribute[] GetAllInputAttributes() { return new Attribute[] { Section, Offset, }; }

    public override Attribute[] GetAllOutputAttributes() { return new Attribute[] { SubComponent, }; }

    public override void Compute() {
      if (Offset.Value == null) {
        Offset.Value = IPoint.Create(Length.Zero, Length.Zero);
      }

      SubComponent.Value = new SubComponent() {
        ISubComponent = ISubComponent.Create(Section.Value.Section, Offset.Value),
        SectionDesign = Section.Value,
      };
    }
  }
}
