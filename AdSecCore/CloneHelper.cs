using System.Collections.Generic;

using AdSecCore.Builders;
using AdSecCore.Functions;
using AdSecCore.Helpers;

using AdSecGHCore;

using Oasys.AdSec.IO.Serialization;
using Oasys.AdSec.Reinforcement.Groups;

namespace AdSecGH.Helpers {
  public static class CloneHelper {

    static public IGroup Clone(this IGroup group) {
      var loads = new Dictionary<int, List<object>>();


      if (!(group is ITemplateGroup templateGroup)) {
        throw new System.InvalidCastException("Group is not a longitudinal group");
      }
      var reinforcementMaterial = templateGroup.Layers[0].BarBundle.Material;
      var designCode = MaterialHelper.FindDesignCode(reinforcementMaterial);
      var concreteMaterials = MaterialHelper.FindConcreteMaterial(reinforcementMaterial);
      var sectionBuilder = new SectionBuilder();
      var section = sectionBuilder.WithHeight(10).WithHeight(10).WithReinforcementGroup(group).WithMaterial(concreteMaterials[0]).Build();


      var sectionDesign = new SectionDesign {
        Section = section,
        DesignCode = new DesignCode() { IDesignCode = designCode },
      };

      var adSecSection = new List<SectionDesign>();
      adSecSection.Add(sectionDesign);

      var jsoString = FileHelper.ModelJson(adSecSection, loads);
      var parser = JsonParser.Deserialize(jsoString);
      return parser.Sections[0].ReinforcementGroups[0];
    }

  }
}
