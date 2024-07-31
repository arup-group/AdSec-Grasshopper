using System;
using System.Linq;

using Grasshopper.Kernel.Types;

using Oasys.AdSec.Reinforcement;
using Oasys.AdSec.Reinforcement.Groups;
using Oasys.AdSec.Reinforcement.Preloads;

using OasysGH.Units;

using OasysUnits;

namespace AdSecGH.Parameters {
  public class AdSecRebarGroupGoo : GH_Goo<AdSecRebarGroup> {
    public override bool IsValid => true;
    public override string TypeDescription => "AdSec " + TypeName + " Parameter";
    public override string TypeName => "Rebar Group";
    internal ICover Cover {
      get => Value.Cover;
      set => Value.Cover = value;
    }

    public AdSecRebarGroupGoo() {
    }

    public AdSecRebarGroupGoo(IGroup group) {
      Value = new AdSecRebarGroup(group);
    }

    public AdSecRebarGroupGoo(AdSecRebarGroup goo) {
      if (goo == null) {
        goo = new AdSecRebarGroup();
      }
      Value = goo;
    }

    public override bool CastFrom(object source) {
      if (source == null) {
        return false;
      }

      //Cast from own type
      if (typeof(AdSecRebarGroup).IsAssignableFrom(source.GetType())) {
        Value = (AdSecRebarGroup)source;
        return true;
      }

      return false;
    }

    public override bool CastTo<Q>(ref Q target) {
      if (typeof(Q).IsAssignableFrom(typeof(AdSecRebarGroup))) {
        if (Value == null) {
          target = default;
        } else {
          target = (Q)(object)Value.Duplicate();
        }
        return true;
      }

      target = default;
      return false;
    }

    public override IGH_Goo Duplicate() {
      var dup = new AdSecRebarGroupGoo(Value);
      if (Value.Cover != null) {
        dup.Value.Cover = ICover.Create(Value.Cover.UniformCover);
      }
      return dup;
    }

    //public AdSecRebarGroupGoo Duplicate()
    //{
    //    AdSecRebarGroupGoo dup;
    //    ICover cover;
    //    try
    //    {
    //        // try longitudinal group first
    //        ILongitudinalGroup longitudinal = (ILongitudinalGroup)Value;

    //        try
    //        {
    //            ITemplateGroup temp = (ITemplateGroup)Value;

    //            dup = new AdSecRebarGroupGoo(new ITemplateGroup.)
    //        }
    //        catch (Exception)
    //        {
    //            try
    //            {
    //                IPerimeterGroup perimeter = (IPerimeterGroup)Value;
    //                m_ToString = "Perimeter Group, " + m_cover.UniformCover.ToUnit(DocumentDefaultUnits.LengthUnitGeometry) + " cover";
    //            }
    //            catch (Exception)
    //            {
    //                try
    //                {
    //                    IArcGroup arc = (IArcGroup)Value;
    //                    m_ToString = "Arc Type Layout";
    //                }
    //                catch (Exception)
    //                {
    //                    try
    //                    {
    //                        ICircleGroup cir = (ICircleGroup)Value;
    //                        m_ToString = "Circle Type Layout";
    //                    }
    //                    catch (Exception)
    //                    {
    //                        try
    //                        {
    //                            ILineGroup lin = (ILineGroup)Value;
    //                            m_ToString = "Line Type Layout";
    //                        }
    //                        catch (Exception)
    //                        {
    //                            try
    //                            {
    //                                ISingleBars sin = (ISingleBars)Value;
    //                                m_ToString = "SingleBars Type Layout";
    //                            }
    //                            catch (Exception)
    //                            {
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        // get any preload
    //        if (longitudinal.Preload != null)
    //        {
    //            try
    //            {
    //                IPreForce force = (IPreForce)longitudinal.Preload;

    //            }
    //            catch (Exception)
    //            {
    //                try
    //                {
    //                    IPreStress stress = (IPreStress)longitudinal.Preload;

    //                }
    //                catch (Exception)
    //                {
    //                    IPreStrain strain = (IPreStrain)longitudinal.Preload;

    //                }
    //            }
    //        }
    //    }
    //    catch (Exception)
    //    {
    //        try
    //        {
    //            ILinkGroup link = (ILinkGroup)Value;
    //            m_ToString = "Link, " + m_cover.UniformCover.ToUnit(DocumentDefaultUnits.LengthUnitGeometry) + " cover";
    //        }
    //        catch (Exception)
    //        {
    //        }

    //    }
    //}
    public override string ToString() {
      string m_ToString = "";
      string m_preLoad = "";
      try {
        // try longitudinal group first
        var longitudinal = (ILongitudinalGroup)Value.Group;

        // get any preload
        if (longitudinal.Preload != null) {
          try {
            var force = (IPreForce)longitudinal.Preload;
            if (force.Force.Value != 0) {
              IQuantity quantityForce = new Force(0, DefaultUnits.ForceUnit);
              string unitforceAbbreviation = string.Concat(quantityForce.ToString().Where(char.IsLetter));
              m_preLoad = ", " + Math.Round(force.Force.As(DefaultUnits.ForceUnit), 4) + unitforceAbbreviation + " prestress";
            }
          } catch (Exception) {
            try {
              var stress = (IPreStress)longitudinal.Preload;
              if (stress.Stress.Value != 0) {
                IQuantity quantityStress = new Pressure(0, DefaultUnits.StressUnitResult);
                string unitstressAbbreviation = string.Concat(quantityStress.ToString().Where(char.IsLetter));
                m_preLoad = ", " + Math.Round(stress.Stress.As(DefaultUnits.StressUnitResult), 4) + unitstressAbbreviation + " prestress";
              }
            } catch (Exception) {
              var strain = (IPreStrain)longitudinal.Preload;
              if (strain.Strain.Value != 0) {
                string unitstrainAbbreviation = Strain.GetAbbreviation(DefaultUnits.MaterialStrainUnit);
                m_preLoad = ", " + Math.Round(strain.Strain.As(DefaultUnits.MaterialStrainUnit), 4) + unitstrainAbbreviation + " prestress";
              }
            }
          }
        }

        try {
          var temp = (ITemplateGroup)Value.Group;
          m_ToString = "Template Group, " + Value.Cover.UniformCover.ToUnit(DefaultUnits.LengthUnitGeometry) + " cover";
        } catch (Exception) {
          try {
            var perimeter = (IPerimeterGroup)Value.Group;
            m_ToString = "Perimeter Group, " + Value.Cover.UniformCover.ToUnit(DefaultUnits.LengthUnitGeometry) + " cover";
          } catch (Exception) {
            try {
              var arc = (IArcGroup)Value.Group;
              m_ToString = "Arc Type Layout";
            } catch (Exception) {
              try {
                var cir = (ICircleGroup)Value.Group;
                m_ToString = "Circle Type Layout";
              } catch (Exception) {
                try {
                  var lin = (ILineGroup)Value.Group;
                  m_ToString = "Line Type Layout";
                } catch (Exception) {
                  try {
                    var sin = (ISingleBars)Value.Group;
                    m_ToString = "SingleBars Type Layout";
                  } catch (Exception) {
                  }
                }
              }
            }
          }
        }
      } catch (Exception) {
        try {
          var link = (ILinkGroup)Value.Group;
          m_ToString = "Link, " + Value.Cover.UniformCover.ToUnit(DefaultUnits.LengthUnitGeometry) + " cover";
        } catch (Exception) {
        }
      }
      return "AdSec " + TypeName + " {" + m_ToString + m_preLoad + "}";
    }
  }
}
