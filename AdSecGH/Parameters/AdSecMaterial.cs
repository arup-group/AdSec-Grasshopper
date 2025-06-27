using System;
using System.Linq;
using System.Reflection;

using AdSecCore.Functions;

using Oasys.AdSec.Materials;
using Oasys.AdSec.Materials.StressStrainCurves;



namespace AdSecGH.Parameters {
  /// <summary>
  ///   AdSec Material class, this class defines the basic properties and methods for any AdSec Material
  /// </summary>
  public class AdSecMaterial {
    public AdSecDesignCode DesignCode { get; set; }
    public string DesignCodeName => DesignCode?.DesignCodeName;
    public string GradeName { get; set; }
    public bool IsValid => Material != null && DesignCode.IsValid;
    public IMaterial Material { get; set; }
    public string TypeName => Type.ToString();
    internal MaterialType Type { get; set; }

    public AdSecMaterial() { }

    public AdSecMaterial(MaterialDesign materialDesign) {
      var material = materialDesign.Material;
      var materialGradeName = materialDesign.GradeName;

      DesignCode = new AdSecDesignCode() {
        DesignCode = materialDesign.DesignCode?.IDesignCode,
        DesignCodeName = materialDesign.DesignCode?.DesignCodeName,
      };

      if (!string.IsNullOrEmpty(materialGradeName)) {
        GradeName = materialGradeName;
      }

      var ulsCompressionCurve = AdSecStressStrainCurveGoo.CreateFromCode(material.Strength.Compression, true);
      var ulsTensionCurve = AdSecStressStrainCurveGoo.CreateFromCode(material.Strength.Tension, false);
      var slsComprssionCurve = AdSecStressStrainCurveGoo.CreateFromCode(material.Serviceability.Compression, true);
      var slsTensionCurve = AdSecStressStrainCurveGoo.CreateFromCode(material.Serviceability.Tension, false);
      var ulsTensionCompression
        = ITensionCompressionCurve.Create(ulsTensionCurve.Value.IStressStrainCurve, ulsCompressionCurve.Value.IStressStrainCurve);
      var slsTensionCompression
        = ITensionCompressionCurve.Create(slsTensionCurve.Value.IStressStrainCurve, slsComprssionCurve.Value.IStressStrainCurve);

      //Create methods throws exception when material is not of the correct type, so cannot use if's here
      try {
        var concrete = (IConcrete)material;
        Material = concrete.ConcreteCrackCalculationParameters == null ?
          IConcrete.Create(ulsTensionCompression, slsTensionCompression) :
          (IMaterial)IConcrete.Create(ulsTensionCompression, slsTensionCompression,
            concrete.ConcreteCrackCalculationParameters);

        Type = MaterialType.Concrete;
      } catch (Exception) {
        try {
          Material = ISteel.Create(ulsTensionCompression, slsTensionCompression);
          Type = MaterialType.Steel;
        } catch (Exception) {
          try {
            Material = IReinforcement.Create(ulsTensionCompression, slsTensionCompression);
            Type = MaterialType.Rebar;
          } catch (Exception) {
            try {
              Material = IFrp.Create(ulsTensionCompression, slsTensionCompression);
              Type = MaterialType.FRP;
            } catch (Exception) {
              throw new InvalidCastException("Unable to cast to known material type");
            }
          }
        }
      }
    }

    internal AdSecMaterial(FieldInfo fieldGrade) {
      Material = (IMaterial)fieldGrade.GetValue(null);
      GradeName = fieldGrade.Name;

      string designCodeReflectedLevels
        = fieldGrade.DeclaringType?.FullName?.Replace("Oasys.AdSec.StandardMaterials.", "");
      var designCodeLevelsSplit = designCodeReflectedLevels?.Split('+').ToList();
      if (designCodeLevelsSplit == null || !designCodeLevelsSplit.Any()) {
        const string message = "Cannot find specified material in standard material list";
        throw new ArgumentOutOfRangeException(nameof(fieldGrade), message);
      }

      if (designCodeLevelsSplit[0].StartsWith("Reinforcement")) {
        Type = designCodeLevelsSplit[1].StartsWith("Steel") ? MaterialType.Rebar : MaterialType.Tendon;
        designCodeLevelsSplit.RemoveRange(0, 2);
      } else {
        Enum.TryParse(designCodeLevelsSplit[0], out MaterialType type);
        Type = type;
        designCodeLevelsSplit.RemoveRange(0, 1);
      }

      DesignCode = new AdSecDesignCode(designCodeLevelsSplit);
    }

    public AdSecMaterial Duplicate() {
      return IsValid ? (AdSecMaterial)MemberwiseClone() : null;
    }

    public override string ToString() {
      string grade = "Custom ";
      if (GradeName != null) {
        grade = $"{GradeName.Replace("  ", " ")} ";
      }

      string code = "";
      if (DesignCode?.DesignCodeName != null) {
        code = $" to {DesignCodeName.Replace("  ", " ")}";
      }

      var description = grade + TypeName.Replace("  ", " ") + code;
      description = description.Replace("+", " ");
      return $"AdSec Material ({description})";
    }
  }
}
