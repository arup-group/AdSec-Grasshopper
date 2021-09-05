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
        public enum AdSecUnits
        {
            Length,
            Force,
            Moment,
            Stress,
            Strain,
            AxialStiffness,
            BendingStiffness,
            Curvature
        }
        internal static List<string> FilteredAngleUnits = new List<string>()
        {
            UnitsNet.Units.AngleUnit.Radian.ToString(),
            UnitsNet.Units.AngleUnit.Degree.ToString()
        };
        #region length
        public static UnitsNet.Units.LengthUnit LengthUnit
        {
            get
            {
                if (m_units == null)
                {
                    m_length = GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
                }
                else
                {
                    m_length = m_units.BaseUnits.Length;
                }
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
        internal static List<string> FilteredLengthUnits = new List<string>()
        {
            UnitsNet.Units.LengthUnit.Millimeter.ToString(),
            UnitsNet.Units.LengthUnit.Centimeter.ToString(),
            UnitsNet.Units.LengthUnit.Meter.ToString(),
            UnitsNet.Units.LengthUnit.Inch.ToString(),
            UnitsNet.Units.LengthUnit.Foot.ToString()
        };
        #endregion

        #region force
        public static UnitsNet.Units.ForceUnit ForceUnit
        {
            get { return m_force; }
            set { m_force = value; }
        }
        private static UnitsNet.Units.ForceUnit m_force = UnitsNet.Units.ForceUnit.Kilonewton;
        internal static List<string> FilteredForceUnits = new List<string>()
        {
            UnitsNet.Units.ForceUnit.Newton.ToString(),
            UnitsNet.Units.ForceUnit.Kilonewton.ToString(),
            UnitsNet.Units.ForceUnit.Meganewton.ToString(),
            UnitsNet.Units.ForceUnit.PoundForce.ToString(),
            UnitsNet.Units.ForceUnit.KilopoundForce.ToString(),
            UnitsNet.Units.ForceUnit.TonneForce.ToString()
        };
        #endregion

        #region moment
        public static Oasys.Units.MomentUnit MomentUnit
        {
            get { return m_moment; }
            set { m_moment = value; }
        }
        private static Oasys.Units.MomentUnit m_moment = Oasys.Units.MomentUnit.KilonewtonMeter;
        internal static List<string> FilteredMomentUnits = Enum.GetNames(typeof(Oasys.Units.MomentUnit)).ToList();
        //    new List<string>()
        //{
        //    // to be implemented
        //    Oasys.Units.MomentUnit.
        //};
        #endregion
        #region stress
        public static UnitsNet.Units.PressureUnit StressUnit
        {
            get { return m_stress; }
            set { m_stress = value; }
        }
        private static UnitsNet.Units.PressureUnit m_stress = UnitsNet.Units.PressureUnit.Megapascal;
        internal static List<string> FilteredStressUnits = new List<string>()
        {
            UnitsNet.Units.PressureUnit.Pascal.ToString(),
            UnitsNet.Units.PressureUnit.Kilopascal.ToString(),
            UnitsNet.Units.PressureUnit.Megapascal.ToString(),
            UnitsNet.Units.PressureUnit.Gigapascal.ToString(),
            UnitsNet.Units.PressureUnit.NewtonPerSquareMillimeter.ToString(),
            UnitsNet.Units.PressureUnit.KilonewtonPerSquareCentimeter.ToString(),
            UnitsNet.Units.PressureUnit.NewtonPerSquareMeter.ToString(),
            UnitsNet.Units.PressureUnit.KilopoundForcePerSquareInch.ToString(),
            UnitsNet.Units.PressureUnit.PoundForcePerSquareInch.ToString(),
            UnitsNet.Units.PressureUnit.PoundForcePerSquareFoot.ToString(),
            UnitsNet.Units.PressureUnit.KilopoundForcePerSquareInch.ToString(),
            UnitsNet.Units.PressureUnit.KilopoundForcePerSquareFoot.ToString()
        };
        #endregion
        #region strain
        public static Oasys.Units.StrainUnit StrainUnit
        {
            get { return m_strain; }
            set { m_strain = value; }
        }
        private static Oasys.Units.StrainUnit m_strain = Oasys.Units.StrainUnit.MilliStrain;
        internal static List<string> FilteredStrainUnits = new List<string>()
        {
            Oasys.Units.StrainUnit.Ratio.ToString(),
            Oasys.Units.StrainUnit.Percent.ToString(),
            Oasys.Units.StrainUnit.MilliStrain.ToString(),
            Oasys.Units.StrainUnit.MicroStrain.ToString()
        };
        #endregion
        #region axial stiffness
        public static Oasys.Units.AxialStiffnessUnit AxialStiffnessUnit
        {
            get { return m_axialstiffness; }
            set { m_axialstiffness = value; }
        }
        private static Oasys.Units.AxialStiffnessUnit m_axialstiffness = Oasys.Units.AxialStiffnessUnit.Kilonewton;
        #endregion
        #region bending stiffness
        public static Oasys.Units.BendingStiffnessUnit BendingStiffnessUnit
        {
            get { return m_bendingstiffness; }
            set { m_bendingstiffness = value; }
        }
        private static Oasys.Units.BendingStiffnessUnit m_bendingstiffness = Oasys.Units.BendingStiffnessUnit.KilonewtonSquareMeter;
        #endregion
        #region curvature
        public static Oasys.Units.CurvatureUnit CurvatureUnit
        {
            get { return m_curvature; }
            set { m_curvature = value; }
        }
        private static Oasys.Units.CurvatureUnit m_curvature = (Oasys.Units.CurvatureUnit)Enum.Parse(typeof(Oasys.Units.CurvatureUnit), "Per" + LengthUnit.ToString());
        internal static List<string> FilteredCurvatureUnits = new List<string>()
        {
            Oasys.Units.CurvatureUnit.PerMillimeter.ToString(),
            Oasys.Units.CurvatureUnit.PerCentimeter.ToString(),
            Oasys.Units.CurvatureUnit.PerMeter.ToString(),
            Oasys.Units.CurvatureUnit.PerInch.ToString(),
            Oasys.Units.CurvatureUnit.PerFoot.ToString()
        };
        #endregion
        #region unit system
        public static UnitsNet.UnitSystem UnitSystem
        {
            get { return m_units; }
            set { m_units = value; }
        }
        private static UnitsNet.UnitSystem m_units;
        #endregion
        #region methods
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
        #endregion
    }
}
