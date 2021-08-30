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

namespace GhAdSec.Parameters
{
    public class AdSecProfileFlangeGoo : GH_Goo<IFlange>
    {
        public AdSecProfileFlangeGoo(IFlange flange)
        : base(flange)
        {
        }

        public override bool IsValid => true;

        public override string TypeName => "Flange Profile";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            return new AdSecProfileFlangeGoo(this.Value);
        }
        public override string ToString()
        {
            string flange = "AdSec Flange {";
            UnitsNet.Length thk1 = this.Value.Width.ToUnit(GhAdSec.DocumentUnits.LengthUnit);
            UnitsNet.Length thk2 = this.Value.Thickness.ToUnit(GhAdSec.DocumentUnits.LengthUnit);
            flange += "Width:" + thk1.ToString() + ", Thk:" + thk2.ToString() + "}";
            return flange;
        }
    }
}
