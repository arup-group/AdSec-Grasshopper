using Grasshopper.Kernel.Types;
using Oasys.Profiles;
using OasysUnits;
using OasysGH.Units;

namespace AdSecGH.Parameters
{
  public class AdSecProfileWebGoo : GH_Goo<IWeb>
  {
    public override bool IsValid => true;
    public override string TypeName => "Web Profile";
    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public AdSecProfileWebGoo(IWeb web) : base(web)
    {
    }

    #region methods
    public override IGH_Goo Duplicate()
    {
      return new AdSecProfileWebGoo(this.Value);
    }

    public override string ToString()
    {
      string web = "AdSec Web {";
      if (this.Value.BottomThickness.Value == this.Value.TopThickness.Value)
      {
        Length thk = this.Value.BottomThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        web += "Constant " + thk.ToString() + "}";
      }
      else
      {
        Length thk1 = this.Value.TopThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        Length thk2 = this.Value.BottomThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        web += "Tapered: Top:" + thk1.ToString() + ", Bottom:" + thk2.ToString() + "}";
      }
      return web;
    }
    #endregion
  }
}
