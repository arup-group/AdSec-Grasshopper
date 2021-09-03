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
using Oasys.AdSec.Reinforcement.Preloads;

namespace GhAdSec.Parameters
{
    public class AdSecPreLoadGoo : GH_Goo<IPreload>
    {
        public AdSecPreLoadGoo(IPreload load)
        : base(load)
        {
        }

        public override bool IsValid => true;

        public override string TypeName => "Prestress";

        public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";

        public override IGH_Goo Duplicate()
        {
            return new AdSecPreLoadGoo(this.Value);
        }
        public override string ToString()
        {
            string str = "";
            try
            {
                IQuantity force = new UnitsNet.Force(0, GhAdSec.DocumentUnits.ForceUnit);
                string forceUnitAbbreviation = string.Concat(force.ToString().Where(char.IsLetter));
                IPreForce frs = (IPreForce)Value;
                str = Math.Round(frs.Force.As(GhAdSec.DocumentUnits.ForceUnit), 4) + forceUnitAbbreviation;
            }
            catch (Exception)
            {
                try
                {
                    IQuantity strain = new Oasys.Units.Strain(0, GhAdSec.DocumentUnits.StrainUnit);
                    string strainUnitAbbreviation = string.Concat(strain.ToString().Where(char.IsLetter));
                    IPreStrain stra = (IPreStrain)Value;
                    str = Math.Round(stra.Strain.As(GhAdSec.DocumentUnits.StrainUnit), 4) + strainUnitAbbreviation;
                }
                catch (Exception)
                {
                    IQuantity stress = new UnitsNet.Pressure(0, GhAdSec.DocumentUnits.StressUnit);
                    string stressUnitAbbreviation = string.Concat(stress.ToString().Where(char.IsLetter));
                    IPreStress stre = (IPreStress)Value;
                    str = Math.Round(stre.Stress.As(GhAdSec.DocumentUnits.StressUnit), 4) + stressUnitAbbreviation;
                }
            }

            return "AdSec " + TypeName + " {" + str + "}";
        }
    }
}
