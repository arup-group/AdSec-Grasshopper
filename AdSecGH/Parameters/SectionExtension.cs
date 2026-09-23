using AdSecCore.Builders;
using AdSecCore.Functions;

using AdSecGH.Helpers;

using Oasys.AdSec.DesignCode;
using Oasys.Profiles;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public static class SectionExtension {
    public static AdSecSectionGoo AdSecSectionGooSample() {
      var singleBars = new BuilderSingleBar().WithSize(2).AtPosition(Geometry.Zero()).Build();
      var Section = new SectionBuilder().WithWidth(40).CreateSquareSection().WithReinforcementGroup(singleBars).Build();
      var sectionDesign = new SectionDesign {
        Section = Section,
        DesignCode = new DesignCode { IDesignCode = IS456.Edition_2000 },
      };
      var secSection = new AdSecSection(sectionDesign);
      var adSecSectionGoo = new AdSecSectionGoo(secSection);
      return adSecSectionGoo;
    }
  }
}
