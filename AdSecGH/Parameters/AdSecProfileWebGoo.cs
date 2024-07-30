using Grasshopper.Kernel.Types;
using Oasys.Profiles;
using OasysGH.Units;
using OasysUnits;

namespace AdSecGH.Parameters {
  public class AdSecProfileWebGoo : GH_Goo<IWeb> {
    public override bool IsValid => true;
    public override string TypeDescription => "AdSec " + TypeName + " Parameter";
    public override string TypeName => "Web Profile";

    public AdSecProfileWebGoo(IWeb web) : base(web) {
    }

    public override IGH_Goo Duplicate() {
      return new AdSecProfileWebGoo(Value);
    }

    public override string ToString() {
      string web = "AdSec Web {";
      if (Value.BottomThickness.Value == Value.TopThickness.Value) {
        Length thk = Value.BottomThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        web += "Constant " + thk.ToString() + "}";
      } else {
        Length thk1 = Value.TopThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        Length thk2 = Value.BottomThickness.ToUnit(DefaultUnits.LengthUnitGeometry);
        web += "Tapered: Top:" + thk1.ToString() + ", Bottom:" + thk2.ToString() + "}";
      }
      return web;
    }
  }
}
