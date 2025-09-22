using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;

namespace AdSecGH.Parameters {
  public class AdSecRebarGroup {

    public AdSecRebarGroup() { }

    public AdSecRebarGroup(AdSecRebarGroup rebarGroup) {
      Group = rebarGroup.Group;
      if (Group != null) {
        Group = Group.Clone();
        var longitudinalGroup = rebarGroup.Group as ILongitudinalGroup;
        if (longitudinalGroup != null) {
          var preLoad = longitudinalGroup.Preload.Clone();
          var cloneLongitudinalGroup = Group as ILongitudinalGroup;
          cloneLongitudinalGroup.Preload = longitudinalGroup.Preload.Clone();
        }
      }

      if (rebarGroup.Cover != null) {
        Cover = ICover.Create(rebarGroup.Cover.UniformCover);
      }
    }

    public AdSecRebarGroup(IGroup group) {
      Group = group;
    }

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

    public AdSecRebarGroup Duplicate() {
      return (AdSecRebarGroup)MemberwiseClone();
    }

    public override string ToString() {
      return Group.ToString();
    }
  }
}
