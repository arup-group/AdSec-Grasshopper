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
        private static Oasys.Units.StrainUnit m_strain = Oasys.Units.StrainUnit.MilliStrain;
        public static UnitsNet.Units.PressureUnit PressureUnit
        {
            get { return m_pressure; }
            set { m_pressure = value; }
        }
        private static UnitsNet.Units.PressureUnit m_pressure = UnitsNet.Units.PressureUnit.Megapascal;

        public static UnitsNet.Units.LengthUnit LengthUnit
        {
            get 
            {
                m_length = m_units.BaseUnits.Length;
                return m_length; 
            }
            set 
            {
                m_length = value;
                // update unit system
                UnitsNet.BaseUnits units = new UnitsNet.BaseUnits(
                    m_length,
                    m_units.BaseUnits.Mass, m_units.BaseUnits.Time, m_units.BaseUnits.Current, m_units.BaseUnits.Temperature, m_units.BaseUnits.Amount, m_units.BaseUnits.LuminousIntensity);
                m_units = new UnitsNet.UnitSystem(units);
            }
        }
        private static UnitsNet.Units.LengthUnit m_length;

        public static UnitsNet.UnitSystem UnitSystem
        {
            get { return m_units; }
            set { m_units = value; }
        }
        private static UnitsNet.UnitSystem m_units;

        internal static void SetupUnits()
        {
            // get SI units
            UnitsNet.UnitSystem si = UnitsNet.UnitSystem.SI;

            // get rhino document length unit
            UnitsNet.Units.LengthUnit length = GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);

            UnitsNet.BaseUnits units = new UnitsNet.BaseUnits(
                length,
                si.BaseUnits.Mass, si.BaseUnits.Time, si.BaseUnits.Current, si.BaseUnits.Temperature, si.BaseUnits.Amount, si.BaseUnits.LuminousIntensity);
            m_units = new UnitsNet.UnitSystem(units);
        }
        internal static UnitsNet.Units.LengthUnit GetRhinoLengthUnit(Rhino.UnitSystem rhinoUnits)
        {
            List<int> id = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 });
            List<string> name = new List<string>(new string[] {
                "None", 
                "Microns", 
                "mm", 
                "cm", 
                "m", 
                "km",
                "Microinches", 
                "Mils", 
                "in", 
                "ft", 
                "Miles", 
                " ", 
                "Angstroms", 
                "Nanometers", 
                "Decimeters", 
                "Dekameters",
                "Hectometers", 
                "Megameters", 
                "Gigameters", 
                "Yards" });
            List<UnitsNet.Units.LengthUnit> unit = new List<UnitsNet.Units.LengthUnit>(new UnitsNet.Units.LengthUnit[] {
                UnitsNet.Units.LengthUnit.Undefined,
                UnitsNet.Units.LengthUnit.Micrometer,
                UnitsNet.Units.LengthUnit.Millimeter,
                UnitsNet.Units.LengthUnit.Centimeter,
                UnitsNet.Units.LengthUnit.Meter,
                UnitsNet.Units.LengthUnit.Kilometer,
                UnitsNet.Units.LengthUnit.Microinch,
                UnitsNet.Units.LengthUnit.Mil,
                UnitsNet.Units.LengthUnit.Inch,
                UnitsNet.Units.LengthUnit.Foot,
                UnitsNet.Units.LengthUnit.Mile,
                UnitsNet.Units.LengthUnit.Undefined,
                UnitsNet.Units.LengthUnit.Undefined,
                UnitsNet.Units.LengthUnit.Nanometer,
                UnitsNet.Units.LengthUnit.Decimeter,
                UnitsNet.Units.LengthUnit.Undefined,
                UnitsNet.Units.LengthUnit.Hectometer,
                UnitsNet.Units.LengthUnit.Undefined,
                UnitsNet.Units.LengthUnit.Undefined,
                UnitsNet.Units.LengthUnit.Yard });
            for (int i = 0; i < id.Count; i++)
                if (rhinoUnits.GetHashCode() == id[i])
                    return unit[i];
            return UnitsNet.Units.LengthUnit.Undefined;
        }
    }
}
