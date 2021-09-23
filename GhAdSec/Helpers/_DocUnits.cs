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
using UnitsNet.Units;

namespace AdSecGH.Helpers
{
    /// <summary>
    /// Class to hold filtered list of units used in AdSecGH. 
    /// </summary>
    internal static class FilteredUnitLists
    {
        internal static List<string> LengthUnits = new List<string>()
        {
            LengthUnit.Millimeter.ToString(),
            LengthUnit.Centimeter.ToString(),
            LengthUnit.Meter.ToString(),
            LengthUnit.Inch.ToString(),
            LengthUnit.Foot.ToString()
        };
        internal static List<string> AngleUnits = new List<string>()
        {
            AngleUnit.Radian.ToString(),
            AngleUnit.Degree.ToString()
        };
        internal static List<string> ForceUnits = new List<string>()
        {
            ForceUnit.Newton.ToString(),
            ForceUnit.Kilonewton.ToString(),
            ForceUnit.Meganewton.ToString(),
            ForceUnit.PoundForce.ToString(),
            ForceUnit.KilopoundForce.ToString(),
            ForceUnit.TonneForce.ToString()
        };
        internal static List<string> MomentUnits = Enum.GetNames(typeof(MomentUnit)).ToList();
        //    new List<string>()
        //{
        //    // to be implemented
        //    Oasys.Units.MomentUnit.
        //};
        internal static List<string> StressUnits = new List<string>()
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
        internal static List<string> StrainUnits = new List<string>()
        {
            StrainUnit.Ratio.ToString(),
            StrainUnit.Percent.ToString(),
            StrainUnit.MilliStrain.ToString(),
            StrainUnit.MicroStrain.ToString()
        };
        internal static List<string> CurvatureUnits = new List<string>()
        {
            CurvatureUnit.PerMillimeter.ToString(),
            CurvatureUnit.PerCentimeter.ToString(),
            CurvatureUnit.PerMeter.ToString(),
            CurvatureUnit.PerInch.ToString(),
            CurvatureUnit.PerFoot.ToString()
        };
    }
    /// <summary>
    /// Class to hold units used in Grasshopper AdSec file. 
    /// </summary>
    internal class DocUnits
    {
        #region fields
        internal enum AdSecUnitTypes
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
        private LengthUnit m_length = LengthUnit.Undefined;
        private ForceUnit m_force = ForceUnit.Kilonewton;
        private MomentUnit m_moment = MomentUnit.KilonewtonMeter;
        private PressureUnit m_stress = PressureUnit.Megapascal;
        private StrainUnit m_strain = StrainUnit.MilliStrain;
        private AxialStiffnessUnit m_axialstiffness = AxialStiffnessUnit.Kilonewton;
        private CurvatureUnit m_curvature = CurvatureUnit.Undefined;
        private BendingStiffnessUnit m_bendingstiffness = BendingStiffnessUnit.KilonewtonSquareMeter;
        private Guid m_docID;
        private readonly List<string> adsecunits = new List<string>() { "Length", "Force", "Moment", "Stress",
                "Strain", "AxialStiffness", "Curvature", "BendingStiffness"};
        #endregion

        private void GetSettings()
        {
            foreach (string unit in adsecunits)
            {
                string myUnit = "AdSec" + unit + "Unit";
                if (Grasshopper.Instances.Settings.ConstainsEntry(myUnit))
                {
                    switch (unit)
                    {
                        case "Length":
                            //Grasshopper.Instances.Settings.GetValue(myUnit);
                            break;
                    }
                    
                }
            
            }
        }

        #region constructors
        internal DocUnits(GH_Document gh_document)
        {
            m_docID = gh_document.DocumentID;
        }
        internal DocUnits(string length, string force, string moment, string stress, string strain, 
            string axialstiffness, string curvature, string bendingstiffness, Guid docID)
        {
            m_length = (LengthUnit)Enum.Parse(typeof(LengthUnit), length);
            m_force = (ForceUnit)Enum.Parse(typeof(ForceUnit), force);
            m_moment = (MomentUnit)Enum.Parse(typeof(MomentUnit), moment);
            m_stress = (PressureUnit)Enum.Parse(typeof(PressureUnit), stress);
            m_strain = (StrainUnit)Enum.Parse(typeof(StrainUnit), strain);
            m_axialstiffness = (AxialStiffnessUnit)Enum.Parse(typeof(AxialStiffnessUnit), axialstiffness);
            m_curvature = (CurvatureUnit)Enum.Parse(typeof(CurvatureUnit), curvature);
            m_bendingstiffness = (BendingStiffnessUnit)Enum.Parse(typeof(BendingStiffnessUnit), bendingstiffness);
            m_docID = docID;
        }
        #endregion

        #region properties
        internal Guid DocumentID
        { 
            get { return m_docID; } 
        }
        internal LengthUnit LengthUnit
        {
            get
            {
                if (m_length == LengthUnit.Undefined)
                {
                    m_length = GetRhinoLengthUnit(RhinoDoc.ActiveDoc.ModelUnitSystem);
                }
                return m_length;
            }
            set { m_length = value; }
        }
        internal ForceUnit ForceUnit
        {
            get { return m_force; }
            set { m_force = value; }
        }
        internal MomentUnit MomentUnit
        {
            get { return m_moment; }
            set { m_moment = value; }
        }
        internal PressureUnit StressUnit
        {
            get { return m_stress; }
            set { m_stress = value; }
        }
        internal StrainUnit StrainUnit
        {
            get { return m_strain; }
            set { m_strain = value; }
        }
        internal AxialStiffnessUnit AxialStiffnessUnit
        {
            get { return m_axialstiffness; }
            set { m_axialstiffness = value; }
        }
        internal BendingStiffnessUnit BendingStiffnessUnit
        {
            get { return m_bendingstiffness; }
            set { m_bendingstiffness = value; }
        }
        internal CurvatureUnit CurvatureUnit
        {
            get 
            {
                if (m_curvature == CurvatureUnit.Undefined)
                {
                    m_curvature = (CurvatureUnit)Enum.Parse(typeof(CurvatureUnit), "Per" + LengthUnit.ToString());
                }
                return m_curvature; 
            }
            
        set { m_curvature = value; }
        }
        #endregion


        #region unit system
        internal UnitsNet.UnitSystem UnitSystem
        {
            get { return m_units; }
            set { m_units = value; }
        }
        private UnitsNet.UnitSystem m_units;
        #endregion
        #region methods
        internal void SetupUnits()
        {
            // get SI units
            UnitsNet.UnitSystem si = UnitsNet.UnitSystem.SI;

            // get rhino document length unit
            LengthUnit length = GetRhinoLengthUnit(RhinoDoc.ActiveDoc.ModelUnitSystem);

            BaseUnits units = new BaseUnits(
                length,
                si.BaseUnits.Mass, si.BaseUnits.Time, si.BaseUnits.Current, si.BaseUnits.Temperature, si.BaseUnits.Amount, si.BaseUnits.LuminousIntensity);
            m_units = new UnitsNet.UnitSystem(units);
        }
        internal LengthUnit GetRhinoLengthUnit(Rhino.UnitSystem rhinoUnits)
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
