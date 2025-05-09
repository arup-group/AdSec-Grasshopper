﻿using System.Linq;

using AdSecCore.Builders;
using AdSecCore.Parameters;

using AdSecGH.Parameters;

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

    public SectionParameter Section { get; set; }
      = Default.Section(description: "AdSec Section to edit or get information from");

    public SectionParameter SectionOut { get; set; } = Default.Section(description: "Edited AdSec Section");

    public ProfileParameter Profile { get; set; }
      = Default.Profile(description: "[Optional] Edit the Profile defining the Section solid boundary", optional: true);
    public ProfileParameter ProfileOut { get; set; } = Default.Profile();
    public MaterialParameter Material { get; set; }
      = Default.Material(description: "[Optional] Edit the Material for the section", optional: true);
    public MaterialParameter MaterialOut { get; set; } = Default.Material();
    public DesignCodeParameter DesignCode { get; set; }
      = Default.DesignCode(description: "[Optional] Edit the Section DesignCode", optional: true);
    public DesignCodeParameter DesignCodeOut { get; set; } = Default.DesignCode();
    public RebarGroupParameter RebarGroup { get; set; } = Default.RebarGroup(
      description: "[Optional] Edit the Reinforcement Groups in the section (applicable for only concrete material).",
      optional: true);
    public RebarGroupParameter RebarGroupOut { get; set; } = Default.RebarGroup(
      description: "Reinforcement Groups in the section (applicable for only concrete material).");
    public SubComponentArrayParameter SubComponent { get; set; } = Default.SubComponent(
      description: "[Optional] Edit the Subcomponents contained within the section", optional: true);
    public SubComponentArrayParameter SubComponentOut { get; set; } = Default.SubComponent(optional: true);

    public GeometryParameter Geometry { get; set; } = new GeometryParameter {
      Name = "SectionCurves",
      NickName = "CAD",
      Description = "All curves used for displaying the section - useful for making CAD drawings",
      Access = Access.List,
    };

    public override Attribute[] GetAllInputAttributes() {
      return new Attribute[] { Section, Profile, Material, DesignCode, RebarGroup, SubComponent, };
    }

    public override Attribute[] GetAllOutputAttributes() {
      return new Attribute[]
        { SectionOut, ProfileOut, MaterialOut, DesignCodeOut, RebarGroupOut, SubComponentOut, Geometry, };
    }

    public override void Compute() {
      ProfileOut.Value = Profile.Value ?? ProfileDesign.From(Section.Value);

      MaterialOut.Value = Material.Value ?? MaterialDesign.From(Section.Value);

      DesignCodeOut.Value = DesignCode.Value ?? DesignCodeParameter.From(Section.Value);

      MaterialOut.Value.DesignCode = DesignCodeOut.Value; // The material needs the new DesignCode

      RebarGroupOut.Value = RebarGroup.Value ?? Section.Value.Section.ReinforcementGroups
       .Select(x => new AdSecRebarGroup(x)).ToArray();
      foreach (var t in RebarGroupOut.Value) {
        t.Cover = Section.Value.Section.Cover;
      }

      SubComponentOut.Value = SubComponent.Value ?? SubComponent.From(Section.Value);

      var section = new SectionBuilder().WithSubComponents(SubComponentOut.Value.Select(x => x.ISubComponent).ToList())
       .WithMaterial(MaterialOut.Value.Material).WithCover(Section.Value.Section.Cover)
       .WithReinforcementGroups(RebarGroupOut.Value.Select(x => x.Group).ToList()).WithProfile(ProfileOut.Value.Profile)
       .Build();
      SectionOut.Value = new SectionDesign {
        Section = section,
        DesignCode = DesignCodeOut.Value,
        LocalPlane = Section.Value.LocalPlane,
        CodeName = DesignCodeOut.Value.DesignCodeName,
        MaterialName = MaterialOut.Value.GradeName,
      };

      Geometry.Value = SectionOut.Value;
    }
  }
}
