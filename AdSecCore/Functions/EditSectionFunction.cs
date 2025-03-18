using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class EditSectionFunction : Function {

    public override FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "EditSection",
      NickName = "EditSect",
      Description = "Edit an AdSec Section",
    };
    public override Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };

    public SectionParameter Section { get; set; } = new SectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section to edit or get information from",
      Access = Access.Item,
    };

    public ProfileParameter Profile { get; set; } = new ProfileParameter {
      Name = "Profile",
      NickName = "Pf",
      Description = "[Optional] Edit the Profile defining the Section solid boundary",
      Access = Access.Item,
      Optional = true,
    };
    public MaterialParameter Material { get; set; } = new MaterialParameter {
      Name = "Material",
      NickName = "Mat",
      Description = "[Optional] Edit the Material for the section",
      Access = Access.Item,
      Optional = true,
    };
    public DesignCodeParameter DesignCode { get; set; } = new DesignCodeParameter {
      Name = "DesignCode",
      NickName = "Code",
      Description = "[Optional] Edit the Section DesignCode",
      Access = Access.Item,
      Optional = true,
    };
    public RebarGroupParameter RebarGroup { get; set; } = new RebarGroupParameter {
      Name = "RebarGroup",
      NickName = "RbG",
      Description = "[Optional] Edit the Reinforcement Groups in the section (applicable for only concrete material).",
      Access = Access.List,
      Optional = true,
    };
    public SubComponentArrayParameter SubComponent { get; set; } = new SubComponentArrayParameter {
      Name = "SubComponent",
      NickName = "Sub",
      Description = "[Optional] Edit the Subcomponents contained within the section",
      Access = Access.List,
      Optional = true,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] { Section, Profile, Material, DesignCode, RebarGroup, SubComponent, };
    }

    public override void Compute() { }
  }
}
