﻿using Oasys.AdSec;
using Oasys.AdSec.DesignCode;

namespace AdSecCore {
  public static class SectionExtensions {

    // public static ISection FlattenSection(this ISection section) {
    //   var profile = IPerimeterProfile.Create(section.Profile);
    //   return ISection.Create(profile, section.Material);
    // }

    public static ISection FlattenSection(this ISection section) {
      var adSec = IAdSec.Create(IS456.Edition_2000);
      return adSec.Flatten(section);
    }
  }
}