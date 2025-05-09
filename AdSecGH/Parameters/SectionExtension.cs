﻿using AdSecCore.Builders;

using Oasys.AdSec.DesignCode;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  public static class SectionExtension {
    public static AdSecSectionGoo AdSecSectionGooSample() {
      var singleBars = new BuilderSingleBar().WithSize(2).AtPosition(Geometry.Zero()).Build();
      var Section = new SectionBuilder().WithWidth(40).CreateSquareSection().WithReinforcementGroup(singleBars).Build();

      var secSection = new AdSecSection(Section, IS456.Edition_2000, string.Empty, string.Empty, Plane.WorldXY);
      var adSecSectionGoo = new AdSecSectionGoo(secSection);
      return adSecSectionGoo;
    }
  }
}
