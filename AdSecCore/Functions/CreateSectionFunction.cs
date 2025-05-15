using System.Linq;

using AdSecCore.Builders;

using AdSecGHCore.Constants;

using Oasys.Profiles;

namespace AdSecCore.Functions {
  
  public class CreateSectionFunction : IFunction {
    public ProfileParameter Profile { get; set; } = new ProfileParameter {
      Name = "Profile",
      NickName = "Pf",
      Description = "AdSec Profile defining the Section solid boundary",
      Access = Access.Item,
    };
    public MaterialParameter Material { get; set; } = new MaterialParameter {
      Name = "Material",
      NickName = "Mat",
      Description = "AdSet Material for the section. The DesignCode of this material will be used for analysis",
      Access = Access.Item,
    };
    public RebarGroupParameter RebarGroup { get; set; } = new RebarGroupParameter {
      Name = "RebarGroup",
      NickName = "RbG",
      Description = "[Optional] AdSec Reinforcement Groups in the section (applicable for only concrete material).",
      Access = Access.List,
      Optional = true,
    };
    public SubComponentArrayParameter SubComponent { get; set; } = new SubComponentArrayParameter {
      Name = "SubComponent",
      NickName = "Sub",
      Description = "[Optional] AdSet Subcomponents contained within the section",
      Access = Access.List,
      Optional = true,
    };

    public SectionParameter Section { get; set; } = new SectionParameter {
      Name = "Section",
      NickName = "Sec",
      Description = "AdSec Section",
      Access = Access.Item,
    };

    public FuncAttribute Metadata { get; set; } = new FuncAttribute {
      Name = "Create Section",
      NickName = "Section",
      Description = "Create an AdSec Section",
    };
    public Organisation Organisation { get; set; } = new Organisation {
      Category = CategoryName.Name(),
      SubCategory = SubCategoryName.Cat4(),
    };

    public virtual Attribute[] GetAllInputAttributes() {
      return new Attribute[] { Profile, Material, RebarGroup, SubComponent, };
    }

    public virtual Attribute[] GetAllOutputAttributes() { return new Attribute[] { Section, }; }

    public void Compute() {
      var sectionBuilder = new SectionBuilder();
      sectionBuilder.WithProfile(Profile.Value.Profile);
      if (RebarGroup.Value != null) {
        var groups = RebarGroup.Value.Select(x => x.Group).ToList();
        sectionBuilder.WithReinforcementGroups(groups);

        sectionBuilder.WithCover(RebarGroup.Value.FirstOrDefault(x => x.Cover != null)?.Cover);
      }

      sectionBuilder.WithMaterial(Material.Value.Material);

      if (SubComponent.Value != null) {
        sectionBuilder.WithSubComponents(SubComponent.Value.Select(x => x.ISubComponent).ToList());
      }

      var section = sectionBuilder.Build();

      if (Profile.Value.Profile is IPerimeterProfile && RebarGroup.Value != null) {
        var reinforcements = RebarGroup.Value;
        var recalibrated = SectionBuilder.CalibrateReinforcementGroupsForSection(
          reinforcements.ToList(), Material.Value.DesignCode.IDesignCode, section);

        sectionBuilder.WithReinforcementGroups(recalibrated.Select(x => x.Group).ToList());
        section = sectionBuilder.Build();
      }

      Section.Value = new SectionDesign {
        Section = section,
        DesignCode = Material.Value.DesignCode,
        LocalPlane = Profile.Value.LocalPlane,
      };
    }
  }
}
