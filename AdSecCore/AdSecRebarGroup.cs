using System.Xml.Linq;

using AdSecCore.Functions;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
namespace AdSecGH.Parameters {
  public class AdSecRebarGroup {
    private readonly RebarGroup _rebarGroup;
    public AdSecRebarGroup() { }

    public AdSecRebarGroup(AdSecRebarGroup rebarGroup) {
      _rebarGroup = rebarGroup._rebarGroup;
      if (rebarGroup.Cover != null) {
        Cover = ICover.Create(rebarGroup.Cover.UniformCover);
      }
    }

    public AdSecRebarGroup(RebarGroup group) {
      _rebarGroup = group;
    }

    public AdSecRebarGroup(IGroup group, string codeDescription) {
      _rebarGroup = new RebarGroup() { Group = group, CodeDescription = codeDescription };
    }

    public ICover Cover { get; set; }

    public string CodeDescription {
      get {
        if (_rebarGroup == null) {
          return string.Empty;
        }
        return _rebarGroup.CodeDescription;
      }
    }

    public IGroup Group {
      get {
        if (_rebarGroup == null) {
          return null;
        }
        return _rebarGroup.Group;
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
