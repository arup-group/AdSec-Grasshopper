using System.Collections.Generic;

using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

namespace AdSecGHTests {
  public static class SampleData {

    private static ISteel _defaultSteelBeam = Steel.AS4100.Edition_1998.AS1163_C250;
    public static IConcrete _defaultConcrete = Concrete.IS456.Edition_2000.M10;
    private static IDesignCode _defaulyDesignCode = IS456.Edition_2000;

    public static SectionDesign GetSectionDesign(IDesignCode designCode = null, ISteel iBeamMat = null) {
      if (iBeamMat == null) {
        iBeamMat = _defaultSteelBeam;
      }

      if (designCode == null) {
        designCode = _defaulyDesignCode;
      }

      var section = new SectionBuilder().WithProfile(ProfileBuilder.GetIBeam()).WithMaterial(iBeamMat).Build();
      var sectionDesign = new SectionDesign() {
        Section = section,
        DesignCode = designCode,
        MaterialName = "AS1163_C250",
        CodeName = "AS4100",
        LocalPlane = OasysPlane.PlaneYZ
      };
      return sectionDesign;
    }

    public static SectionDesign GetCompositeSectionDesign(IDesignCode designCode = null) {
      if (designCode == null) {
        designCode = _defaulyDesignCode;
      }

      var iProfile = ProfileBuilder.GetIBeam();
      var profile = new ProfileBuilder().WithWidth(100).WidthDepth(100).Build();
      var subSection = new SectionBuilder().WithProfile(iProfile).WithMaterial(_defaultSteelBeam).Build();

      var section = new SectionBuilder().WithProfile(profile).WithMaterial(_defaultConcrete).WithSubComponents(
        new List<ISubComponent> {
          ISubComponent.Create(subSection, Geometry.Zero())
        })
       .WithReinforcementGroup(new BuilderLineGroup().Build()).Build();
      var sectionDesign = new SectionDesign() {
        Section = section,
        DesignCode = designCode,
        MaterialName = "AS1163_C250",
        CodeName = "IS456",
        LocalPlane = OasysPlane.PlaneYZ
      };
      return sectionDesign;
    }
  }
}
