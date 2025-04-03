using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;

using Rhino.Geometry;

namespace AdSecGH.Parameters {
  /// <summary>
  /// AdSec Material class, this class defines the basic properties and methods for any AdSec Material
  public class AdSecMaterial {
    internal enum AdSecMaterialType {
      Concrete,
      Rebar,
      Tendon,
      Steel,
      FRP,
    }

    public AdSecDesignCode DesignCode { get; set; }
    public string DesignCodeName {
      get {
        if (DesignCode == null) {
          return null;
        }

        return DesignCode.DesignCodeName;
      }
    }
    public string GradeName { get; set; }
    public bool IsValid {
      get {
        if (Material == null) {
          return false;
        }
        return true;
      }
    }
    public IMaterial Material { get; set; }
    public string TypeName => Type.ToString();
    internal AdSecMaterialType Type { get; set; }

    public AdSecMaterial() {
    }

    public AdSecMaterial(IMaterial material, string materialGradeName) {
      if (materialGradeName != null) {
        GradeName = materialGradeName;
      }

      // StressStrain ULS Compression
      Tuple<Curve, List<Point3d>> ulsComp = AdSecStressStrainCurveGoo.CreateFromCode(material.Strength.Compression, true);
      var ulsCompCrv = new AdSecStressStrainCurveGoo(ulsComp.Item1, material.Strength.Compression,
          AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, ulsComp.Item2);
      // StressStrain ULS Tension
      Tuple<Curve, List<Point3d>> ulsTens = AdSecStressStrainCurveGoo.CreateFromCode(material.Strength.Tension, false);
      var ulsTensCrv = new AdSecStressStrainCurveGoo(ulsTens.Item1, material.Strength.Tension,
          AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, ulsTens.Item2);
      // StressStrain SLS Compression
      Tuple<Curve, List<Point3d>> slsComp = AdSecStressStrainCurveGoo.CreateFromCode(material.Serviceability.Compression, true);
      var slsCompCrv = new AdSecStressStrainCurveGoo(slsComp.Item1, material.Serviceability.Compression,
          AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, slsComp.Item2);
      // StressStrain SLS Tension
      Tuple<Curve, List<Point3d>> slsTens = AdSecStressStrainCurveGoo.CreateFromCode(material.Serviceability.Tension, false);
      var slsTensCrv = new AdSecStressStrainCurveGoo(slsTens.Item1, material.Serviceability.Tension,
          AdSecStressStrainCurveGoo.StressStrainCurveType.StressStrainDefault, slsTens.Item2);
      // combine compression and tension
      var ulsTC = ITensionCompressionCurve.Create(ulsTensCrv.StressStrainCurve, ulsCompCrv.StressStrainCurve);
      var slsTC = ITensionCompressionCurve.Create(slsTensCrv.StressStrainCurve, slsCompCrv.StressStrainCurve);

      try {
        // try cast to concrete material
        var concrete = (IConcrete)material;
        if (concrete.ConcreteCrackCalculationParameters == null) {
          Material = IConcrete.Create(ulsTC, slsTC);
        } else {
          Material = IConcrete.Create(ulsTC, slsTC, concrete.ConcreteCrackCalculationParameters);
        }
        Type = AdSecMaterialType.Concrete;
      } catch (Exception) {
        try {
          // try cast to steel material
          var steel = (ISteel)material;
          Material = ISteel.Create(ulsTC, slsTC);
          Type = AdSecMaterialType.Steel;
        } catch (Exception) {
          try {
            // try cast to rebar material
            var reinforcement = (IReinforcement)material;
            Material = IReinforcement.Create(ulsTC, slsTC);
            Type = AdSecMaterialType.Rebar;
          } catch (Exception) {
            try {
              // try cast to frp material
              var frp = (IFrp)material;
              Material = IFrp.Create(ulsTC, slsTC);
              Type = AdSecMaterialType.FRP;
            } catch (Exception) {
              throw new Exception("unable to cast to known material type");
            }
          }
        }
      }
    }

    internal AdSecMaterial(FieldInfo fieldGrade) {
      // convert reflected interface to member
      Material = (IMaterial)fieldGrade.GetValue(null);
      // get the name of the grade
      GradeName = fieldGrade.Name;

      // Get material type
      string designCodeReflectedLevels = fieldGrade.DeclaringType.FullName.Replace("Oasys.AdSec.StandardMaterials.", "");
      var designCodeLevelsSplit = designCodeReflectedLevels.Split('+').ToList();

      // set material type
      if (designCodeLevelsSplit[0].StartsWith("Reinforcement")) {
        if (designCodeLevelsSplit[1].StartsWith("Steel")) {
          Type = AdSecMaterialType.Rebar;
        } else {
          Type = AdSecMaterialType.Tendon;
        }
        designCodeLevelsSplit.RemoveRange(0, 2);
      } else {
        Enum.TryParse(designCodeLevelsSplit[0], out AdSecMaterialType type);
        Type = type;
        designCodeLevelsSplit.RemoveRange(0, 1);
      }

      // set designcode
      DesignCode = new AdSecDesignCode(designCodeLevelsSplit);
    }

    public AdSecMaterial Duplicate() {
      if (this == null) {
        return null;
      }

      var dup = (AdSecMaterial)MemberwiseClone();
      return dup;
    }

    public override string ToString() {
      string grd = "Custom ";
      if (GradeName != null) {
        grd = $"{GradeName.Replace("  ", " ")} ";
      }

      string code = "";
      if (DesignCode != null && DesignCode.DesignCodeName != null) {
        code = $" to {DesignCodeName.Replace("  ", " ")}";
      }

      return grd + TypeName.Replace("  ", " ") + code;
    }
  }
}
