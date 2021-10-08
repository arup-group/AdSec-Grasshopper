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

namespace AdSecGH.Parameters
{
    public class AdSecSolutionGoo : GH_Goo<ISolution>
    {
        public AdSecSolutionGoo(ISolution solution, Plane local)
        : base(solution)
        {
            m_plane = local;
        }
        private Plane m_plane;
        internal Plane LocalPlane
        {
            get { return m_plane; }
        }
        public override bool IsValid => true;

        public override string TypeName => "Results";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            return new AdSecSolutionGoo(this.Value, m_plane);
        }
        public override string ToString()
        {
            return "AdSec " + TypeName; // + " {"
        }
    }
}
