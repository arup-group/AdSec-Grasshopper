using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino;
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

namespace AdSecGH.Parameters
{
  /// <summary>
  /// AdSec Material class, this class defines the basic properties and methods for any AdSec Material
  /// </summary>
  public class AdSecMaterial
  {
    public IMaterial Material
    {
      get { return m_material; }
      set { m_material = value; }
    }
    public string TypeName
    {
      get { return m_type.ToString(); }
    }
    public string GradeName
    {
      get { return m_materialGradeName; }
      set { m_materialGradeName = value; }
    }
    public AdSecDesignCode DesignCode
    {
      get { return m_designCode; }
      set { m_designCode = value; }
    }
    public string DesignCodeName
    {
      get { if (m_designCode == null) { return null; } return m_designCode.DesignCodeName; }
    }

    internal enum AdSecMaterialType
    {
      Concrete,
      Rebar,
      Tendon,
      Steel,
      FRP,
    }
    internal AdSecMaterialType Type
    {
      get { return m_type; }
      set { m_type = value; }
    }

    #region fields
    private AdSecDesignCode m_designCode;
    private IMaterial m_material;
    private AdSecMaterialType m_type;
    private string m_materialGradeName;
    #endregion

    #region constructors
    public AdSecMaterial()
    {
    }
    public AdSecMaterial(IMaterial material, string materialGradeName)
    {
      if (materialGradeName != null)
        m_materialGradeName = materialGradeName;

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
          m_material = IConcrete.Create(ulsTC, slsTC);
        else
          m_material = IConcrete.Create(ulsTC, slsTC, concrete.ConcreteCrackCalculationParameters);
        m_type = AdSecMaterialType.Concrete;
      }
      catch (Exception)
      {
        try
        {
          // try cast to steel material
          ISteel steel = (ISteel)material;
          m_material = ISteel.Create(ulsTC, slsTC);
          m_type = AdSecMaterialType.Steel;
        }
        catch (Exception)
        {
          try
          {
            // try cast to rebar material
            IReinforcement reinforcement = (IReinforcement)material;
            m_material = IReinforcement.Create(ulsTC, slsTC);
            m_type = AdSecMaterialType.Rebar;
          }
          catch (Exception)
          {
            try
            {
              // try cast to frp material
              IFrp frp = (IFrp)material;
              m_material = IFrp.Create(ulsTC, slsTC);
              m_type = AdSecMaterialType.FRP;
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
      m_material = (IMaterial)fieldGrade.GetValue(null);
      // get the name of the grade
      m_materialGradeName = fieldGrade.Name;

      // Get material type
      string designCodeReflectedLevels = fieldGrade.DeclaringType.FullName.Replace("Oasys.AdSec.StandardMaterials.", "");
      List<string> designCodeLevelsSplit = designCodeReflectedLevels.Split('+').ToList();

      // set material type
      if (designCodeLevelsSplit[0].StartsWith("Reinforcement"))
      {
        if (designCodeLevelsSplit[1].StartsWith("Steel"))
          m_type = AdSecMaterialType.Rebar;
        else
          m_type = AdSecMaterialType.Tendon;
        designCodeLevelsSplit.RemoveRange(0, 2);
      }
      else
      {
        Enum.TryParse(designCodeLevelsSplit[0], out m_type);
        designCodeLevelsSplit.RemoveRange(0, 1);
      }

      // set designcode
      m_designCode = new AdSecDesignCode(designCodeLevelsSplit);
    }



    public AdSecMaterial Duplicate()
    {
      if (this == null) { return null; }
      AdSecMaterial dup = (AdSecMaterial)this.MemberwiseClone();
      return dup;
    }
    #endregion

    #region properties
    public bool IsValid
    {
      get
      {
        if (this.Material == null) { return false; }
        return true;
      }
    }
    #endregion

    #region methods
    public override string ToString()
    {
      string grd = "Custom ";
      if (GradeName != null)
        grd = GradeName.Replace("  ", " ") + " ";

      string code = "";
      if (DesignCode != null)
      {
        if (DesignCode.DesignCodeName != null)
          code = " to " + DesignCodeName.Replace("  ", " ");
      }

      return grd + TypeName.Replace("  ", " ") + code;
    }

    #endregion
  }

  /// <summary>
  /// Goo wrapper class, makes sure this can be used in Grasshopper.
  /// </summary>
  public class AdSecMaterialGoo : GH_Goo<AdSecMaterial>
  {
    #region constructors
    public AdSecMaterialGoo()
    {
      this.Value = new AdSecMaterial();
    }
    public AdSecMaterialGoo(AdSecMaterial goo)
    {
      if (goo == null)
        goo = new AdSecMaterial();
      this.Value = goo; //goo.Duplicate(); 
    }

    public override IGH_Goo Duplicate()
    {
      return DuplicateGoo();
    }
    public AdSecMaterialGoo DuplicateGoo()
    {
      return new AdSecMaterialGoo(Value == null ? new AdSecMaterial() : Value.Duplicate());
    }
    #endregion

    #region properties
    public override bool IsValid
    {
      get
      {
        if (Value == null) { return false; }
        return true;
      }
    }
    public override string IsValidWhyNot
    {
      get
      {
        if (Value.IsValid) { return string.Empty; }
        return Value.IsValid.ToString();
      }
    }
    public override string ToString()
    {
      if (Value == null)
        return "Null";
      else
        return "AdSec " + TypeName + " {" + Value.ToString() + "}";
    }
    public override string TypeName => "Material";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";


    #endregion

    #region casting methods
    public override bool CastTo<Q>(ref Q target)
    {
      // This function is called when Grasshopper needs to convert this 
      // instance of GsaBool6 into some other type Q.            


      if (typeof(Q).IsAssignableFrom(typeof(AdSecMaterial)))
      {
        if (Value == null)
          target = default;
        else
          target = (Q)(object)Value.Duplicate();
        return true;
      }

      target = default;
      return false;
    }
    public override bool CastFrom(object source)
    {
      // This function is called when Grasshopper needs to convert other data 
      // into this parameter.


      if (source == null) { return false; }

      //Cast from own type
      if (typeof(AdSecMaterial).IsAssignableFrom(source.GetType()))
      {
        Value = (AdSecMaterial)source;
        return true;
      }

      return false;
    }
    #endregion
  }

  /// <summary>
  /// This class provides a Parameter interface for the Data_GsaBool6 type.
  /// </summary>
  public class AdSecMaterialParameter : GH_PersistentParam<AdSecMaterialGoo>
  {
    public AdSecMaterialParameter()
      : base(new GH_InstanceDescription("Material", "Mat", "AdSec Material Parameter", Components.Ribbon.CategoryName.Name(), Components.Ribbon.SubCategoryName.Cat9()))
    {
    }

    public override Guid ComponentGuid => new Guid("cf5636e2-628d-4794-ab29-97f83002db34");

    public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;

    protected override System.Drawing.Bitmap Icon => Properties.Resources.MaterialParam;

    protected override GH_GetterResult Prompt_Plural(ref List<AdSecMaterialGoo> values)
    {
      return GH_GetterResult.cancel;
    }
    protected override GH_GetterResult Prompt_Singular(ref AdSecMaterialGoo value)
    {
      return GH_GetterResult.cancel;
    }
    protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomSingleValueItem()
    {
      System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
      {
        Text = "Not available",
        Visible = false
      };
      return item;
    }
    protected override System.Windows.Forms.ToolStripMenuItem Menu_CustomMultiValueItem()
    {
      System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem
      {
        Text = "Not available",
        Visible = false
      };
      return item;
    }

    #region preview methods

    public bool Hidden
    {
      get { return true; }
      //set { m_hidden = value; }
    }
    public bool IsPreviewCapable
    {
      get { return false; }
    }
    #endregion
  }
}
