using AdSecCore.Builders;

using AdSecGHCore.Constants;

namespace AdSecCore.Functions {
  public class CreateSectionFunction : IFunction {

    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Section",
      NickName = "Section",
      Description = "Create an AdSec Section"
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };
    public ProfileParameter Profile { get; set; } = new ProfileParameter() {
      Name = "Profile",
      NickName = "Pf",
      Description = "AdSec Profile defining the Section solid boundary",
      Access = Access.Item
    };
    public MaterialParameter Material { get; set; } = new MaterialParameter() {
      Name = "Material",
      NickName = "Mat",
      Description = "AdSet Material for the section. The DesignCode of this material will be used for analysis",
      Access = Access.Item,
    };
    public RebarGroupParameter RebarGroup { get; set; } = new RebarGroupParameter() {
      Name = "RebarGroup",
      NickName = "RbG",
      Description = "[Optional] AdSec Reinforcement Groups in the section (applicable for only concrete material).",
      Access = Access.List,
      Optional = true,
    };
    public SubComponentParameter SubComponent { get; set; } = new SubComponentParameter() {
      Name = "SubComponent",
      NickName = "Sub",
      Description = "[Optional] AdSet Subcomponents contained within the section",
      Access = Access.List,
      Optional = true,
    };

    public virtual Attribute[] GetAllInputAttributes() {
      return new Attribute[] { Profile, Material, RebarGroup, SubComponent };
    }

    public SectionParameter Section { get; set; } = new SectionParameter() {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section",
      Access = Access.Item
    };
    public virtual Attribute[] GetAllOutputAttributes() { return new Attribute[] { Section }; }

    public void Compute() {
      var sectionBuilder = new SectionBuilder();
      sectionBuilder.SetProfile(Profile.Value.Profile);
      sectionBuilder.WithMaterial(Material.Value);
      Section.Value = new SectionDesign() {
        Section = sectionBuilder.Build(),
      };
    }
  }
}
