
using AdSecCore.Functions;

namespace AdSecCore.Parameters {

  public static class Default {

    public static SectionParameter Section(
      string name = "Section", string nickName = "Sec", string description = "An AdSec Section",
      Access access = Access.Item, bool optional = false) {
      return new SectionParameter {
        Name = name,
        NickName = nickName,
        Description = description,
        Access = access,
        Optional = optional
      };
    }

    public static ProfileParameter Profile(
      string name = "Profile", string nickName = "Pf",
      string description = "Profile defining the Section solid boundary", Access access = Access.Item,
      bool optional = false) {
      return new ProfileParameter {
        Name = name,
        NickName = nickName,
        Description = description,
        Access = access,
        Optional = optional
      };
    }

    public static MaterialParameter Material(
      string name = "Material", string nickName = "Mat", string description = "Material for the section",
      Access access = Access.Item, bool optional = false) {
      return new MaterialParameter {
        Name = name,
        NickName = nickName,
        Description = description,
        Access = access,
        Optional = optional
      };
    }

    public static DesignCodeParameter DesignCode(
      string name = "DesignCode", string nickName = "Code", string description = "Section DesignCode",
      Access access = Access.Item, bool optional = false) {
      return new DesignCodeParameter {
        Name = name,
        NickName = nickName,
        Description = description,
        Access = access,
        Optional = optional
      };
    }

    public static RebarGroupArrayParameter RebarGroup(
      string name = "RebarGroup", string nickName = "RbG", string description = "Reinforcement Groups in the section",
      Access access = Access.List, bool optional = false) {
      return new RebarGroupArrayParameter {
        Name = name,
        NickName = nickName,
        Description = description,
        Access = access,
        Optional = optional
      };
    }

    public static SubComponentArrayParameter SubComponent(
      string name = "SubComponent", string nickName = "Sub",
      string description = "Subcomponents contained within the section", Access access = Access.List,
      bool optional = false) {
      return new SubComponentArrayParameter {
        Name = name,
        NickName = nickName,
        Description = description,
        Access = access,
        Optional = optional
      };
    }

    public static RebarBundleParameter RebarBundle(
      string name = "Rebar", string nickName = "Rb",
      string description = "AdSec Rebar (single or bundle) for AdSec Reinforcement", Access access = Access.Item,
      bool optional = false) {
      return new RebarBundleParameter() {
        Name = name,
        NickName = nickName,
        Description = description,
        Access = access,
        Optional = optional
      };
    }
  }
}
