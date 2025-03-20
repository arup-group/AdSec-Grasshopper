using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

namespace AdSecGHTests {
  public static class SampleData {

    private static ISteel _defaultSteelBeam = Steel.AS4100.Edition_1998.AS1163_C250;
    private static IDesignCode _defaulyDesignCode = IS456.Edition_2000;

    public static SectionDesign GetSectionDesign(IDesignCode designCode = null, ISteel iBeamMat = null) {
      if (iBeamMat == null) {
        iBeamMat = _defaultSteelBeam;
      }

      if (designCode == null) {
        designCode = _defaulyDesignCode;
      }

      var section = new SectionBuilder().SetProfile(ProfileBuilder.GetIBeam()).WithMaterial(iBeamMat).Build();
      var sectionDesign = new SectionDesign() {
        Section = section,
        DesignCode = designCode,
        MaterialName = "AS1163_C250",
        CodeName = "AS4100",
        LocalPlane = OasysPlane.PlaneYZ
      };
      return sectionDesign;
    }
  }
}
