using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;

namespace AdSecGHCore {
  public static class SampleData {

    private static readonly ISteel _defaultSteelBeam = Steel.AS4100.Edition_1998.AS1163_C250;
    public static IConcrete _defaultConcrete = Concrete.AS3600.Edition_2001.MPA20;
    private static readonly IDesignCode _defaultDesignCode = IS456.Edition_2000;

    public static SectionDesign GetSectionDesign(IDesignCode? designCode = null, ISteel? iBeamMat = null) {
      iBeamMat ??= _defaultSteelBeam;

      designCode ??= _defaultDesignCode;

      var section = new SectionBuilder().WithProfile(ProfileBuilder.GetIBeam()).WithMaterial(iBeamMat).Build();
      var sectionDesign = new SectionDesign {
        Section = section,
        DesignCode = new DesignCode { IDesignCode = designCode, },
        MaterialName = "AS1163_C250",
        CodeName = "AS4100",
        LocalPlane = OasysPlane.PlaneYZ,
      };
      return sectionDesign;
    }

    public static SectionDesign GetCompositeSectionDesign(IDesignCode designCode = null) {
      if (designCode == null) {
        designCode = _defaultDesignCode;
      }

      var profile = new ProfileBuilder().WithWidth(100).WidthDepth(100).Build();
      var section = new SectionBuilder().WithProfile(profile).WithMaterial(_defaultConcrete).WithSubComponents(
        new List<ISubComponent> {
          GetSubComponentZero().ISubComponent,
        }).WithReinforcementGroup(new BuilderLineGroup().Build()).Build();
      var sectionDesign = new SectionDesign {
        Section = section,
        DesignCode = new DesignCode { IDesignCode = designCode, },
        MaterialName = "AS1163_C250",
        CodeName = "IS456",
        LocalPlane = OasysPlane.PlaneYZ,
      };
      return sectionDesign;
    }

    public static SubComponent GetSubComponentZero() {
      return GetSubComponent(Geometry.Zero());
    }

    public static SubComponent GetSubComponent(IPoint offset) {
      var iProfile = ProfileBuilder.GetIBeam();
      var subSection = new SectionBuilder().WithProfile(iProfile).WithMaterial(_defaultSteelBeam).Build();
      return new SubComponent {
        ISubComponent = ISubComponent.Create(subSection, offset),
        SectionDesign = new SectionDesign {
          Section = subSection,
        },
      };
    }
  }
}
