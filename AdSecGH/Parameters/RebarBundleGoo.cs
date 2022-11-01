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
using Oasys.AdSec.Reinforcement;
using OasysUnits;

namespace AdSecGH.Parameters
{
  public class AdSecRebarBundleGoo : GH_Goo<IBarBundle>
  {
    public AdSecRebarBundleGoo(IBarBundle bar)
    : base(bar)
    {
    }

    public override bool IsValid => true;

    public override string TypeName => "Rebar Bundle";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

    public override IGH_Goo Duplicate()
    {
      return new AdSecRebarBundleGoo(this.Value);
    }
    public override string ToString()
    {
      string bar = "Rebar {";
      Length thk1 = this.Value.Diameter.ToUnit(Units.LengthUnit);
      bar += "Ø" + thk1.ToString();
      if (this.Value.CountPerBundle > 1)
      {
        bar += ", Bundle (" + this.Value.CountPerBundle + ")";
      }
      //bar += ", " + this.Value.Material.ToString();
      return "AdSec " + bar + "}";
    }
  }
}
