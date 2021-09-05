using GhAdSec.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.Profiles;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet.GH;
using UnitsNet;
using UnitsNet.Units;
using Oasys.Units;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec;
using Oasys.AdSec.Reinforcement.Groups;

namespace GhAdSec.Components
{
    class GetInput
    {
        internal static Length Length(GH_Component owner, IGH_DataAccess DA, int inputid, UnitsNet.Units.LengthUnit docLengthUnit, bool isOptional = false)
        {
            GH_UnitNumber unitNumber = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    unitNumber = (GH_UnitNumber)gh_typ.Value;
                    // check that unit is of right type
                    if (!unitNumber.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.LengthUnit)))
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].Name + " input, index " + inputid + ": Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + unitNumber.Value.QuantityInfo.Name + " but must be Length");
                        return UnitsNet.Length.Zero;
                    }
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    unitNumber = new GH_UnitNumber(new UnitsNet.Length(val, docLengthUnit));
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to UnitNumber");
                    return UnitsNet.Length.Zero;
                }
            }
            else if (!isOptional)
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            return (UnitsNet.Length)unitNumber.Value;
        }
        internal static IFlange Flange(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecProfileFlangeGoo flange = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecProfileFlangeGoo)
                {
                    flange = (AdSecProfileFlangeGoo)gh_typ.Value;
                    return flange.Value;
                }
                // try cast from web
                else if (gh_typ.Value is AdSecProfileWebGoo)
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") from Web to Flange");
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to Flange");
                    return null;
                }
            }
            else if (!isOptional)
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            return null;
        }
        internal static IWeb Web(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecProfileWebGoo web = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecProfileWebGoo)
                {
                    web = (AdSecProfileWebGoo)gh_typ.Value;
                    return web.Value;
                }
                // try cast from web
                else if (gh_typ.Value is AdSecProfileFlangeGoo)
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") from Flange to Web");
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to Web");
                    return null;
                }
            }
            else if (!isOptional)
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            return null;
        }
        internal static IStressStrainPoint StressStrainPoint(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            IStressStrainPoint pt1 = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                Point3d ghpt = new Point3d();
                if (gh_typ.Value is IStressStrainPoint)
                {
                    pt1 = (IStressStrainPoint)gh_typ.Value;
                }
                else if (gh_typ.Value is AdSecStressStrainPointGoo)
                {
                    AdSecStressStrainPointGoo sspt = (AdSecStressStrainPointGoo)gh_typ.Value;
                    pt1 = sspt.StressStrainPoint;
                }
                else if (GH_Convert.ToPoint3d(gh_typ.Value, ref ghpt, GH_Conversion.Both))
                {
                    pt1 = GhAdSec.Parameters.AdSecStressStrainPointGoo.CreateFromPoint3d(ghpt);
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to StressStrainPoint");
                    return null;
                }
                return pt1;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static Oasys.Collections.IList<IStressStrainPoint> StressStrainPoints(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
            {
            Oasys.Collections.IList<IStressStrainPoint> pts = Oasys.Collections.IList<IStressStrainPoint>.Create();
            List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
            if (DA.GetDataList(inputid, gh_typs))
            {
                for (int i = 0; i < gh_typs.Count; i++)
                {
                    Curve polycurve = null;
                    Point3d ghpt = new Point3d();
                    if (gh_typs[i].Value is IStressStrainPoint)
                    {
                        pts.Add((IStressStrainPoint)gh_typs[i].Value);
                    }
                    else if (gh_typs[i].Value is AdSecStressStrainPointGoo)
                    {
                        AdSecStressStrainPointGoo sspt = (AdSecStressStrainPointGoo)gh_typs[i].Value;
                        pts.Add(sspt.StressStrainPoint);
                    }
                    else if (GH_Convert.ToPoint3d(gh_typs[i].Value, ref ghpt, GH_Conversion.Both))
                    {
                        pts.Add(GhAdSec.Parameters.AdSecStressStrainPointGoo.CreateFromPoint3d(ghpt));
                    }
                    else if (GH_Convert.ToCurve(gh_typs[i].Value, ref polycurve, GH_Conversion.Both))
                    {
                        PolylineCurve curve = (PolylineCurve)polycurve;
                        pts = GhAdSec.Parameters.AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
                    }
                    else
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + "), index in input list: " + i + ", to a StressStrainPoint or a Polyline");
                        return null;
                    }
                }
                if (pts.Count < 2)
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input must contain at least 2 points to create an Explicit Stress Strain Curve");
                    return null;
                }
                return pts;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static AdSecStressStrainCurveGoo StressStrainCurveGoo(GH_Component owner, IGH_DataAccess DA, int inputid, bool compression, bool isOptional = false)
        {
            AdSecStressStrainCurveGoo ssCrv = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                Curve polycurve = null;
                if (gh_typ.Value is AdSecStressStrainCurveGoo)
                {
                    // try direct cast
                    ssCrv = (AdSecStressStrainCurveGoo)gh_typ.Value;
                }
                else if (GH_Convert.ToCurve(gh_typ.Value, ref polycurve, GH_Conversion.Both))
                {
                    // try convert to polylinecurve
                    PolylineCurve curve = (PolylineCurve)polycurve;
                    Oasys.Collections.IList<IStressStrainPoint> pts = GhAdSec.Parameters.AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
                    IExplicitStressStrainCurve exCrv = IExplicitStressStrainCurve.Create();
                    exCrv.Points = pts;
                    Tuple<Curve, List<Point3d>> tuple = GhAdSec.Parameters.AdSecStressStrainCurveGoo.Create(exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, compression);
                    ssCrv = new AdSecStressStrainCurveGoo(tuple.Item1, exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to StressStrainCurve");
                    return null;
                }
                return ssCrv;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static Pressure Stress(GH_Component owner, IGH_DataAccess DA, int inputid, UnitsNet.Units.PressureUnit stressUnit, bool isOptional = false)
        {
            UnitsNet.Pressure stressFib = new UnitsNet.Pressure();

            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                GH_UnitNumber inStress;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    inStress = (GH_UnitNumber)gh_typ.Value;
                    if (!inStress.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.PressureUnit)))
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].Name + " input, index " + inputid + ": Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + inStress.Value.QuantityInfo.Name + " but must be Stress (Pressure)");
                        return Pressure.Zero;
                    }
                    stressFib = (UnitsNet.Pressure)inStress.Value.ToUnit(stressUnit);
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    inStress = new GH_UnitNumber(new UnitsNet.Pressure(val, stressUnit));
                    stressFib = (UnitsNet.Pressure)inStress.Value;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to a UnitNumber of Stress");
                    return Pressure.Zero;
                }
                return stressFib;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return Pressure.Zero;
        }
        internal static Strain Strain(GH_Component owner, IGH_DataAccess DA, int inputid, StrainUnit strainUnit, bool isOptional = false)
        {
            Oasys.Units.Strain strainFib = new Oasys.Units.Strain();

            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                GH_UnitNumber inStrain;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    inStrain = (GH_UnitNumber)gh_typ.Value;
                    if (!inStrain.Value.QuantityInfo.UnitType.Equals(typeof(Oasys.Units.StrainUnit)))
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].Name + " input, index " + inputid + ": Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + inStrain.Value.QuantityInfo.Name + " but must be Strain");
                        return Oasys.Units.Strain.Zero;
                    }
                    strainFib = (Oasys.Units.Strain)inStrain.Value.ToUnit(strainUnit);
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    inStrain = new GH_UnitNumber(new Oasys.Units.Strain(val, strainUnit));
                    strainFib = (Oasys.Units.Strain)inStrain.Value;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to a UnitNumber of Strain");
                    return Oasys.Units.Strain.Zero;
                }
                return strainFib;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return Oasys.Units.Strain.Zero;
        }
        internal static Curvature Curvature(GH_Component owner, IGH_DataAccess DA, int inputid, CurvatureUnit curvatureUnit, bool isOptional = false)
        {
            Oasys.Units.Curvature crvature = new Oasys.Units.Curvature();

            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                GH_UnitNumber inStrain;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    inStrain = (GH_UnitNumber)gh_typ.Value;
                    if (!inStrain.Value.QuantityInfo.UnitType.Equals(typeof(Oasys.Units.CurvatureUnit)))
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].Name + " input, index " + inputid + ": Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + inStrain.Value.QuantityInfo.Name + " but must be Curvature");
                        return Oasys.Units.Curvature.Zero;
                    }
                    crvature = (Oasys.Units.Curvature)inStrain.Value.ToUnit(curvatureUnit);
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    inStrain = new GH_UnitNumber(new Oasys.Units.Curvature(val, curvatureUnit));
                    crvature = (Oasys.Units.Curvature)inStrain.Value;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to a UnitNumber of Curvature");
                    return Oasys.Units.Curvature.Zero;
                }
                return crvature;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return Oasys.Units.Curvature.Zero;
        }
        internal static Force Force(GH_Component owner, IGH_DataAccess DA, int inputid, UnitsNet.Units.ForceUnit forceUnit, bool isOptional = false)
        {
            UnitsNet.Force force = new UnitsNet.Force();

            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                GH_UnitNumber inForce;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    inForce = (GH_UnitNumber)gh_typ.Value;
                    if (!inForce.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.ForceUnit)))
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].Name + " input, index " + inputid + ": Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + inForce.Value.QuantityInfo.Name + " but must be Force");
                        return UnitsNet.Force.Zero;
                    }
                    force = (UnitsNet.Force)inForce.Value.ToUnit(forceUnit);
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    inForce = new GH_UnitNumber(new UnitsNet.Force(val, forceUnit));
                    force = (UnitsNet.Force)inForce.Value;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to a UnitNumber of Force");
                    return UnitsNet.Force.Zero;
                }
                return force;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return UnitsNet.Force.Zero;
        }
        internal static Moment Moment(GH_Component owner, IGH_DataAccess DA, int inputid, MomentUnit momentUnit, bool isOptional = false)
        {
            Oasys.Units.Moment moment = new Oasys.Units.Moment();

            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                GH_UnitNumber inMoment;

                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    inMoment = (GH_UnitNumber)gh_typ.Value;
                    if (!inMoment.Value.QuantityInfo.UnitType.Equals(typeof(Oasys.Units.MomentUnit)))
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].Name + " input, index " + inputid + ": Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + inMoment.Value.QuantityInfo.Name + " but must be Moment");
                        return Oasys.Units.Moment.Zero;
                    }
                    moment = (Oasys.Units.Moment)inMoment.Value.ToUnit(momentUnit);
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    inMoment = new GH_UnitNumber(new Oasys.Units.Moment(val, momentUnit));
                    moment = (Oasys.Units.Moment)inMoment.Value;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to a UnitNumber of Moment");
                    return Oasys.Units.Moment.Zero;
                }
                return moment;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return Oasys.Units.Moment.Zero;
        }
        internal static IConcreteCrackCalculationParameters ConcreteCrackCalculationParameters(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            IConcreteCrackCalculationParameters concreteCrack = null;

            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                if (gh_typ.Value is IConcreteCrackCalculationParameters)
                {
                    concreteCrack = (IConcreteCrackCalculationParameters)gh_typ.Value;
                }
                else if (gh_typ.Value is AdSecConcreteCrackCalculationParametersGoo)
                {
                    AdSecConcreteCrackCalculationParametersGoo adsecccp = (AdSecConcreteCrackCalculationParametersGoo)gh_typ.Value;
                    concreteCrack = adsecccp.ConcreteCrackCalculationParameters;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to ConcreteCrackCalculationParameters");
                    return null;
                }
                return concreteCrack;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static AdSecDesignCode AdSecDesignCode(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecDesignCode designCode = new AdSecDesignCode();
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                if (gh_typ.Value is AdSecDesignCodeGoo)
                {
                    gh_typ.CastTo(ref designCode);
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to a DesignCode");
                    return null;
                }
                return designCode;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static AdSecMaterial AdSecMaterial(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecMaterial material = new AdSecMaterial();
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                if (gh_typ.Value is AdSecMaterialGoo)
                {
                    gh_typ.CastTo(ref material);
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to an AdSec Material");
                }
                return material.Duplicate();
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static AdSecRebarBundleGoo AdSecRebarBundleGoo(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecRebarBundleGoo rebar = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                if (gh_typ.Value is AdSecRebarBundleGoo)
                {
                    rebar = (AdSecRebarBundleGoo)gh_typ.Value;
                }
                else if (gh_typ.Value is AdSecRebarLayerGoo)
                {
                    AdSecRebarLayerGoo spacing = (AdSecRebarLayerGoo)gh_typ.Value;
                    rebar = new AdSecRebarBundleGoo(spacing.Value.BarBundle);
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Converted " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") from RebarSpacing to an AdSec Rebar. All spacing information has been lost!");
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to an AdSec Rebar");
                }
                return rebar;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static IBarBundle IBarBundle(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            return AdSecRebarBundleGoo(owner, DA, inputid, isOptional).Value;
        }
        internal static AdSecRebarLayerGoo AdSecRebarLayerGoo(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecRebarLayerGoo spacing = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                if (gh_typ.Value is AdSecRebarLayerGoo)
                {
                    spacing = (AdSecRebarLayerGoo)gh_typ.Value;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to an AdSec RebarSpacing");
                }
                return spacing;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static ILayer ILayer(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            return AdSecRebarLayerGoo(owner, DA, inputid, isOptional).Value;
        }
        internal static IGroup ReinforcementGroup(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecRebarGroupGoo group = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                if (gh_typ.Value is AdSecRebarGroupGoo)
                {
                    group = (AdSecRebarGroupGoo)gh_typ.Value;
                    return group.Value;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to an AdSec RebarLayout");
                    return null;
                }
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static Oasys.Collections.IList<IGroup> ReinforcementGroups(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            Oasys.Collections.IList<IGroup> subs = Oasys.Collections.IList<IGroup>.Create();
            List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
            if (DA.GetDataList(inputid, gh_typs))
            {
                for (int i = 0; i < gh_typs.Count; i++)
                {
                    if (gh_typs[i].Value is IGroup)
                    {
                        subs.Add((IGroup)gh_typs[i].Value);
                    }
                    else if (gh_typs[i].Value is AdSecRebarGroupGoo)
                    {
                        AdSecRebarGroupGoo bargroup = (AdSecRebarGroupGoo)gh_typs[i].Value;
                        subs.Add(bargroup.Value);
                    }
                    else
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + "), index in input list: " + i + ", to a SubComponent or a Section");
                        return null;
                    }
                }
                return subs;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }

        internal static AdSecPointGoo AdSecPointGoo(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecPointGoo pt = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                Point3d ghpt = new Point3d();
                if (gh_typ.Value is AdSecPointGoo)
                {
                    gh_typ.CastTo(ref pt);
                }
                else if (GH_Convert.ToPoint3d(gh_typ.Value, ref ghpt, GH_Conversion.Both))
                {
                    pt = new AdSecPointGoo(ghpt);
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to an Vertex Point");
                }

                return pt;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;

        }
        internal static IPoint IPoint(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            AdSecPointGoo pt = AdSecPointGoo(owner, DA, inputid, isOptional);
            if (pt == null) { return null; }
            return pt.AdSecPoint;
        }
        internal static Oasys.Collections.IList<IPoint> IPoints(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            Oasys.Collections.IList<IPoint> pts = Oasys.Collections.IList<IPoint>.Create();
            List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
            if (DA.GetDataList(inputid, gh_typs))
            {
                for (int i = 0; i < gh_typs.Count; i++)
                {
                    Curve polycurve = null;
                    Point3d ghpt = new Point3d();
                    if (gh_typs[i].Value is IPoint)
                    {
                        pts.Add((IPoint)gh_typs[i].Value);
                    }
                    else if (gh_typs[i].Value is AdSecPointGoo)
                    {
                        AdSecPointGoo sspt = (AdSecPointGoo)gh_typs[i].Value;
                        pts.Add(sspt.AdSecPoint);
                    }
                    else if (GH_Convert.ToPoint3d(gh_typs[i].Value, ref ghpt, GH_Conversion.Both))
                    {
                        pts.Add(GhAdSec.Parameters.AdSecPointGoo.CreateFromPoint3d(ghpt));
                    }
                    else if (GH_Convert.ToCurve(gh_typs[i].Value, ref polycurve, GH_Conversion.Both))
                    {
                        PolylineCurve curve = (PolylineCurve)polycurve;
                        pts = GhAdSec.Parameters.AdSecPointGoo.PtsFromPolylineCurve(curve);
                    }
                    else
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + "), index in input list: " + i + ", to a StressStrainPoint or a Polyline");
                        return null;
                    }
                }
                return pts;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static Angle Angle(GH_Component owner, IGH_DataAccess DA, int inputid, UnitsNet.Units.AngleUnit angleUnit, bool isOptional = false)
        {
            GH_UnitNumber a1 = new GH_UnitNumber(new UnitsNet.Angle(0, angleUnit));
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is GH_UnitNumber)
                {
                    a1 = (GH_UnitNumber)gh_typ.Value;
                    // check that unit is of right type
                    if (!a1.Value.QuantityInfo.UnitType.Equals(typeof(UnitsNet.Units.AngleUnit)))
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].Name + " input, index " + inputid + ": Wrong unit type supplied"
                            + System.Environment.NewLine + "Unit type is " + a1.Value.QuantityInfo.Name + " but must be Angle");
                        return UnitsNet.Angle.Zero;
                    }
                }
                // try cast to double
                else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
                {
                    // create new quantity from default units
                    a1 = new GH_UnitNumber(new UnitsNet.Angle(val, angleUnit));
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to an Angle");
                    return UnitsNet.Angle.Zero;
                }
                return (UnitsNet.Angle)a1.Value;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return UnitsNet.Angle.Zero;
        }
        internal static IPerimeterProfile Boundaries(GH_Component owner, IGH_DataAccess DA, int inputid_Boundary, int inputid_Voids, UnitsNet.Units.LengthUnit lengthUnit, bool isOptional = false)
        {
            AdSecProfileGoo perimeter = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid_Boundary, ref gh_typ))
            {
                Brep brep = null;
                Curve crv = null;
                Polyline solid = null;
                if (GH_Convert.ToBrep(gh_typ.Value, ref brep, GH_Conversion.Both))
                {
                    perimeter = new AdSecProfileGoo(brep, lengthUnit);
                }
                else if (GH_Convert.ToCurve(gh_typ.Value, ref crv, GH_Conversion.Both))
                {
                    if (crv.TryGetPolyline(out solid))
                    {
                        perimeter = new AdSecProfileGoo(solid, lengthUnit);
                    }
                    else
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert Curve in " + owner.Params.Input[inputid_Boundary].Name + " input (index " + inputid_Boundary + ") to Polyline");
                        return null;
                    }

                    if (inputid_Voids >= 0)
                    {
                        // try get voids
                        List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
                        if (DA.GetDataList(inputid_Voids, gh_typs))
                        {
                            List<Polyline> voids = new List<Polyline>();

                            for (int i = 0; i < gh_typs.Count; i++)
                            {
                                if (GH_Convert.ToCurve(gh_typs[i].Value, ref crv, GH_Conversion.Both))
                                {
                                    Polyline voidCrv = null;
                                    if (crv.TryGetPolyline(out voidCrv))
                                    {
                                        voids.Add(voidCrv);
                                    }
                                    else
                                    {
                                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input " + owner.Params.Input[inputid_Voids].Name + " input (index " + inputid_Voids + "), index in input list: " + i + ", to Polyline");
                                        return null;
                                    }
                                }
                                else
                                {
                                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input " + owner.Params.Input[inputid_Voids].Name + " input (index " + inputid_Voids + "), index in input list: " + i + ", to Polyline");
                                    return null;
                                }
                            }

                            perimeter = new AdSecProfileGoo(solid, voids, lengthUnit);
                        }
                        else if (owner.Params.Input[inputid_Voids].SourceCount > 0)
                        {
                            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input " + owner.Params.Input[inputid_Voids].Name + " input (index " + inputid_Voids + ") to Polyline(s)");
                            return null;
                        }
                    }
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid_Boundary].Name + " input (index " + inputid_Boundary + ") to Boundary");
                    return null;
                }
                return (IPerimeterProfile)perimeter.Profile;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid_Boundary].Name + " input, index " + inputid_Boundary + " - Input required");
            }
            return null;
        }
        internal static IProfile Profile(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            IProfile prfl = null;
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecProfileGoo)
                {
                    AdSecProfileGoo a1 = (AdSecProfileGoo)gh_typ.Value;
                    prfl = a1.Profile;
                }
                else
                {
                    prfl = Boundaries(owner, DA, inputid, -1, GhAdSec.DocumentUnits.LengthUnit);
                }
                
                return prfl;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static AdSecSection Section(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
            if (DA.GetData(inputid, ref gh_typ))
            {
                // try cast directly to quantity type
                if (gh_typ.Value is AdSecSectionGoo)
                {
                    AdSecSectionGoo a1 = (AdSecSectionGoo)gh_typ.Value;
                    return a1.Value;
                }
                else
                {
                    owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + ") to an Section");
                    return null;
                }
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
        internal static Oasys.Collections.IList<ISubComponent> SubComponents(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
        {
            Oasys.Collections.IList<ISubComponent> subs = Oasys.Collections.IList<ISubComponent>.Create();
            List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
            if (DA.GetDataList(inputid, gh_typs))
            {
                for (int i = 0; i < gh_typs.Count; i++)
                {
                    if (gh_typs[i].Value is ISubComponent)
                    {
                        subs.Add((ISubComponent)gh_typs[i].Value);
                    }
                    else if (gh_typs[i].Value is AdSecSubComponentGoo)
                    {
                        AdSecSubComponentGoo subcomp = (AdSecSubComponentGoo)gh_typs[i].Value;
                        subs.Add(subcomp.Value);
                    }
                    else if (gh_typs[i].Value is AdSecSectionGoo)
                    {
                        AdSecSectionGoo section = (AdSecSectionGoo)gh_typs[i].Value;
                        IPoint offset = Oasys.Profiles.IPoint.Create(UnitsNet.Length.Zero, UnitsNet.Length.Zero);
                        ISubComponent sub = ISubComponent.Create(section.Value.Section, offset);
                        subs.Add(sub);
                    }
                    else
                    {
                        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].Name + " input (index " + inputid + "), index in input list: " + i + ", to a SubComponent or a Section");
                        return null;
                    }
                }
                return subs;
            }
            else if (!isOptional)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error with " + owner.Params.Input[inputid].Name + " input, index " + inputid + " - Input required");
            }
            return null;
        }
    }
}
