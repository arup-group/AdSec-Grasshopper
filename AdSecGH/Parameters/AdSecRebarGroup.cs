using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;

namespace AdSecGH.Parameters {
  public class AdSecRebarGroup {
    public ICover Cover { get; set; }
    public IGroup Group { get; set; }
    public bool IsValid {
      get {
        if (Group == null) {
          return false;
        }
        return true;
      }
    }

    public AdSecRebarGroup() {
    }

    public AdSecRebarGroup(IGroup group) {
      Group = group;
    }

    public AdSecRebarGroup Duplicate() {
      if (this == null) {
        return null;
      }
      var dup = (AdSecRebarGroup)MemberwiseClone();
      return dup;
    }

    public override string ToString() {
      return Group.ToString();
    }
  }
}
