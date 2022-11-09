using System;
using System.Collections;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.IO;
using System.Linq;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using Rhino.DocObjects;
using Rhino.Collections;
using GH_IO;
using GH_IO.Serialization;
using Rhino.Display;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement.Layers;
using OasysUnits;

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
      Length dia = this.Value.BarBundle.Diameter.ToUnit(Units.LengthUnit);
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
          Length spacing = byBarPitch.Pitch.ToUnit(Units.LengthUnit);
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
