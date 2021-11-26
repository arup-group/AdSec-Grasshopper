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
using Oasys.Profiles;

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
                UnitsNet.Length thk = this.Value.BottomThickness.ToUnit(Units.LengthUnit);
                web += "Constant " + thk.ToString() + "}";
            }
            else
            {
                UnitsNet.Length thk1 = this.Value.TopThickness.ToUnit(Units.LengthUnit);
                UnitsNet.Length thk2 = this.Value.BottomThickness.ToUnit(Units.LengthUnit);
                web += "Tapered: Top:" + thk1.ToString() + ", Bottom:" + thk2.ToString() + "}";
            }
            return web;
        }
    }
}
