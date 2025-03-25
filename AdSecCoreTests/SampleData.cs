using AdSecCore.Builders;
using AdSecCore.Functions;

using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.StandardMaterials;

namespace AdSecGHTests {
  public static class SampleData {

    private static readonly ISteel _defaultSteelBeam = Steel.AS4100.Edition_1998.AS1163_C250;
    private static readonly IDesignCode _defaultDesignCode = IS456.Edition_2000;

    public static SectionDesign GetSectionDesign(IDesignCode? designCode = null, ISteel? iBeamMat = null) {
      iBeamMat ??= _defaultSteelBeam;

      designCode ??= _defaultDesignCode;

      var section = new SectionBuilder().WithProfile(ProfileBuilder.GetIBeam()).WithMaterial(iBeamMat).Build();
      var sectionDesign = new SectionDesign() {
        Section = section,
        DesignCode = designCode,
        MaterialName = "AS1163_C250",
        CodeName = "AS4100",
        LocalPlane = OasysPlane.PlaneYZ,
      };
      return sectionDesign;
    }
  }
}
