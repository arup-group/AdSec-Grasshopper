using System;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Reinforcement.Layers;

namespace AdSecGH.Parameters
{
  public class AdSecRebarLayerGoo : GH_Goo<ILayer>
  {
    public AdSecRebarLayerGoo(ILayer layer)
    : base(layer)
    {
    }

    public override bool IsValid => true;

    public override string TypeName => "Rebar Spacing";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_Goo Duplicate()
    {
      return new AdSecRebarLayerGoo(this.Value);
    }
    public override string ToString()
    {
      string bar = "";
      UnitsNet.Length dia = this.Value.BarBundle.Diameter.ToUnit(Units.LengthUnit);
      bar += "Ø" + dia.ToString();
      if (this.Value.BarBundle.CountPerBundle > 1)
      {
        bar += ", Bundle (" + this.Value.BarBundle.CountPerBundle + ")";
      }

      string str = "";
      try
      {
        ILayerByBarCount byBarCount = (ILayerByBarCount)Value;
        str = byBarCount.Count.ToString() + "No. " + bar;
      }
      catch (Exception)
      {
        try
        {
          ILayerByBarPitch byBarPitch = (ILayerByBarPitch)Value;
          UnitsNet.Length spacing = byBarPitch.Pitch.ToUnit(Units.LengthUnit);
          str = bar + " bars / " + spacing.ToString();
        }
        catch (Exception)
        {

        }
      }

      return "AdSec " + TypeName + " {" + str + "}";
    }
  }
}
