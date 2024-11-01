using Oasys.AdSec;
using Oasys.AdSec.DesignCode;
using Oasys.Profiles;

namespace AdSecCore {
  public static class SectionExtensions {

    public static ISection FlattenSection(this ISection section) {
      var profile = IPerimeterProfile.Create(section.Profile);
      return ISection.Create(profile, section.Material);
    }

    public static ISection FlattenSection(this ISection section, IDesignCode designCode) {
      var adSec = IAdSec.Create(designCode);
      return adSec.Flatten(section);
    }
  }
}
