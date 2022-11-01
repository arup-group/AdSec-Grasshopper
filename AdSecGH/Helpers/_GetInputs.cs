using System;
using System.Collections.Generic;
using AdSecGH.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Oasys.AdSec;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Layers;
using Oasys.Profiles;
using OasysGH.Parameters;
using OasysUnits;
using OasysUnits.Units;
using Rhino.Geometry;

namespace AdSecGH.Components
{
  class GetInput
  {
    internal static List<Length> Lengths(GH_Component owner, IGH_DataAccess DA, int inputid, LengthUnit docLengthUnit, bool isOptional = false)
    {
      List<Length> lengths = new List<Length>();
      List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs))
      {
        for (int i = 0; i < gh_typs.Count; i++)
        {
          GH_UnitNumber unitNumber = null;
          // try cast directly to quantity type
          if (gh_typs[i].Value is GH_UnitNumber)
          {
            unitNumber = (GH_UnitNumber)gh_typs[i].Value;
            // check that unit is of right type
            if (!unitNumber.Value.QuantityInfo.UnitType.Equals(typeof(LengthUnit)))
            {
              owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Error in " + owner.Params.Input[inputid].NickName + " (item " + i + ") input: Wrong unit type"
                  + Environment.NewLine + "Unit type is " + unitNumber.Value.QuantityInfo.Name + " but must be Length");
            }
            else
            {
              lengths.Add((Length)unitNumber.Value);
            }
          }
          // try cast to double
          else if (GH_Convert.ToDouble(gh_typs[i].Value, out double val, GH_Conversion.Both))
          {
            // create new quantity from default units
            lengths.Add(new Length(val, docLengthUnit));
          }
          else
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i + ") to UnitNumber");
            return null;
          }
        }
        return lengths;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static List<ICover> Covers(GH_Component owner, IGH_DataAccess DA, int inputid, LengthUnit docLengthUnit, bool isOptional = false)
    {
      List<ICover> covers = new List<ICover>();
      foreach (Length length in Lengths(owner, DA, inputid, docLengthUnit, isOptional))
        covers.Add(ICover.Create(length));
      return covers;
    }
    internal static Length GetLength(GH_Component owner, IGH_DataAccess DA, int inputid, LengthUnit docLengthUnit, bool isOptional = false)
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
          if (!unitNumber.Value.QuantityInfo.UnitType.Equals(typeof(LengthUnit)))
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].NickName + " input: Wrong unit type"
                + Environment.NewLine + "Unit type is " + unitNumber.Value.QuantityInfo.Name + " but must be Length");
            return Length.Zero;
          }
        }
        // try cast to double
        else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
        {
          // create new quantity from default units
          unitNumber = new GH_UnitNumber(new Length(val, docLengthUnit));
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to UnitNumber");
          return Length.Zero;
        }
      }
      else if (!isOptional)
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      else
      {
        if (unitNumber == null)
          return Length.Zero;
      }

      return (Length)unitNumber.Value;
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
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " from Web to Flange");
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to Flange");
          return null;
        }
      }
      else if (!isOptional)
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " from Flange to Web");
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to Web");
          return null;
        }
      }
      else if (!isOptional)
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
          pt1 = AdSecStressStrainPointGoo.CreateFromPoint3d(ghpt);
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to StressStrainPoint");
          return null;
        }
        return pt1;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
            pts.Add(AdSecStressStrainPointGoo.CreateFromPoint3d(ghpt));
          }
          else if (GH_Convert.ToCurve(gh_typs[i].Value, ref polycurve, GH_Conversion.Both))
          {
            PolylineCurve curve = (PolylineCurve)polycurve;
            pts = AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
          }
          else
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + owner.Params.Input[inputid].NickName + " to StressStrainPoint or a Polyline");
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
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
          Oasys.Collections.IList<IStressStrainPoint> pts = AdSecStressStrainCurveGoo.StressStrainPtsFromPolyline(curve);
          IExplicitStressStrainCurve exCrv = IExplicitStressStrainCurve.Create();
          exCrv.Points = pts;
          Tuple<Curve, List<Point3d>> tuple = AdSecStressStrainCurveGoo.Create(exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, compression);
          ssCrv = new AdSecStressStrainCurveGoo(tuple.Item1, exCrv, AdSecStressStrainCurveGoo.StressStrainCurveType.Explicit, tuple.Item2);
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to StressStrainCurve");
          return null;
        }
        return ssCrv;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static Pressure GetStress(GH_Component owner, IGH_DataAccess DA, int inputid, PressureUnit stressUnit, bool isOptional = false)
    {
      Pressure stressFib = new Pressure();

      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        GH_UnitNumber inStress;

        // try cast directly to quantity type
        if (gh_typ.Value is GH_UnitNumber)
        {
          inStress = (GH_UnitNumber)gh_typ.Value;
          if (!inStress.Value.QuantityInfo.UnitType.Equals(typeof(PressureUnit)))
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].NickName + " input: Wrong unit type"
                + Environment.NewLine + "Unit type is " + inStress.Value.QuantityInfo.Name + " but must be Stress (Pressure)");
            return Pressure.Zero;
          }
          stressFib = (Pressure)inStress.Value.ToUnit(stressUnit);
        }
        // try cast to double
        else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
        {
          // create new quantity from default units
          inStress = new GH_UnitNumber(new Pressure(val, stressUnit));
          stressFib = (Pressure)inStress.Value;
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to UnitNumber of Stress");
          return Pressure.Zero;
        }
        return stressFib;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return Pressure.Zero;
    }
    internal static Strain GetStrain(GH_Component owner, IGH_DataAccess DA, int inputid, StrainUnit strainUnit, bool isOptional = false)
    {
      Strain strainFib = new Strain();

      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        GH_UnitNumber inStrain;

        // try cast directly to quantity type
        if (gh_typ.Value is GH_UnitNumber)
        {
          inStrain = (GH_UnitNumber)gh_typ.Value;
          if (!inStrain.Value.QuantityInfo.UnitType.Equals(typeof(StrainUnit)))
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].NickName + " input: Wrong unit type"
                + Environment.NewLine + "Unit type is " + inStrain.Value.QuantityInfo.Name + " but must be Strain");
            return Strain.Zero;
          }
          strainFib = (Strain)inStrain.Value.ToUnit(strainUnit);
        }
        // try cast to double
        else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
        {
          // create new quantity from default units
          inStrain = new GH_UnitNumber(new Strain(val, strainUnit));
          strainFib = (Strain)inStrain.Value;
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to UnitNumber of Strain");
          return Strain.Zero;
        }
        return strainFib;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return Strain.Zero;
    }
    internal static Curvature GetCurvature(GH_Component owner, IGH_DataAccess DA, int inputid, CurvatureUnit curvatureUnit, bool isOptional = false)
    {
      Curvature crvature = new Curvature();

      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        GH_UnitNumber inStrain;

        // try cast directly to quantity type
        if (gh_typ.Value is GH_UnitNumber)
        {
          inStrain = (GH_UnitNumber)gh_typ.Value;
          if (!inStrain.Value.QuantityInfo.UnitType.Equals(typeof(CurvatureUnit)))
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].NickName + " input: Wrong unit type"
                + Environment.NewLine + "Unit type is " + inStrain.Value.QuantityInfo.Name + " but must be Curvature");
            return Curvature.Zero;
          }
          crvature = (Curvature)inStrain.Value.ToUnit(curvatureUnit);
        }
        // try cast to double
        else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
        {
          // create new quantity from default units
          inStrain = new GH_UnitNumber(new Curvature(val, curvatureUnit));
          crvature = (Curvature)inStrain.Value;
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to UnitNumber of Curvature");
          return Curvature.Zero;
        }
        return crvature;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return Curvature.Zero;
    }
    internal static Force GetForce(GH_Component owner, IGH_DataAccess DA, int inputid, ForceUnit forceUnit, bool isOptional = false)
    {
      Force force = new Force();

      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        GH_UnitNumber inForce;

        // try cast directly to quantity type
        if (gh_typ.Value is GH_UnitNumber)
        {
          inForce = (GH_UnitNumber)gh_typ.Value;
          if (!inForce.Value.QuantityInfo.UnitType.Equals(typeof(ForceUnit)))
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].NickName + " input: Wrong unit type"
                + Environment.NewLine + "Unit type is " + inForce.Value.QuantityInfo.Name + " but must be Force");
            return Force.Zero;
          }
          force = (Force)inForce.Value.ToUnit(forceUnit);
        }
        // try cast to double
        else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
        {
          // create new quantity from default units
          inForce = new GH_UnitNumber(new Force(val, forceUnit));
          force = (Force)inForce.Value;
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to UnitNumber of Force");
          return Force.Zero;
        }
        return force;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return Force.Zero;
    }
    internal static Moment GetMoment(GH_Component owner, IGH_DataAccess DA, int inputid, MomentUnit momentUnit, bool isOptional = false)
    {
      Moment moment = new Moment();

      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        GH_UnitNumber inMoment;

        // try cast directly to quantity type
        if (gh_typ.Value is GH_UnitNumber)
        {
          inMoment = (GH_UnitNumber)gh_typ.Value;
          if (!inMoment.Value.QuantityInfo.UnitType.Equals(typeof(MomentUnit)))
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].NickName + " input: Wrong unit type"
                + Environment.NewLine + "Unit type is " + inMoment.Value.QuantityInfo.Name + " but must be Moment");
            return Moment.Zero;
          }
          moment = (Moment)inMoment.Value.ToUnit(momentUnit);
        }
        // try cast to double
        else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
        {
          // create new quantity from default units
          inMoment = new GH_UnitNumber(new Moment(val, momentUnit));
          moment = (Moment)inMoment.Value;
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to UnitNumber of Moment");
          return Moment.Zero;
        }
        return moment;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return Moment.Zero;
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
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to ConcreteCrackCalculationParameters");
          return null;
        }
        return concreteCrack;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to DesignCode");
          return null;
        }
        return designCode;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to an AdSec Material");
        }
        return material;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Converted " + owner.Params.Input[inputid].NickName + " from RebarSpacing to an AdSec Rebar. All spacing information has been lost!");
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to Rebar");
        }
        return rebar;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to RebarSpacing");
        }
        return spacing;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static ILayer ILayer(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    {
      return AdSecRebarLayerGoo(owner, DA, inputid, isOptional).Value;
    }
    internal static Oasys.Collections.IList<ILayer> ILayers(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    {
      Oasys.Collections.IList<ILayer> grps = Oasys.Collections.IList<ILayer>.Create();
      List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs))
      {
        for (int i = 0; i < gh_typs.Count; i++)
        {
          if (gh_typs[i].Value is ILayer)
          {
            grps.Add((ILayer)gh_typs[i].Value);
          }
          else if (gh_typs[i].Value is AdSecRebarLayerGoo)
          {
            AdSecRebarLayerGoo rebarGoo = (AdSecRebarLayerGoo)gh_typs[i].Value;
            grps.Add(rebarGoo.Value);
          }
          else
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i + ") to RebarLayer");
          }
        }
        return grps;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static AdSecRebarGroupGoo ReinforcementGroup(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    {
      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        if (gh_typ.Value is AdSecRebarGroupGoo)
        {
          return (AdSecRebarGroupGoo)gh_typ.Value;
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to RebarLayout");
          return null;
        }
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static List<AdSecRebarGroup> ReinforcementGroups(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    {
      List<AdSecRebarGroup> grps = new List<AdSecRebarGroup>();
      List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs))
      {
        for (int i = 0; i < gh_typs.Count; i++)
        {
          if (gh_typs[i].Value is IGroup)
          {
            grps.Add(new AdSecRebarGroup((IGroup)gh_typs[i].Value));
          }
          else if (gh_typs[i].Value is AdSecRebarGroupGoo)
          {
            AdSecRebarGroupGoo rebarGoo = (AdSecRebarGroupGoo)gh_typs[i].Value;
            grps.Add(rebarGoo.Value);
          }
          else
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i + ") to RebarGroup");
          }
        }
        return grps;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to an Vertex Point");
        }

        return pt;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      else if (isOptional)
      {
        return new AdSecPointGoo(Oasys.Profiles.IPoint.Create(
        new Length(0, Units.LengthUnit),
        new Length(0, Units.LengthUnit)));
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
        List<Point3d> tempPts = new List<Point3d>();
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
            tempPts.Add(ghpt);
          }
          else if (GH_Convert.ToCurve(gh_typs[i].Value, ref polycurve, GH_Conversion.Both))
          {
            PolylineCurve curve = (PolylineCurve)polycurve;
            pts = Parameters.AdSecPointGoo.PtsFromPolylineCurve(curve);
          }
          else
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i + ") to StressStrainPoint or Polyline");
          }
        }
        if (tempPts.Count > 0)
        {
          if (tempPts.Count == 1)
          {
            pts.Add(Parameters.AdSecPointGoo.CreateFromPoint3d(tempPts[0], Plane.WorldYZ));
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Single Point converted to local point. Assumed that local coordinate system is in a YZ-Plane");
          }
          else
          {
            Plane.FitPlaneToPoints(tempPts, out Plane plane);
            //Polyline pol = new Polyline(tempPts);
            //plane.Origin = pol.CenterPoint();
            foreach (Point3d pt in tempPts)
              pts.Add(Parameters.AdSecPointGoo.CreateFromPoint3d(pt, plane));
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "List of Points have been converted to local points. Assumed that local coordinate system is matching best-fit plane through points");
          }

        }

        return pts;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static Angle GetAngle(GH_Component owner, IGH_DataAccess DA, int inputid, AngleUnit angleUnit, bool isOptional = false)
    {
      GH_UnitNumber a1 = new GH_UnitNumber(new Angle(0, angleUnit));
      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        // try cast directly to quantity type
        if (gh_typ.Value is GH_UnitNumber)
        {
          a1 = (GH_UnitNumber)gh_typ.Value;
          // check that unit is of right type
          if (!a1.Value.QuantityInfo.UnitType.Equals(typeof(AngleUnit)))
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error in " + owner.Params.Input[inputid].NickName + " input: Wrong unit type"
                + Environment.NewLine + "Unit type is " + a1.Value.QuantityInfo.Name + " but must be Angle");
            return Angle.Zero;
          }
        }
        // try cast to double
        else if (GH_Convert.ToDouble(gh_typ.Value, out double val, GH_Conversion.Both))
        {
          // create new quantity from default units
          a1 = new GH_UnitNumber(new Angle(val, angleUnit));
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to Angle");
          return Angle.Zero;
        }
        return (Angle)a1.Value;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return Angle.Zero;
    }
    internal static AdSecProfileGoo Boundaries(GH_Component owner, IGH_DataAccess DA, int inputid_Boundary, int inputid_Voids, LengthUnit lengthUnit, bool isOptional = false)
    {
      owner.ClearRuntimeMessages();
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
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input " + owner.Params.Input[inputid_Boundary].NickName + " to Polyline");
            return null;
          }
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid_Boundary].NickName + " to Boundary");
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
                  owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert input " + owner.Params.Input[inputid_Voids].NickName + " (item " + i + ") to Polyline");
                }
              }
              else
              {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert input " + owner.Params.Input[inputid_Voids].NickName + " (item " + i + ") to Polyline");
              }
            }

            perimeter = new AdSecProfileGoo(solid, voids, lengthUnit);
          }
          else if (owner.Params.Input[inputid_Voids].SourceCount > 0)
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert input " + owner.Params.Input[inputid_Voids].NickName + " input to Polyline(s)");
            return null;
          }
        }
        return (AdSecProfileGoo)perimeter;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid_Boundary].NickName + " failed to collect data!");
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
          prfl = Boundaries(owner, DA, inputid, -1, Units.LengthUnit).Profile;
        }

        return prfl;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static AdSecProfileGoo AdSecProfileGoo(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    {
      AdSecProfileGoo prfl = null;
      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        // try cast directly to quantity type
        if (gh_typ.Value is AdSecProfileGoo)
        {
          prfl = (AdSecProfileGoo)gh_typ.Value;
        }
        else
        {
          prfl = Boundaries(owner, DA, inputid, -1, Units.LengthUnit);
        }
        return prfl;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static AdSecSection AdSecSection(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
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
        else if (gh_typ.Value is AdSecSubComponentGoo)
        {
          AdSecSubComponentGoo a2 = (AdSecSubComponentGoo)gh_typ.Value;
          return a2.section;
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to Section");
          return null;
        }
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static List<AdSecSection> AdSecSections(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    {
      List<AdSecSection> subs = new List<AdSecSection>();
      List<GH_ObjectWrapper> gh_typs = new List<GH_ObjectWrapper>();
      if (DA.GetDataList(inputid, gh_typs))
      {
        for (int i = 0; i < gh_typs.Count; i++)
        {
          if (gh_typs[i].Value is AdSecSection)
          {
            subs.Add((AdSecSection)gh_typs[i].Value);
          }
          else if (gh_typs[i].Value is AdSecSectionGoo)
          {
            AdSecSectionGoo subcomp = (AdSecSectionGoo)gh_typs[i].Value;
            subs.Add(subcomp.Value);
          }
          else
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i + ") to Section");
          }
        }
        return subs;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
    internal static AdSecSolutionGoo Solution(GH_Component owner, IGH_DataAccess DA, int inputid, bool isOptional = false)
    {
      GH_ObjectWrapper gh_typ = new GH_ObjectWrapper();
      if (DA.GetData(inputid, ref gh_typ))
      {
        // try cast directly to quantity type
        if (gh_typ.Value is AdSecSolutionGoo)
        {
          return (AdSecSolutionGoo)gh_typ.Value;
        }
        else
        {
          owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to convert " + owner.Params.Input[inputid].NickName + " to AdSec Results");
          return null;
        }
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
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
            IPoint offset = Oasys.Profiles.IPoint.Create(Length.Zero, Length.Zero);
            ISubComponent sub = ISubComponent.Create(section.Value.Section, offset);
            subs.Add(sub);
          }
          else
          {
            owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Unable to convert " + owner.Params.Input[inputid].NickName + " (item " + i + ") to SubComponent or Section");
          }
        }
        return subs;
      }
      else if (!isOptional)
      {
        owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter " + owner.Params.Input[inputid].NickName + " failed to collect data!");
      }
      return null;
    }
  }
}
