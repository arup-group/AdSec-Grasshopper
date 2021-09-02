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
using Oasys.AdSec;
using UnitsNet;

namespace GhAdSec.Parameters
{
    public class AdSecLoadGoo : GH_Goo<ILoad>
    {
        public AdSecLoadGoo(ILoad load)
        : base(load)
        {
        }

        public override bool IsValid => true;

        public override string TypeName => "Load";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            return new AdSecLoadGoo(this.Value);
        }
        public override string ToString()
        {
            IQuantity quantityMoment = new Oasys.Units.Moment(0, GhAdSec.DocumentUnits.MomentUnit);
            string unitMomentAbbreviation = string.Concat(quantityMoment.ToString().Where(char.IsLetter));
            IQuantity quantityForce = new UnitsNet.Force(0, GhAdSec.DocumentUnits.ForceUnit);
            string unitforceAbbreviation = string.Concat(quantityForce.ToString().Where(char.IsLetter));
            return "AdSec " + TypeName + " {"
                + Math.Round(this.Value.X.As(GhAdSec.DocumentUnits.ForceUnit), 4) + unitforceAbbreviation + ", "
                + Math.Round(this.Value.YY.As(GhAdSec.DocumentUnits.MomentUnit), 4) + unitMomentAbbreviation + ", "
                + Math.Round(this.Value.ZZ.As(GhAdSec.DocumentUnits.MomentUnit), 4) + unitMomentAbbreviation + "}";
        }
    }
}
