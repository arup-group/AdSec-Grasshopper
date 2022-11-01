using System;
using Grasshopper.Kernel.Types;
using OasysGH.Parameters;
using Oasys.Profiles;
using OasysUnits;

namespace AdSecGH.Parameters
{
  public class AdSecProfileWebGoo : GH_Goo<IWeb>
  {
    public AdSecProfileWebGoo(IWeb web)
    : base(web)
    {
    }

    public override bool IsValid => true;

    public override string TypeName => "Web Profile";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_Goo Duplicate()
    {
      return new AdSecProfileWebGoo(this.Value);
    }
    public override string ToString()
    {
      string web = "AdSec Web {";
      if (this.Value.BottomThickness.Value == this.Value.TopThickness.Value)
      {
        Length thk = this.Value.BottomThickness.ToUnit(Units.LengthUnit);
        web += "Constant " + thk.ToString() + "}";
      }
      else
      {
        Length thk1 = this.Value.TopThickness.ToUnit(Units.LengthUnit);
        Length thk2 = this.Value.BottomThickness.ToUnit(Units.LengthUnit);
        web += "Tapered: Top:" + thk1.ToString() + ", Bottom:" + thk2.ToString() + "}";
      }
      return web;
    }
  }
}
