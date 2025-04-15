using AdSecCore.Functions;

using Oasys.AdSec;

namespace AdSecCore {
  public static class SectionExtensions {

    public static ISection FlattenSection(this SectionDesign section) {
      var adSec = IAdSec.Create(section.DesignCode);
      return adSec.Flatten(section.Section);
    }
  }
}
