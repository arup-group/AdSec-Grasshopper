using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Grasshopper.Documentation;
using Rhino.Collections;
using Oasys.AdSec.DesignCode;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec;
using Oasys.AdSec.StandardMaterials;
using Oasys.Profiles;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using UnitsNet;
using Oasys.Units;

namespace GhAdSec
{
    /// <summary>
    /// Class to hold units used in Grasshopper AdSec file. 
    /// </summary>
    public static class DocumentUnits
    {
        public static Oasys.Units.StrainUnit StrainUnit
        {
            get { return m_strain; }
            set { m_strain = value; }
        }
        private static Oasys.Units.StrainUnit m_strain = Oasys.Units.StrainUnit.Ratio;
        public static UnitsNet.Units.PressureUnit PressureUnit
        {
            get { return m_pressure; }
            set { m_pressure = value; }
        }
        private static UnitsNet.Units.PressureUnit m_pressure = UnitsNet.Units.PressureUnit.Megapascal;
    }
}
