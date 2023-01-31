using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;
using Rhino.Geometry;

namespace AdSecGH.Parameters
{
  /// <summary>
  /// AdSec Material class, this class defines the basic properties and methods for any AdSec Material
  public class AdSecMaterial
  {
    internal enum AdSecMaterialType
    {
      Concrete,
      Rebar,
      Tendon,
      Steel,
      FRP,
    }

    public IMaterial Material { get; set; }
    public AdSecDesignCode DesignCode { get; set; }
    public string GradeName { get; set; }
    public string TypeName
    {
      get
      {
        return this.Type.ToString();
      }
    }
    public string DesignCodeName
    {
      get
      {
        if (this.DesignCode == null)
          return null;
        return this.DesignCode.DesignCodeName;
      }
    }
    public bool IsValid
    {
      get
      {
        if (this.Material == null)
          return false;
        return true;
      }
    }
    internal AdSecMaterialType Type { get; set; }

    #region constructors
    public AdSecMaterial()
    {
    }

    public AdSecMaterial(IMaterial material, string materialGradeName)
    {
      if (materialGradeName != null)
        this.GradeName = materialGradeName;

      // StressStrain ULS Compression
      Tuple<Curve, List<Point3d>> ulsComp = AdSecStressStrainCurveGoo.CreateFromCode(material.Strength.Compression, true);
      AdSecStressStrainCurveGoo ulsCompCrv = new AdSecStressStrainCurveGoo(ulsComp.Item1, material.Strength.Compression,
          AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, ulsComp.Item2);
      // StressStrain ULS Tension
      Tuple<Curve, List<Point3d>> ulsTens = AdSecStressStrainCurveGoo.CreateFromCode(material.Strength.Tension, false);
      AdSecStressStrainCurveGoo ulsTensCrv = new AdSecStressStrainCurveGoo(ulsTens.Item1, material.Strength.Tension,
          AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, ulsTens.Item2);
      // StressStrain SLS Compression
      Tuple<Curve, List<Point3d>> slsComp = AdSecStressStrainCurveGoo.CreateFromCode(material.Serviceability.Compression, true);
      AdSecStressStrainCurveGoo slsCompCrv = new AdSecStressStrainCurveGoo(slsComp.Item1, material.Serviceability.Compression,
          AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, slsComp.Item2);
      // StressStrain SLS Tension
      Tuple<Curve, List<Point3d>> slsTens = AdSecStressStrainCurveGoo.CreateFromCode(material.Serviceability.Tension, false);
      AdSecStressStrainCurveGoo slsTensCrv = new AdSecStressStrainCurveGoo(slsTens.Item1, material.Serviceability.Tension,
          AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, slsTens.Item2);
      // combine compression and tension
      ITensionCompressionCurve ulsTC = ITensionCompressionCurve.Create(ulsTensCrv.StressStrainCurve, ulsCompCrv.StressStrainCurve);
      ITensionCompressionCurve slsTC = ITensionCompressionCurve.Create(slsTensCrv.StressStrainCurve, slsCompCrv.StressStrainCurve);

      try
      {
        // try cast to concrete material
        IConcrete concrete = (IConcrete)material;
        if (concrete.ConcreteCrackCalculationParameters == null)
          this.Material = IConcrete.Create(ulsTC, slsTC);
        else
          this.Material = IConcrete.Create(ulsTC, slsTC, concrete.ConcreteCrackCalculationParameters);
        this.Type = AdSecMaterialType.Concrete;
      }
      catch (Exception)
      {
        try
        {
          // try cast to steel material
          ISteel steel = (ISteel)material;
          this.Material = ISteel.Create(ulsTC, slsTC);
          this.Type = AdSecMaterialType.Steel;
        }
        catch (Exception)
        {
          try
          {
            // try cast to rebar material
            IReinforcement reinforcement = (IReinforcement)material;
            this.Material = IReinforcement.Create(ulsTC, slsTC);
            this.Type = AdSecMaterialType.Rebar;
          }
          catch (Exception)
          {
            try
            {
              // try cast to frp material
              IFrp frp = (IFrp)material;
              this.Material = IFrp.Create(ulsTC, slsTC);
              this.Type = AdSecMaterialType.FRP;
            }
            catch (Exception)
            {

              throw new Exception("unable to cast to known material type");
            }
          }
        }
      }
    }

    internal AdSecMaterial(FieldInfo fieldGrade)
    {
      // convert reflected interface to member
      this.Material = (IMaterial)fieldGrade.GetValue(null);
      // get the name of the grade
      this.GradeName = fieldGrade.Name;

      // Get material type
      string designCodeReflectedLevels = fieldGrade.DeclaringType.FullName.Replace("Oasys.AdSec.StandardMaterials.", "");
      List<string> designCodeLevelsSplit = designCodeReflectedLevels.Split('+').ToList();

      // set material type
      if (designCodeLevelsSplit[0].StartsWith("Reinforcement"))
      {
        if (designCodeLevelsSplit[1].StartsWith("Steel"))
          this.Type = AdSecMaterialType.Rebar;
        else
          this.Type = AdSecMaterialType.Tendon;
        designCodeLevelsSplit.RemoveRange(0, 2);
      }
      else
      {
        AdSecMaterialType type;
        Enum.TryParse(designCodeLevelsSplit[0], out type);
        this.Type = type;
        designCodeLevelsSplit.RemoveRange(0, 1);
      }

      // set designcode
      this.DesignCode = new AdSecDesignCode(designCodeLevelsSplit);
    }
    #endregion

    #region methods

    public AdSecMaterial Duplicate()
    {
      if (this == null)
        return null;
      AdSecMaterial dup = (AdSecMaterial)this.MemberwiseClone();
      return dup;
    }

    public override string ToString()
    {
      string grd = "Custom ";
      if (GradeName != null)
        grd = GradeName.Replace("  ", " ") + " ";

      string code = "";
      if (DesignCode != null)
        if (DesignCode.DesignCodeName != null)
          code = " to " + DesignCodeName.Replace("  ", " ");

      return grd + TypeName.Replace("  ", " ") + code;
    }
    #endregion
  }
}
