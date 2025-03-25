using AdSecCore.Functions;

using Oasys.AdSec;

namespace AdSecCore {
  public static class SectionExtensions {

    public static ISection FlattenSection(this SectionDesign section) {
      var adSec = IAdSec.Create(section.DesignCode.IDesignCode);
      return adSec.Flatten(section.Section);
    }
  }
}
