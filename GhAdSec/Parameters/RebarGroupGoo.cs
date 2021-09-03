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
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec;

namespace GhAdSec.Parameters
{
    public class AdSecRebarGroupGoo : GH_Goo<IGroup>
    {
        public AdSecRebarGroupGoo(IGroup group)
        : base(group)
        {
        }

        public override bool IsValid => true;
        public override string TypeName => "Rebar Layout";
        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            return new AdSecRebarGroupGoo(this.Value);
        }

        public override string ToString()
        {
            string m_ToString = "";
            try
            {
                IArcGroup arc = (IArcGroup)Value;
                m_ToString = "Arc Type Layout";

            }
            catch (Exception)
            {
                try
                {
                    ICircleGroup cir = (ICircleGroup)Value;
                    m_ToString = "Circle Type Layout";
                }
                catch (Exception)
                {
                    try
                    {
                        ILineGroup lin = (ILineGroup)Value;
                        m_ToString = "Line Type Layout";
                    }
                    catch (Exception)
                    {
                        try
                        {
                            ISingleBars sin = (ISingleBars)Value;
                            m_ToString = "SingleBars Type Layout";
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }

            return "AdSec " + TypeName + " {" + m_ToString + "}";
        }
    }
}
