using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;

namespace AdSecGH.Parameters
{
  public class AdSecRebarGroup
  {
    #region properties
    public IGroup Group { get; set; }
    public ICover Cover { get; set; }
    public bool IsValid
    {
      get
      {
        if (this.Group == null)
          return false;
        return true;
      }
    }
    #endregion

    #region constructors
    public AdSecRebarGroup()
    {
    }

    public AdSecRebarGroup(IGroup group)
    {
      this.Group = group;
    }
    #endregion

    #region methods
    public AdSecRebarGroup Duplicate()
    {
      if (this == null)
        return null;
      AdSecRebarGroup dup = (AdSecRebarGroup)this.MemberwiseClone();
      return dup;
    }

    public override string ToString()
    {
      return Group.ToString();
    }
    #endregion
  }
}
