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
using OasysUnits;
using OasysUnits.Units;

namespace AdSecGH.Parameters
{
  /// <summary>
  /// AdSec DesignCode class, this class defines the basic properties and methods for any AdSec DesignCode
  /// </summary>
  public class AdSecDesignCode
  {
    public IDesignCode DesignCode
    {
      get { return m_designCode; }
      set { m_designCode = value; }
    }
    public string DesignCodeName
    {
      get { return m_designCodeName; }
      set { m_designCodeName = value; }
    }

    #region fields
    private IDesignCode m_designCode;
    private string m_designCodeName;
    #endregion

    #region constructors
    public AdSecDesignCode()
    {
    }
    public AdSecDesignCode(IDesignCode designCode, string designCodeName)
    {
      m_designCode = designCode;
      m_designCodeName = designCodeName;
    }
    internal AdSecDesignCode(FieldInfo fieldDesignCode)
    {
      string designCodeReflectedLevels = fieldDesignCode.DeclaringType.FullName.Replace("Oasys.AdSec.DesignCode.", "");
      List<string> designCodeLevelsSplit = designCodeReflectedLevels.Split('+').ToList();
      CreateFromReflectedLevels(designCodeLevelsSplit, true);
    }
    internal AdSecDesignCode(List<string> designCodeReflectedLevels)
    {
      CreateFromReflectedLevels(designCodeReflectedLevels);
    }
    private bool CreateFromReflectedLevels(List<string> designCodeReflectedLevels, bool fromDesignCode = false)
    {
      // Get all DesignCodes in DLL under namespace
      Dictionary<string, Type> designCodeKVP = Helpers.ReflectAdSecAPI.ReflectAdSecNamespace("Oasys.AdSec.DesignCode");

      // Loop through DesignCodes types to find the DesignCode type matching our input list of levels
      string designcodeName = "";
      Type typ = null;
      for (int i = 0; i < designCodeReflectedLevels.Count - 1; i++)
      {
        designcodeName = designcodeName + designCodeReflectedLevels[i] + " ";
        designCodeKVP.TryGetValue(designCodeReflectedLevels[i], out typ);
        if (typ == null) { return false; }
        designCodeKVP = Helpers.ReflectAdSecAPI.ReflectNestedTypes(typ);
      }
      if (designCodeReflectedLevels.Count == 1)
      {
        designcodeName = designCodeReflectedLevels[0];
        designCodeKVP.TryGetValue(designCodeReflectedLevels[0], out typ);
        if (typ == null) { return false; }
      }

      // we need to find the right type Interface under Oasys.AdSec.IAdsec in order to cast to IDesignCode
      // the string to search for depends on where we call this function from, if we come from an IMaterial type
      // we can simply use the full name but if from IDesignCode we need to add the name of the code with a +
      string searchFor = (fromDesignCode) ? typ.FullName + "+" + designCodeReflectedLevels.Last() : typ.FullName;

      // loop through all types in Oasys.AdSec.IAdsec and cast to IDesignCode if match with above string
      foreach (var type in Assembly.GetAssembly(typeof(IAdSec)).GetTypes())
      {
        if (type.IsInterface && type.Namespace == "Oasys.AdSec.DesignCode")
        {
          foreach (var field in type.GetFields())
          {
            if (field.DeclaringType.FullName == searchFor)
            {
              m_designCode = (IDesignCode)field.GetValue(null);
            }
          }
        }
      }

      if (m_designCode == null) { return false; }
      m_designCodeName = designcodeName.TrimEnd(' ') + " " + designCodeReflectedLevels.Last();
      return true;
    }

    public AdSecDesignCode Duplicate()
    {
      if (this == null) { return null; }
      AdSecDesignCode dup = (AdSecDesignCode)this.MemberwiseClone();
      return dup;
    }
    #endregion

    #region properties
    public bool IsValid
    {
      get
      {
        if (this.DesignCode == null) { return false; }
        return true;
      }
    }
    #endregion

    #region methods
    public override string ToString()
    {
      return DesignCodeName.Replace("  ", " ");
    }

    #endregion
  }

  /// <summary>
  /// Goo wrapper class, makes sure this can be used in Grasshopper.
  /// </summary>
  public class AdSecDesignCodeGoo : GH_Goo<AdSecDesignCode>
  {
    #region constructors
    public AdSecDesignCodeGoo()
    {
      this.Value = new AdSecDesignCode();
    }
    public AdSecDesignCodeGoo(AdSecDesignCode goo)
    {
      if (goo == null)
        goo = new AdSecDesignCode();
      this.Value = goo; // goo.Duplicate(); 
    }

    public override IGH_Goo Duplicate()
    {
      return DuplicateGoo();
    }
    public AdSecDesignCodeGoo DuplicateGoo()
    {
      return new AdSecDesignCodeGoo(Value == null ? new AdSecDesignCode() : Value.Duplicate());
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
    public override string TypeName => "DesignCode";

    public override string TypeDescription => "AdSec " + this.TypeName + " Parameter";


    #endregion

    #region casting methods
    public override bool CastTo<Q>(ref Q target)
    {
      // This function is called when Grasshopper needs to convert this 
      // instance of GsaBool6 into some other type Q.            


      if (typeof(Q).IsAssignableFrom(typeof(AdSecDesignCode)))
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
      if (typeof(AdSecDesignCode).IsAssignableFrom(source.GetType()))
      {
        Value = (AdSecDesignCode)source;
        return true;
      }

      return false;
    }
    #endregion
  }

  /// <summary>
  /// This class provides a Parameter interface for the Data_GsaBool6 type.
  /// </summary>
  public class AdSecDesignCodeParameter : GH_PersistentParam<AdSecDesignCodeGoo>
  {
    public AdSecDesignCodeParameter()
      : base(new GH_InstanceDescription("DesignCode", "Code", "AdSec DesignCode Parameter", Components.Ribbon.CategoryName.Name(), Components.Ribbon.SubCategoryName.Cat9()))
    {
    }

    public override Guid ComponentGuid => new Guid("6d656276-61f6-47ce-81bc-9fabdd39edc2");

    public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;

    protected override System.Drawing.Bitmap Icon => Properties.Resources.DesignCodeParameter;

    protected override GH_GetterResult Prompt_Plural(ref List<AdSecDesignCodeGoo> values)
    {
      return GH_GetterResult.cancel;
    }
    protected override GH_GetterResult Prompt_Singular(ref AdSecDesignCodeGoo value)
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
