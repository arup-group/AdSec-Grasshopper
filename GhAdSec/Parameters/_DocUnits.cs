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
using UnitsNet.Units;
using Oasys.Units;

namespace AdSecGH
{
    /// <summary>
    /// Class to hold units used in Grasshopper AdSec file. 
    /// </summary>
    public static class Units
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
            AngleUnit.Radian.ToString(),
            AngleUnit.Degree.ToString()
        };
        #region length
        internal static bool useRhinoLengthUnit;
        public static LengthUnit LengthUnit
        {
            get
            {
                if (m_units == null || useRhinoLengthUnit)
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
        private static LengthUnit m_length;
        internal static List<string> FilteredLengthUnits = new List<string>()
        {
            LengthUnit.Millimeter.ToString(),
            LengthUnit.Centimeter.ToString(),
            LengthUnit.Meter.ToString(),
            LengthUnit.Inch.ToString(),
            LengthUnit.Foot.ToString()
        };
        #endregion

        #region force
        public static ForceUnit ForceUnit
        {
            get { return m_force; }
            set { m_force = value; }
        }
        private static ForceUnit m_force = ForceUnit.Kilonewton;
        internal static List<string> FilteredForceUnits = new List<string>()
        {
            ForceUnit.Newton.ToString(),
            ForceUnit.Kilonewton.ToString(),
            ForceUnit.Meganewton.ToString(),
            ForceUnit.PoundForce.ToString(),
            ForceUnit.KilopoundForce.ToString(),
            ForceUnit.TonneForce.ToString()
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
        #endregion
        #region stress
        public static PressureUnit StressUnit
        {
            get { return m_stress; }
            set { m_stress = value; }
        }
        private static PressureUnit m_stress = PressureUnit.Megapascal;
        internal static List<string> FilteredStressUnits = new List<string>()
        {
            PressureUnit.Pascal.ToString(),
            PressureUnit.Kilopascal.ToString(),
            PressureUnit.Megapascal.ToString(),
            PressureUnit.Gigapascal.ToString(),
            PressureUnit.NewtonPerSquareMillimeter.ToString(),
            PressureUnit.KilonewtonPerSquareCentimeter.ToString(),
            PressureUnit.NewtonPerSquareMeter.ToString(),
            PressureUnit.KilopoundForcePerSquareInch.ToString(),
            PressureUnit.PoundForcePerSquareInch.ToString(),
            PressureUnit.PoundForcePerSquareFoot.ToString(),
            PressureUnit.KilopoundForcePerSquareInch.ToString(),
            PressureUnit.KilopoundForcePerSquareFoot.ToString()
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
        internal static List<string> FilteredAxialStiffnessUnits = Enum.GetNames(typeof(Oasys.Units.AxialStiffnessUnit)).ToList();
        #endregion
        #region bending stiffness
        public static Oasys.Units.BendingStiffnessUnit BendingStiffnessUnit
        {
            get { return m_bendingstiffness; }
            set { m_bendingstiffness = value; }
        }
        private static Oasys.Units.BendingStiffnessUnit m_bendingstiffness = Oasys.Units.BendingStiffnessUnit.KilonewtonSquareMeter;
        internal static List<string> FilteredBendingStiffnessUnits = Enum.GetNames(typeof(Oasys.Units.BendingStiffnessUnit)).ToList();
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
            bool settingsExist = ReadSettings();
            LengthUnit length;
            if (settingsExist)
            {
                length = m_length;
            }
            else
            {
                // get rhino document length unit
                m_length = GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
                SaveSettings();
            }
            // get SI units
            UnitsNet.UnitSystem si = UnitsNet.UnitSystem.SI;

            UnitsNet.BaseUnits units = new UnitsNet.BaseUnits(
                m_length,
                si.BaseUnits.Mass, si.BaseUnits.Time, si.BaseUnits.Current, si.BaseUnits.Temperature, si.BaseUnits.Amount, si.BaseUnits.LuminousIntensity);
            m_units = new UnitsNet.UnitSystem(units);

        }
        internal static void SaveSettings()
        {
            Grasshopper.Instances.Settings.SetValue("AdSecLengthUnit", LengthUnit.ToString());
            Grasshopper.Instances.Settings.SetValue("AdSecUseRhinoLengthUnit", useRhinoLengthUnit);
            Grasshopper.Instances.Settings.SetValue("AdSecForceUnit", ForceUnit.ToString());
            Grasshopper.Instances.Settings.SetValue("AdSecMomentUnit", MomentUnit.ToString());
            Grasshopper.Instances.Settings.SetValue("AdSecStressUnit", StressUnit.ToString());
            Grasshopper.Instances.Settings.SetValue("AdSecStrainUnit", StrainUnit.ToString());
            Grasshopper.Instances.Settings.SetValue("AdSecAxialStiffnessUnit", AxialStiffnessUnit.ToString());
            Grasshopper.Instances.Settings.SetValue("AdSecCurvatureUnit", CurvatureUnit.ToString());
            Grasshopper.Instances.Settings.SetValue("AdSecBendingStiffnessUnit", BendingStiffnessUnit.ToString());
            Grasshopper.Instances.Settings.WritePersistentSettings();
        }
        internal static bool ReadSettings()
        {
            if (!Grasshopper.Instances.Settings.ConstainsEntry("AdSecLengthUnit"))
                return false;

            string length = Grasshopper.Instances.Settings.GetValue("AdSecLengthUnit", string.Empty);
            string force = Grasshopper.Instances.Settings.GetValue("AdSecForceUnit", string.Empty);
            string moment = Grasshopper.Instances.Settings.GetValue("AdSecMomentUnit", string.Empty);
            string stress = Grasshopper.Instances.Settings.GetValue("AdSecStressUnit", string.Empty);
            string strain = Grasshopper.Instances.Settings.GetValue("AdSecStrainUnit", string.Empty);
            string axialstiffness = Grasshopper.Instances.Settings.GetValue("AdSecAxialStiffnessUnit", string.Empty);
            string curvature = Grasshopper.Instances.Settings.GetValue("AdSecCurvatureUnit", string.Empty);
            string bendingstiffness = Grasshopper.Instances.Settings.GetValue("AdSecBendingStiffnessUnit", string.Empty);

            useRhinoLengthUnit = Grasshopper.Instances.Settings.GetValue("AdSecUseRhinoLengthUnit", false);

            if (useRhinoLengthUnit)
            {
                m_length = GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
            }
            else
            {
                m_length = (LengthUnit)Enum.Parse(typeof(LengthUnit), length);
            }

            m_force = (ForceUnit)Enum.Parse(typeof(ForceUnit), force);
            m_moment = (MomentUnit)Enum.Parse(typeof(MomentUnit), moment);
            m_stress = (PressureUnit)Enum.Parse(typeof(PressureUnit), stress);
            m_strain = (StrainUnit)Enum.Parse(typeof(StrainUnit), strain);
            m_axialstiffness = (AxialStiffnessUnit)Enum.Parse(typeof(AxialStiffnessUnit), axialstiffness);
            m_curvature = (CurvatureUnit)Enum.Parse(typeof(CurvatureUnit), curvature);
            m_bendingstiffness = (BendingStiffnessUnit)Enum.Parse(typeof(BendingStiffnessUnit), bendingstiffness);

            return true;
        }
        internal static LengthUnit GetRhinoLengthUnit()
        {
            return GetRhinoLengthUnit(Rhino.RhinoDoc.ActiveDoc.ModelUnitSystem);
        }
        internal static LengthUnit GetRhinoLengthUnit(Rhino.UnitSystem rhinoUnits)
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
            List<LengthUnit> unit = new List<LengthUnit>(new LengthUnit[] {
                LengthUnit.Undefined,
                LengthUnit.Micrometer,
                LengthUnit.Millimeter,
                LengthUnit.Centimeter,
                LengthUnit.Meter,
                LengthUnit.Kilometer,
                LengthUnit.Microinch,
                LengthUnit.Mil,
                LengthUnit.Inch,
                LengthUnit.Foot,
                LengthUnit.Mile,
                LengthUnit.Undefined,
                LengthUnit.Undefined,
                LengthUnit.Nanometer,
                LengthUnit.Decimeter,
                LengthUnit.Undefined,
                LengthUnit.Hectometer,
                LengthUnit.Undefined,
                LengthUnit.Undefined,
                LengthUnit.Yard });
            for (int i = 0; i < id.Count; i++)
                if (rhinoUnits.GetHashCode() == id[i])
                    return unit[i];
            return LengthUnit.Undefined;
        }
        #endregion
    }
}
